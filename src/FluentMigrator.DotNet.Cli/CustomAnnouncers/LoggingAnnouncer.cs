#region License
// Copyright (c) 2007-2018, Sean Chambers and the FluentMigrator Project
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

using FluentMigrator.Runner;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.DotNet.Cli.CustomAnnouncers
{
    [Obsolete]
    public class LoggingAnnouncer : IAnnouncer
    {
        private readonly ILogger<LoggingAnnouncer> _logger;
        private readonly CustomAnnouncerOptions _options;

        public LoggingAnnouncer(ILogger<LoggingAnnouncer> logger, IOptions<CustomAnnouncerOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public bool ShowElapsedTime => _options.ShowElapsedTime;

        public bool ShowSql => _options.ShowSql;

        public void Heading(string message)
        {
            _logger.LogHeader(message);
        }

        public void Say(string message)
        {
            _logger.LogSay(message);
        }

        public void Emphasize(string message)
        {
            _logger.LogEmphasized(message);
        }

        public void Sql(string sql)
        {
            if (!_options.ShowSql)
                return;

            if (string.IsNullOrEmpty(sql))
            {
                _logger.LogSay("No SQL statement executed.");
            }
            else
            {
                _logger.LogSql(sql);
            }
        }

        public void ElapsedTime(TimeSpan timeSpan)
        {
            if (!_options.ShowElapsedTime)
                return;

            _logger.LogElapsedTime(timeSpan);
        }

        public void Error(string message)
        {
            _logger.LogError(message);
        }

        public void Error(Exception exception)
        {
            _logger.LogError(exception, exception.Message);
        }

        public void Write(string message, bool isNotSql = true)
        {
            _logger.LogSay(message);
        }
    }
}
