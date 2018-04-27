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

namespace FluentMigrator.Runner.Logging
{
    internal class LogFileFluentMigratorLogger : FluentMigratorRunnerLogger
    {
        private readonly SqlTextWriter _writer;
        private readonly LogFileFluentMigratorLoggerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFileFluentMigratorLogger"/> class.
        /// </summary>
        public LogFileFluentMigratorLogger(
            SqlTextWriter writer,
            LogFileFluentMigratorLoggerOptions options)
            : base(writer, writer, options)
        {
            _writer = writer;
            _options = options;
        }

        /// <inheritdoc />
        protected override void WriteSql(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                _writer.WriteLine("No SQL statement executed.");
            }
            else
            {
                _writer.WriteLineDirect(sql);
                if (_options.OutputGoBetweenStatements)
                    _writer.WriteLineDirect("GO");
            }
        }
    }
}
