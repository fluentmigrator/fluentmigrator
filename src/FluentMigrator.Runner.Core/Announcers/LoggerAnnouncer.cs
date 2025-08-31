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

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Announcers
{
    /// <summary>
    /// Implementation of <see cref="IAnnouncer"/> that redirects all log messages to an <see cref="ILogger"/>.
    /// </summary>
    [Obsolete]
    public class LoggerAnnouncer : IAnnouncer
    {
        [NotNull]
        private readonly ILogger _logger;

        [NotNull]
        private readonly AnnouncerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerAnnouncer"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory to create the logger from</param>
        /// <param name="options">The announcer options</param>
        public LoggerAnnouncer([NotNull] ILoggerFactory loggerFactory, [NotNull] IOptions<AnnouncerOptions> options)
        {
            _logger = loggerFactory.CreateLogger(RunnerEventIds.RunnerCategory);
            _options = options.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerAnnouncer"/> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options">The announcer options</param>
        public LoggerAnnouncer([NotNull] ILogger logger, [NotNull] AnnouncerOptions options)
        {
            _logger = logger;
            _options = options;
        }

        /// <inheritdoc />
        public void Heading(string message)
        {
            _logger.LogHeader(message);
        }

        /// <inheritdoc />
        public void Say(string message)
        {
            _logger.LogSay(message);
        }

        /// <inheritdoc />
        public void Emphasize(string message)
        {
            _logger.LogEmphasized(message);
        }

        /// <inheritdoc />
        public void Sql(string sql)
        {
            if (_options.ShowSql)
                _logger.LogSql(sql);
        }

        /// <inheritdoc />
        public void ElapsedTime(TimeSpan timeSpan)
        {
            if (_options.ShowElapsedTime)
            {
                _logger.LogElapsedTime(timeSpan);
            }
        }

        /// <inheritdoc />
        public void Error(string message)
        {
            _logger.LogError(message);
        }

        /// <inheritdoc />
        public void Error(Exception exception)
        {
            _logger.LogError(exception, exception.Message);
        }

        /// <inheritdoc />
        public void Write(string message, bool isNotSql = true)
        {
            if (isNotSql)
            {
                Say(message);
            }
            else if (_options.ShowSql)
            {
                Sql(message);
            }
        }
    }
}
