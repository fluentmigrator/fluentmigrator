#region License
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.BatchParser
{
    /// <summary>
    /// The main class to perform SQL batch collection
    /// </summary>
    internal class SearchStatus
    {
        [NotNull]
        private readonly SearchContext _context;

        [NotNull]
        private readonly ILineReader _reader;

        [NotNull, ItemNotNull]
        private readonly Stack<IRangeSearcher> _activeRanges;

        [CanBeNull]
        private readonly SpecialTokenInfo _foundToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchStatus"/> class.
        /// </summary>
        /// <param name="context">The search context</param>
        /// <param name="reader">The reader to be read from</param>
        public SearchStatus(
            [NotNull] SearchContext context,
            [NotNull] ILineReader reader)
            : this(context, reader, new Stack<IRangeSearcher>(), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchStatus"/> class
        /// </summary>
        /// <param name="context">The search context</param>
        /// <param name="reader">The reader to be read from</param>
        /// <param name="activeRanges">The stack of active ranges</param>
        /// <param name="foundToken">The found special token</param>
        private SearchStatus(
            [NotNull] SearchContext context,
            [NotNull] ILineReader reader,
            [NotNull, ItemNotNull]Stack<IRangeSearcher> activeRanges,
            [CanBeNull] SpecialTokenInfo foundToken)
        {
            _context = context;
            _reader = reader;
            _activeRanges = activeRanges;
            _foundToken = foundToken;
        }

        /// <summary>
        /// Tries to find the next token or range
        /// </summary>
        /// <returns><c>null</c> when no token or range could be found</returns>
        [CanBeNull]
        public SearchStatus Process()
        {
            if (_activeRanges.Count == 0)
                return FindTokenOrRangeStart();

            return FindRangeEnd();
        }

        /// <summary>
        /// Search a special token
        /// </summary>
        /// <param name="reader">The reader where the token should be searched in</param>
        /// <param name="searchers">The collection of searchers to test</param>
        /// <returns><c>null</c> when no token could be found</returns>
        [CanBeNull]
        private static SpecialTokenInfo FindToken([NotNull] ILineReader reader, [NotNull, ItemNotNull] IEnumerable<ISpecialTokenSearcher> searchers)
        {
            SpecialTokenInfo result = null;
            foreach (var searcher in searchers)
            {
                var searcherResult = searcher.Find(reader);
                if (searcherResult != null && (result == null || result.Index > searcherResult.Index))
                {
                    result = searcherResult;
                }
            }

            return result;
        }

        /// <summary>
        /// Search for the start of a range
        /// </summary>
        /// <param name="reader">The reader where the range start token should be searched in</param>
        /// <param name="searchers">The collection of searchers to test</param>
        /// <returns><c>null</c> when no range could be found</returns>
        [CanBeNull]
        private static RangeStart FindRangeStart([NotNull] ILineReader reader, [NotNull, ItemNotNull] IEnumerable<IRangeSearcher> searchers)
        {
            RangeStart result = null;
            foreach (var searcher in searchers)
            {
                var index = searcher.FindStartCode(reader);
                if (index != -1 && (result == null || result.Index > index))
                {
                    result = new RangeStart(searcher, index);
                }
            }

            return result;
        }

        /// <summary>
        /// Search for the end of a range
        /// </summary>
        /// <returns><c>null</c> when no range end token could be found</returns>
        [CanBeNull]
        private SearchStatus FindRangeEnd()
        {
            Debug.Assert(
                _activeRanges.Count != 0 || _foundToken == null,
                "Operation can only be performed when a range is active and no token has been found");

            var searcher = _activeRanges.Peek();
            var result = searcher.FindEndCode(_reader);
            if (result == null)
            {
                var reader = _reader;
                if (_context.StripComments && searcher.IsComment)
                {
                    reader = reader.Advance(reader.Length);
                }
                else
                {
                    reader = WriteSql(reader);
                }

                if (reader == null)
                    throw new InvalidOperationException($"Missing end of range ({searcher.GetType().Name})");
                return new SearchStatus(_context, reader, _activeRanges, null);
            }

            if (result.IsNestedStart)
            {
                var reader = _reader;

                if (_context.StripComments && searcher.IsComment)
                {
                    reader = reader.Advance(result.Index - reader.Index);
                    Debug.Assert(reader != null, "The returned ILineReader must not be null");
                }

                Debug.Assert(result.NestedRangeSearcher != null, "result.NestedRangeSearcher != null");
                return UseNewRange(reader, new RangeStart(result.NestedRangeSearcher, result.Index));
            }

            var nextReader = WriteSql(_reader, searcher, result);
            if (nextReader == null)
                return null;

            _activeRanges.Pop();
            return new SearchStatus(_context, nextReader, _activeRanges, null);
        }

        /// <summary>
        /// Search for a token or range start token
        /// </summary>
        /// <remarks>
        /// In other words: Search for everything that is allowed outside of a range.
        /// </remarks>
        /// <returns><c>null</c> if neither a token nor a range start sequence could be found</returns>
        [CanBeNull]
        private SearchStatus FindTokenOrRangeStart()
        {
            Debug.Assert(_activeRanges.Count == 0, "Operation can only be performed when no range is active");

            var rangeStart = FindRangeStart(_reader, _context.RangeSearchers);
            var tokenInfo = FindToken(_reader, _context.SpecialTokenSearchers);
            if (tokenInfo != null)
            {
                if (rangeStart == null || rangeStart.Index > tokenInfo.Index)
                {
                    var reader = WriteSql(_reader, tokenInfo.Index, tokenInfo.Length);
                    _context.OnSpecialToken(new SpecialTokenEventArgs(tokenInfo.Token, tokenInfo.Opaque));
                    if (reader == null)
                        return null;
                    return new SearchStatus(_context, reader, _activeRanges, tokenInfo);
                }
            }
            else if (rangeStart == null)
            {
                var reader = WriteSql(_reader);
                if (reader == null)
                    return null;
                return new SearchStatus(_context, reader, _activeRanges, null);
            }

            return UseNewRange(_reader, rangeStart);
        }

        /// <summary>
        /// Handle the case where a new range start sequence was found
        /// </summary>
        /// <param name="reader">The reader where the sequence was found</param>
        /// <param name="info">Information about the start sequence</param>
        /// <returns>A new search status</returns>
        [NotNull]
        private SearchStatus UseNewRange([NotNull] ILineReader reader, [NotNull] RangeStart info)
        {
            var nextReader = WriteSql(reader, info);
            if (nextReader == null)
                throw new InvalidOperationException($"Missing end of range ({info.Searcher.GetType().Name})");

            _activeRanges.Push(info.Searcher);
            return new SearchStatus(_context, nextReader, _activeRanges, null);
        }

        [CanBeNull]
        private ILineReader WriteSql([NotNull] ILineReader reader)
        {
            var content = reader.ReadString(reader.Length);
            _context.OnBatchSql(new SqlBatchCollectorEventArgs(content, true));
            return reader.Advance(reader.Length);
        }

        [CanBeNull]
        private ILineReader WriteSql([NotNull] ILineReader reader, [NotNull] RangeStart info)
        {
            if (info.Searcher.IsComment && _context.StripComments)
                return WriteSql(reader, info.Index, info.Searcher.StartCodeLength);
            return WriteSql(reader, info.Index + info.Searcher.StartCodeLength);
        }

        [CanBeNull]
        private ILineReader WriteSql([NotNull] ILineReader reader, [NotNull] IRangeSearcher searcher, [NotNull] EndCodeSearchResult info)
        {
            if (searcher.IsComment && _context.StripComments)
            {
                var length = info.Index - reader.Index + searcher.EndCodeLength;
                if (length == reader.Length)
                    _context.OnBatchSql(new SqlBatchCollectorEventArgs(string.Empty, true));
                return reader.Advance(length);
            }

            return WriteSql(_reader, info.Index + searcher.EndCodeLength);
        }

        [CanBeNull]
        private ILineReader WriteSql([NotNull] ILineReader reader, int itemIndex, int skipLength = 0)
        {
            var readLength = itemIndex - reader.Index;
            var content = reader.ReadString(readLength);
            var isEndOfLine = readLength == reader.Length;
            if (!string.IsNullOrEmpty(content) || isEndOfLine)
            {
                _context.OnBatchSql(new SqlBatchCollectorEventArgs(content, isEndOfLine));
            }

            return reader.Advance(readLength + skipLength);
        }

        private class RangeStart
        {
            public RangeStart([NotNull] IRangeSearcher searcher, int index)
            {
                Searcher = searcher;
                Index = index;
            }

            [NotNull]
            public IRangeSearcher Searcher { get; }

            public int Index { get; }
        }
    }
}
