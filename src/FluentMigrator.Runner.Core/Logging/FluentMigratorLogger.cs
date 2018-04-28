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
    /// The base class for FluentMigrator-style <see cref="ILogger"/> implementations
    /// </summary>
    public abstract class FluentMigratorLogger : ILogger
    {
        private readonly FluentMigratorLoggerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigratorLogger"/> class.
        /// </summary>
        /// <param name="options">The logger options</param>
        protected FluentMigratorLogger(FluentMigratorLoggerOptions options)
        {
            _options = options;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case LogLevel.Error:
                case LogLevel.Critical:
                    if (exception != null)
                    {
                        WriteError(exception);
                    }
                    else
                    {
                        // ReSharper disable once ExpressionIsAlwaysNull
                        WriteError(formatter(state, exception));
                    }

                    break;
                case LogLevel.Warning:
                    if (eventId.Name == RunnerEventIds.RunnerCategory)
                    {
                        WriteEmphasize(formatter(state, exception));
                    }

                    break;
                case LogLevel.Information:
                    if (eventId.Name == RunnerEventIds.RunnerCategory)
                    {
                        if (eventId.Id == RunnerEventIds.Sql.Id)
                        {
                            if (_options.ShowSql)
                            {
                                var sql = formatter(state, exception);
                                if (string.IsNullOrEmpty(sql))
                                {
                                    WriteEmptySql();
                                }
                                else
                                {
                                    WriteSql(sql);
                                }
                            }
                        }
                        else if (eventId.Id == RunnerEventIds.Emphasize.Id)
                        {
                            WriteEmphasize(formatter(state, exception));
                        }
                        else if (eventId.Id == RunnerEventIds.Heading.Id)
                        {
                            WriteHeading(formatter(state, exception));
                        }
                        else if (eventId.Id == RunnerEventIds.ElapsedTime.Id)
                        {
                            if (typeof(TState) == typeof(TimeSpan))
                            {
                                if (_options.ShowElapsedTime)
                                {
                                    WriteElapsedTime((TimeSpan)(object)state);
                                }
                            }
                        }
                        else
                        {
                            WriteSay(formatter(state, exception));
                        }
                    }
                    else
                    {
                        WriteSay(formatter(state, exception));
                    }

                    break;
            }
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Information;
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        /// <summary>
        /// Writes an error message
        /// </summary>
        /// <param name="message">The error message</param>
        protected abstract void WriteError(string message);

        /// <summary>
        /// Writes an exception message
        /// </summary>
        /// <param name="exception">The exception containing the message</param>
        protected abstract void WriteError(Exception exception);

        /// <summary>
        /// Writes a heading
        /// </summary>
        /// <param name="message">The heading message</param>
        protected abstract void WriteHeading(string message);

        /// <summary>
        /// Writes an emphasized text
        /// </summary>
        /// <param name="message">The message to write</param>
        protected abstract void WriteEmphasize(string message);

        /// <summary>
        /// Writes an SQL statement
        /// </summary>
        /// <param name="sql">The SQL statement</param>
        protected abstract void WriteSql(string sql);

        /// <summary>
        /// Called when an attempt was made to write an empty SQL statement
        /// </summary>
        protected abstract void WriteEmptySql();

        /// <summary>
        /// Writes the elapsed time
        /// </summary>
        /// <param name="timeSpan">The elapsed time</param>
        protected abstract void WriteElapsedTime(TimeSpan timeSpan);

        /// <summary>
        /// Writes a message
        /// </summary>
        /// <param name="message">The message</param>
        protected abstract void WriteSay(string message);

        private class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new NoopDisposable();

            public void Dispose()
            {
            }
        }
    }
}
