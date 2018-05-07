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

using Microsoft.Extensions.Logging;

namespace FluentMigrator.Runner.Logging
{
    /// <summary>
    /// Provider for a <see cref="AnnouncerFluentMigratorLogger"/>
    /// </summary>
    [Obsolete("Used to ease transition to the logging framework")]
    internal class LegacyFluentMigratorLoggerProvider : ILoggerProvider
    {
        private readonly IAnnouncer _announcer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyFluentMigratorLoggerProvider"/> class.
        /// </summary>
        /// <param name="announcer">The announcer to send the messages to</param>
        public LegacyFluentMigratorLoggerProvider(IAnnouncer announcer)
        {
            _announcer = announcer;
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return new AnnouncerFluentMigratorLogger(_announcer);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Nothing to be seen here. Move along.
        }
    }
}
