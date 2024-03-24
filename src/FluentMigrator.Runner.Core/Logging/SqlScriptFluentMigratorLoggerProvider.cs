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

using System.IO;

using Microsoft.Extensions.Logging;

namespace FluentMigrator.Runner.Logging
{
    /// <summary>
    /// The base class for writing SQL scripts produced by the <see cref="IMigrationGenerator"/> implementations
    /// </summary>
    public class SqlScriptFluentMigratorLoggerProvider : ILoggerProvider
    {
        private readonly TextWriter _writer;
        private readonly bool _disposeWriter;
        private readonly SqlTextWriter _sqlWriter;
        private readonly ILogger _logFileLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlScriptFluentMigratorLoggerProvider"/> class.
        /// </summary>
        /// <param name="output">The writer to write the SQL script to</param>
        /// <param name="options">The log file logger options</param>
        /// <param name="disposeWriter">A value indicating whether the <paramref name="output"/> writer should be disposed by this logger provider</param>
        public SqlScriptFluentMigratorLoggerProvider(
            TextWriter output,
            SqlScriptFluentMigratorLoggerOptions options = null,
            bool disposeWriter = true)
        {
            var opt = options ?? new SqlScriptFluentMigratorLoggerOptions() { ShowSql = true };
            _writer = output;
            _disposeWriter = disposeWriter;
            _sqlWriter = new SqlTextWriter(_writer);
            _logFileLogger = new SqlScriptFluentMigratorLogger(_sqlWriter, opt);
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return _logFileLogger;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _sqlWriter?.Dispose();

            if (_disposeWriter)
                _writer?.Dispose();
        }
    }
}
