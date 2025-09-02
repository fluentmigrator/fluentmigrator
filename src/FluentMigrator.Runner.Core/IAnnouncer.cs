#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Represents an interface for logging and announcing messages during migrations.
    /// </summary>
    /// <remarks>
    /// This interface is marked as <see cref="ObsoleteAttribute"/> and may be removed in future versions.
    /// Implementations of this interface are used to log messages, errors, SQL statements, and other information
    /// during the execution of database migrations.
    /// </remarks>
    [Obsolete]
    public interface IAnnouncer
    {
        /// <summary>
        /// Logs a heading message.
        /// </summary>
        /// <param name="message"></param>
        void Heading(string message);
        /// <summary>
        /// Logs a regular message.
        /// </summary>
        /// <param name="message"></param>
        void Say(string message);
        /// <summary>
        /// Logs an emphasized message.
        /// </summary>
        /// <param name="message"></param>
        void Emphasize(string message);
        /// <summary>
        /// Logs a SQL message.
        /// </summary>
        /// <param name="sql"></param>
        void Sql(string sql);
        /// <summary>
        /// Logs the elapsed time.
        /// </summary>
        /// <param name="timeSpan"></param>
        void ElapsedTime(TimeSpan timeSpan);
        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message"></param>
        void Error(string message);
        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="exception"></param>
        void Error(Exception exception);

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isNotSql"></param>
        [Obsolete]
        void Write(string message, bool isNotSql = true);
    }
}
