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

using static FluentMigrator.Runner.ConsoleUtilities;

namespace FluentMigrator.Runner.Logging
{
    /// <summary>
    /// The default fluent migrator console logger
    /// </summary>
    public class FluentMigratorConsoleLogger : FluentMigratorRunnerLogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigratorConsoleLogger"/> class.
        /// </summary>
        /// <param name="options">The logger options</param>
        public FluentMigratorConsoleLogger(FluentMigratorLoggerOptions options)
            : base(Console.Out, Console.Error, options)
        {
        }

        /// <inheritdoc />
        protected override void WriteHeading(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            base.WriteHeading(message);
            Console.ResetColor();
        }

        /// <inheritdoc />
        protected override void WriteEmphasize(string message)
        {
            AsEmphasize(() => base.WriteEmphasize(message));
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
                Console.WriteLine(sql);
            }
        }

        /// <inheritdoc />
        protected override void WriteEmptySql()
        {
            Console.WriteLine(@"No SQL statement executed.");
        }

        /// <inheritdoc />
        protected override void WriteElapsedTime(TimeSpan timeSpan)
        {
            Console.ResetColor();
            base.WriteElapsedTime(timeSpan);
        }

        /// <inheritdoc />
        protected override void WriteSay(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            base.WriteSay(message);
            Console.ResetColor();
        }

        /// <inheritdoc />
        protected override void WriteError(Exception exception)
        {
            AsError(() => base.WriteError(exception));
        }

        /// <inheritdoc />
        protected override void WriteError(string message)
        {
            AsError(() => base.WriteError(message));
        }
    }
}
