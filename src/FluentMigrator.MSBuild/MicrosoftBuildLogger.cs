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
using System.Runtime.CompilerServices;

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

        private static bool _supportsLogsMessagesOfImportance = true;

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
                    _loggingHelper.LogMessage(MessageImportance.Low, formatter(state, exception));
                    break;
                case LogLevel.Debug:
                    _loggingHelper.LogMessage(MessageImportance.Normal, formatter(state, exception));
                    break;
                case LogLevel.Information:
                    _loggingHelper.LogMessage(MessageImportance.High, formatter(state, exception));
                    break;
                case LogLevel.Warning:
                    _loggingHelper.LogWarning(formatter(state, exception));
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    _loggingHelper.LogError(formatter(state, exception));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            MessageImportance importance;

            switch (logLevel)
            {
                case LogLevel.Information:
                    importance = MessageImportance.High;
                    break;
                case LogLevel.Debug:
                    importance = MessageImportance.Normal;
                    break;
                case LogLevel.Trace:
                    importance = MessageImportance.Low;
                    break;
                case LogLevel.Warning:
                case LogLevel.Error:
                case LogLevel.Critical:
                    // These are always logged.
                    return true;
                default:
                    // Do not log LogLevel.None or invalid.
                    return false;
            }

            if (_supportsLogsMessagesOfImportance)
            {
                // MSBuild versions earlier than 17 do not have LogsMessagesOfImportance, so we will try to call it
                // and if it does not exist we will swallow the MissingMethodException and return true.
                try
                {
                    return LogsMessagesOfImportance_17(_loggingHelper, importance);
                }
                catch (MissingMethodException)
                {
                    // Mark that the method does not exist and don't try to call it again, to avoid the overhead of exceptions.
                    _supportsLogsMessagesOfImportance = false;
                }
            }

            return true;

            [MethodImpl(MethodImplOptions.NoInlining)]
            static bool LogsMessagesOfImportance_17(TaskLoggingHelper loggingHelper, MessageImportance importance)
            {
                return loggingHelper.LogsMessagesOfImportance(importance);
            }
        }

        public IDisposable BeginScope<TState>(TState state) => default;

        private static void ThrowArgumentNullException(string paramName) =>
            throw new ArgumentNullException(paramName);
    }
}
