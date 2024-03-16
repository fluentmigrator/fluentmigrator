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

namespace FluentMigrator.Runner.Logging
{
    /// <summary>
    /// Logger that provides some default formatting
    /// </summary>
    public class FluentMigratorRunnerLogger : FluentMigratorLogger
    {
        private readonly TextWriter _output;
        private readonly TextWriter _error;

        /// <inheritdoc />
        public FluentMigratorRunnerLogger(TextWriter output, TextWriter error, FluentMigratorLoggerOptions options)
            : base(options)
        {
            _output = output;
            _error = error;
        }

        /// <inheritdoc />
        protected override void WriteError(Exception exception)
        {
            _error.WriteException(exception);
        }

        /// <inheritdoc />
        protected override void WriteHeading(string message)
        {
            _output.WriteHorizontalRuler();
            _output.WriteLine(message);
            _output.WriteHorizontalRuler();
        }

        /// <inheritdoc />
        protected override void WriteEmphasize(string message)
        {
            _output.WriteLine($"[+] {message}");
        }

        /// <inheritdoc />
        protected override void WriteSql(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                WriteEmptySql();
            }
            else
            {
                _output.WriteLine(sql);
            }
        }

        /// <inheritdoc />
        protected override void WriteEmptySql()
        {
            _output.WriteLine("No SQL statement executed.");
        }

        /// <inheritdoc />
        protected override void WriteSay(string message)
        {
            _output.WriteLine($"{message}");
        }

        /// <inheritdoc />
        protected override void WriteElapsedTime(TimeSpan timeSpan)
        {
            _output.WriteLine($"=> {timeSpan.TotalSeconds}s");
        }

        /// <inheritdoc />
        protected override void WriteError(string message)
        {
            _output.WriteExceptionMessage(message);
        }
    }
}
