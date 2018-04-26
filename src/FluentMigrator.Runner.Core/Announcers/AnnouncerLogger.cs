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

namespace FluentMigrator.Runner.Announcers
{
    /// <summary>
    /// A <see cref="ILogger"/> implementation that redirects all messages to a <see cref="IAnnouncer"/>
    /// </summary>
    [Obsolete]
    public class AnnouncerLogger : ILogger
    {
        private readonly IAnnouncer _announcer;

        public AnnouncerLogger(IAnnouncer announcer)
        {
            _announcer = announcer;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case LogLevel.Error:
                case LogLevel.Critical:
                    _announcer.Error(formatter(state, exception));
                    break;
                case LogLevel.Warning:
                    _announcer.Emphasize(formatter(state, exception));
                    break;
                case LogLevel.Information:
                    if (eventId.Name == RunnerEventIds.DefaultEventName)
                    {
                        if (eventId.Id == RunnerEventIds.Sql.Id)
                        {
                            _announcer.Sql(formatter(state, exception));
                        }
                        else if (eventId.Id == RunnerEventIds.Emphasize.Id)
                        {
                            _announcer.Emphasize(formatter(state, exception));
                        }
                        else if (eventId.Id == RunnerEventIds.Heading.Id)
                        {
                            _announcer.Heading(formatter(state, exception));
                        }
                        else if (eventId.Id == RunnerEventIds.ElapsedTime.Id)
                        {
                            _announcer.Write(formatter(state, exception));
                        }
                        else
                        {
                            _announcer.Say(formatter(state, exception));
                        }
                    }
                    else
                    {
                        _announcer.Say(formatter(state, exception));
                    }

                    break;
                case LogLevel.Debug:
                case LogLevel.Trace:
                    _announcer.Say(formatter(state, exception));
                    break;
            }
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        private class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new NoopDisposable();

            public void Dispose()
            {
            }
        }
    }
}
