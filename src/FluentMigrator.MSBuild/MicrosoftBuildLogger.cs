#region License
// Copyright (c) 2024, Fluent Migrator Project
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

using Microsoft.Build.Framework;

using Microsoft.Build.Utilities;
using Microsoft.Extensions.Logging;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace FluentMigrator.MSBuild
{
    /// <summary>
    /// A Serilog sink that redirects events to MSBuild.
    /// </summary>
    public class MicrosoftBuildLogger : ILogger
    {
        private readonly TaskLoggingHelper _loggingHelper;

        /// <summary>
        /// Creates an <see cref="MicrosoftBuildLogger"/> from an <see cref="ITask"/>.
        /// </summary>
        /// <param name="task">The <see cref="ITask"/> inside which events are sent.</param>
        public MicrosoftBuildLogger(ITask task)
        {
            if (task is null)
                ThrowArgumentNullException(nameof(task));
            _loggingHelper = task is Task taskConcrete ? taskConcrete.Log : new TaskLoggingHelper(task);
        }

        /// <summary>
        /// Creates an <see cref="MicrosoftBuildLogger"/> from a <see cref="TaskLoggingHelper"/>.
        /// </summary>
        /// <param name="loggingHelper">The <see cref="TaskLoggingHelper"/> to which events are sent.</param>
        public MicrosoftBuildLogger(TaskLoggingHelper loggingHelper)
        {
            if (loggingHelper is null)
                ThrowArgumentNullException(nameof(loggingHelper));
            _loggingHelper = loggingHelper;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            switch (logLevel)
            {
                case LogLevel.Trace:
                    _loggingHelper.LogMessage(GetMessageImportance(logLevel), $"{formatter(state, exception)}");
                    break;
                case LogLevel.Debug:
                    _loggingHelper.LogMessage(GetMessageImportance(logLevel), $"{formatter(state, exception)}");
                    break;
                case LogLevel.Information:
                    _loggingHelper.LogMessage(GetMessageImportance(logLevel), $"{formatter(state, exception)}");
                    break;
                case LogLevel.Warning:
                    LogWarning(state, exception, formatter);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    LogError(state, exception, formatter);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private MessageImportance GetMessageImportance(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace: return MessageImportance.Low;
                case LogLevel.Debug: return MessageImportance.Normal;
                case LogLevel.Information: return MessageImportance.High;
                case LogLevel.Warning: return MessageImportance.High;
                case LogLevel.Error: return MessageImportance.High;
                case LogLevel.Critical: return MessageImportance.High;
                default: return MessageImportance.High;
            }
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return _loggingHelper.LogsMessagesOfImportance(GetMessageImportance(logLevel));
        }

        public IDisposable BeginScope<TState>(TState state) => default;

        private static void ThrowArgumentNullException(string paramName) =>
            throw new ArgumentNullException(paramName);

        // MSBuild versions earlier than 16.8 do not support help links, so we will try to call the new overload and if
        // it does not exist we will catch the MissingMethodException and call the old overload.
        private void LogWarning<TState>(TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _loggingHelper.LogWarning($"{formatter(state, exception)}");
        }

        private void LogError<TState>(TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _loggingHelper.LogError($"{formatter(state, exception)}");
        }
    }
}
