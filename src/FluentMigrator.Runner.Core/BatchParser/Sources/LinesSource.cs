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

using JetBrains.Annotations;

namespace FluentMigrator.Runner.BatchParser.Sources
{
    /// <summary>
    /// A <see cref="ITextSource"/> implementation that uses lines as input
    /// </summary>
    public class LinesSource : ITextSource
    {
        [NotNull, ItemNotNull]
        private readonly IEnumerable<string> _batchSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinesSource"/> class.
        /// </summary>
        /// <param name="batchSource">The collection of lines to be used as source</param>
        public LinesSource([NotNull, ItemNotNull] IEnumerable<string> batchSource)
        {
            _batchSource = batchSource;
        }

        /// <inheritdoc />
        public ILineReader CreateReader()
        {
            var enumerator = _batchSource.GetEnumerator();
            if (!enumerator.MoveNext())
                return null;
            return new LineReader(enumerator, 0);
        }

        private class LineReader : ILineReader
        {
            [NotNull]
            private readonly IEnumerator<string> _enumerator;

            public LineReader([NotNull] IEnumerator<string> enumerator, int index)
            {
                _enumerator = enumerator;
                Index = index;
                Line = _enumerator.Current ?? throw new InvalidOperationException("The returned line must not be null");
            }

            public string Line { get; private set; }

            public int Index { get; }

            public int Length => Line.Length - Index;

            public string ReadString(int length)
            {
                return Line.Substring(Index, length);
            }

            public ILineReader Advance(int length)
            {
                var currentLine = Line;
                var currentIndex = Index;
                var remaining = currentLine.Length - currentIndex;

                if (length >= remaining)
                {
                    do
                    {
                        length -= remaining;
                        if (!_enumerator.MoveNext())
                            return null;

                        currentIndex = 0;
                        currentLine = _enumerator.Current ?? throw new InvalidOperationException("The returned line must not be null");
                        remaining = currentLine.Length;
                    } while (length >= remaining && length != 0);
                }

                Line = currentLine;
                currentIndex += length;
                return new LineReader(_enumerator, currentIndex);
            }
        }
    }
}
