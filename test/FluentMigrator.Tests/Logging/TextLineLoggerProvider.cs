#region License
// Copyright (c) 2018, FluentMigrator Project
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
using System.IO;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Logging;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FluentMigrator.Tests.Logging
{
    /// <summary>
    /// Writes all messages in a collection of strings
    /// </summary>
    public class TextLineLoggerProvider : ILoggerProvider
    {
        [NotNull, ItemNotNull]
        private readonly ICollection<string> _lines;

        [NotNull]
        private readonly FluentMigratorLoggerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextLineLoggerProvider"/> class.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="options"></param>
        public TextLineLoggerProvider([NotNull, ItemNotNull] ICollection<string> lines, FluentMigratorLoggerOptions options = null)
        {
            _lines = lines;
            _options = options ?? new FluentMigratorLoggerOptions();
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return new TextLineLogger(_lines, _options);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        private class TextLineLogger : FluentMigratorLogger
        {
            [NotNull]
            [ItemNotNull]
            private readonly ICollection<string> _lines;

            [NotNull]
            private readonly FluentMigratorLoggerOptions _options;

            /// <inheritdoc />
            public TextLineLogger([NotNull, ItemNotNull] ICollection<string> lines, [NotNull] FluentMigratorLoggerOptions options)
                : base(options)
            {
                _lines = lines;
                _options = options;
            }

            /// <inheritdoc />
            protected override void WriteError(string message)
            {
                AddLines(writer => writer.WriteExceptionMessage(message));
            }

            /// <inheritdoc />
            protected override void WriteError(Exception exception)
            {
                AddLines(writer => writer.WriteException(exception));
            }

            /// <inheritdoc />
            protected override void WriteHeading(string message)
            {
                _lines.Add(message);
            }

            /// <inheritdoc />
            protected override void WriteEmphasize(string message)
            {
                _lines.Add($"[+] {message}");
            }

            /// <inheritdoc />
            protected override void WriteSql(string sql)
            {
                if (_options.ShowSql)
                    _lines.Add(sql);
            }

            /// <inheritdoc />
            protected override void WriteEmptySql()
            {
                _lines.Add("No SQL statement executed.");
            }

            /// <inheritdoc />
            protected override void WriteElapsedTime(TimeSpan timeSpan)
            {
                if (_options.ShowElapsedTime)
                    _lines.Add($"=> {timeSpan.TotalSeconds}s");
            }

            /// <inheritdoc />
            protected override void WriteSay(string message)
            {
                _lines.Add($"[+] {message}");
            }

            private void AddLines(Action<TextWriter> writeAction)
            {
                string output;

                using (var writer = new StringWriter())
                {
                    writeAction(writer);

                    output = writer.ToString();
                }

                var emptyLinesCount = 0;
                using (var reader = new StringReader(output))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(line))
                        {
                            emptyLinesCount += 1;
                        }
                        else
                        {
                            for (var i = 0; i < emptyLinesCount; i++)
                            {
                                _lines.Add(string.Empty);
                            }

                            _lines.Add(line);
                        }
                    }
                }

                if (emptyLinesCount > 1)
                {
                    for (int i = 1; i < emptyLinesCount; i++)
                    {
                        _lines.Add(string.Empty);
                    }
                }
            }
        }
    }
}
