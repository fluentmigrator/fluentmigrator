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

using Microsoft.Extensions.Logging;

namespace FluentMigrator.Runner.Logging
{
    /// <summary>
    /// A <see cref="ILogger"/> implementation that redirects all messages to a <see cref="IAnnouncer"/>
    /// </summary>
    [Obsolete]
    public class AnnouncerFluentMigratorLogger : FluentMigratorLogger
    {
        private readonly IAnnouncer _announcer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnouncerFluentMigratorLogger"/> class.
        /// </summary>
        /// <param name="announcer">The announcer to send all messages to</param>
        public AnnouncerFluentMigratorLogger(IAnnouncer announcer)
            : base(new FluentMigratorLoggerOptions() { ShowElapsedTime = true, ShowSql = true })
        {
            _announcer = announcer;
        }

        /// <inheritdoc />
        protected override void WriteError(string message)
        {
            _announcer.Error(message);
        }

        /// <inheritdoc />
        protected override void WriteError(Exception exception)
        {
            _announcer.Error(exception);
        }

        /// <inheritdoc />
        protected override void WriteHeading(string message)
        {
            _announcer.Heading(message);
        }

        /// <inheritdoc />
        protected override void WriteEmphasize(string message)
        {
            _announcer.Emphasize(message);
        }

        /// <inheritdoc />
        protected override void WriteSql(string sql)
        {
            _announcer.Sql(sql);
        }

        /// <inheritdoc />
        protected override void WriteEmptySql()
        {
            _announcer.Sql(string.Empty);
        }

        /// <inheritdoc />
        protected override void WriteElapsedTime(TimeSpan timeSpan)
        {
            _announcer.ElapsedTime(timeSpan);
        }

        /// <inheritdoc />
        protected override void WriteSay(string message)
        {
            _announcer.Say(message);
        }
    }
}
