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
using System.Text;

using Microsoft.Extensions.Logging;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Utility functions around logging
    /// </summary>
    public static class LoggingUtilities
    {
        /// <summary>
        /// Log elapsed time
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="timeSpan">The elapsed time</param>
        public static void LogElapsedTime(this ILogger logger, TimeSpan timeSpan)
        {
            logger.Log(LogLevel.Information, RunnerEventIds.ElapsedTime, timeSpan, null, (ts, ex) => $"=> {ts.TotalSeconds}s");
        }

        /// <summary>
        /// Log emphasized message
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="message">The message</param>
        public static void LogEmphasized(this ILogger logger, string message)
        {
            logger.LogWarning(RunnerEventIds.Emphasize, message);
        }

        /// <summary>
        /// Log header message
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="message">The message</param>
        public static void LogHeader(this ILogger logger, string message)
        {
            logger.LogInformation(RunnerEventIds.Heading, message);
        }

        /// <summary>
        /// Log SQL code
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="sql">The SQL code</param>
        public static void LogSql(this ILogger logger, string sql)
        {
            logger.LogInformation(RunnerEventIds.Sql, sql);
        }

        /// <summary>
        /// Log an informational message
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="message">The message</param>
        public static void LogSay(this ILogger logger, string message)
        {
            logger.LogInformation(RunnerEventIds.Say, message);
        }

        /// <summary>
        /// Writes a horizontal ruler to the given <paramref name="writer"/>
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the text to</param>
        public static void WriteHorizontalRuler(this TextWriter writer)
        {
            writer.WriteLine("".PadRight(79, '-'));
        }

        /// <summary>
        /// Writes the header to the given <paramref name="writer"/>
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the text to</param>
        public static void WriteHeader(this TextWriter writer)
        {
            writer.WriteHorizontalRuler();
            writer.WriteLine("=============================== FluentMigrator ================================");
            writer.WriteHorizontalRuler();
            writer.WriteLine("Source Code:");
            writer.WriteLine("  https://github.com/fluentmigrator/fluentmigrator");
            writer.WriteLine("Ask For Help:");
            writer.WriteLine("  https://github.com/fluentmigrator/fluentmigrator/discussions");
            writer.WriteHorizontalRuler();
        }

        /// <summary>
        /// Writes the exception message to the given <paramref name="writer"/>
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the text to</param>
        /// <param name="message">The exception message</param>
        /// <param name="level">A value > 0 when this exception is an inner exception</param>
        public static void WriteExceptionMessage(this TextWriter writer, string message, int level = 0)
        {
            // Indicate the dependencies of the inner exceptions
            var indent = new StringBuilder();
            if (level != 0)
            {
                for (var i = 0; i != level - 1; ++i)
                {
                    indent.Append("|  ");
                }

                indent.Append("+- ");
            }

            writer.WriteLine($"!!! {indent}{message}");
        }

        /// <summary>
        /// Writes the exception (and all its inner exceptions) to the given <paramref name="writer"/>
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the text to</param>
        /// <param name="exception">The exception containing the message</param>
        /// <param name="level">A value > 0 when this exception is an inner exception</param>
        public static void WriteException(this TextWriter writer, Exception exception, int level = 0)
        {
            while (exception != null)
            {
                writer.WriteExceptionMessage(exception.Message, level);
                level += 1;
                if (exception is AggregateException aggregateException)
                {
                    foreach (var innerException in aggregateException.Flatten().InnerExceptions)
                    {
                        writer.WriteExceptionMessage(innerException.Message, level);
                    }

                    exception = null;
                }
                else
                {
                    exception = exception.InnerException;
                }
            }
        }
    }
}
