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
using System.IO;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.BatchParser.Sources
{
    /// <summary>
    /// A <see cref="ITextSource"/> implementation that uses a <see cref="TextReader"/> as source.
    /// </summary>
    public class TextReaderSource : ITextSource, IDisposable
    {
        private readonly TextReader _reader;
        private readonly bool _isOwner;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextReaderSource"/> class.
        /// </summary>
        /// <param name="reader">The text reader to use</param>
        /// <remarks>
        /// This function doesn't take ownership of the <paramref name="reader"/>.
        /// </remarks>
        public TextReaderSource([NotNull] TextReader reader)
            : this(reader, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextReaderSource"/> class.
        /// </summary>
        /// <param name="reader">The text reader to use</param>
        /// <param name="takeOwnership"><c>true</c> when the <see cref="TextReaderSource"/> should become the owner of the <paramref name="reader"/></param>
        public TextReaderSource([NotNull] TextReader reader, bool takeOwnership)
        {
            _reader = reader;
            _isOwner = takeOwnership;
        }

        /// <inheritdoc />
        public ILineReader CreateReader()
        {
            var currentLine = _reader.ReadLine();
            if (currentLine == null)
            {
                return null;
            }

            return new LineReader(_reader, currentLine, 0);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_isOwner)
            {
                _reader.Dispose();
            }
        }

        private class LineReader : ILineReader
        {
            [NotNull]
            private readonly TextReader _reader;

            public LineReader([NotNull] TextReader reader, [NotNull] string currentLine, int index)
            {
                _reader = reader;
                Line = currentLine;
                Index = index;
            }

            public string Line { get; }

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
                        var line = _reader.ReadLine();
                        if (line == null)
                            return null;
                        currentIndex = 0;
                        currentLine = line;
                        remaining = currentLine.Length;
                    } while (length >= remaining && length != 0);
                }

                currentIndex += length;
                return new LineReader(_reader, currentLine, currentIndex);
            }
        }
    }
}
