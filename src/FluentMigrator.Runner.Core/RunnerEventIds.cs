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

using Microsoft.Extensions.Logging;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// The default runner event IDs
    /// </summary>
    public static class RunnerEventIds
    {
        /// <summary>
        /// Represents the category name used for logging events in the FluentMigrator runner.
        /// </summary>
        public const string RunnerCategory = "FluentMigrator.Runner";

        /// <summary>
        /// Gets the event ID for a heading
        /// </summary>
        public static readonly EventId Heading = new EventId(1000, RunnerCategory);

        /// <summary>
        /// Gets the event ID for an emphasized message
        /// </summary>
        public static readonly EventId Emphasize = new EventId(1001, RunnerCategory);

        /// <summary>
        /// Gets the event ID for elapsed time
        /// </summary>
        public static readonly EventId ElapsedTime = new EventId(1002, RunnerCategory);

        /// <summary>
        /// Gets the event ID for an SQL statement
        /// </summary>
        public static readonly EventId Sql = new EventId(1003, RunnerCategory);

        /// <summary>
        /// Gets the event ID for an informational message
        /// </summary>
        public static readonly EventId Say = new EventId(1004, RunnerCategory);
    }
}
