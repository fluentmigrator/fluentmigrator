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

using System.Text.RegularExpressions;

namespace FluentMigrator.Runner.BatchParser.RangeSearchers
{
    /// <summary>
    /// An example implementation of a nested multi-line comment (e.g. <c>/* comment /* nested */ */</c>)
    /// </summary>
    public sealed class NestingMultiLineComment : IRangeSearcher
    {
        private readonly Regex _startCodeRegex;
        private readonly Regex _endCodeRegex;

        /// <summary>
        /// Initializes a new instance of the <see cref="NestingMultiLineComment"/> class.
        /// </summary>
        public NestingMultiLineComment()
        {
            var startCode = "/*";
            var endCode = "*/";

            StartCodeLength = startCode.Length;
            EndCodeLength = endCode.Length;

            var startCodePattern = Regex.Escape(startCode);
            var endCodePattern = Regex.Escape(endCode);

            _startCodeRegex = new Regex(startCodePattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);
            _endCodeRegex = new Regex(endCodePattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);
        }

        /// <inheritdoc />
        public int StartCodeLength { get; }

        /// <inheritdoc />
        public int EndCodeLength { get; }

        /// <inheritdoc />
        public bool IsComment => true;

        /// <inheritdoc />
        public int FindStartCode(ILineReader reader)
        {
            var match = _startCodeRegex.Match(reader.Line, reader.Index);
            if (!match.Success)
                return -1;
            return match.Index;
        }

        /// <inheritdoc />
        public EndCodeSearchResult FindEndCode(ILineReader reader)
        {
            var matchStart = _startCodeRegex.Match(reader.Line, reader.Index);
            var matchEnd = _endCodeRegex.Match(reader.Line, reader.Index);
            if (!matchEnd.Success && !matchStart.Success)
                return null;
            if (!matchStart.Success)
                return matchEnd.Index;
            if (!matchEnd.Success)
                return new EndCodeSearchResult(matchStart.Index, this);
            if (matchStart.Index < matchEnd.Index)
                return new EndCodeSearchResult(matchStart.Index, this);
            return matchEnd.Index;
        }
    }
}
