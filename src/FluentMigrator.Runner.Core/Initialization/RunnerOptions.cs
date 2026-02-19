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
using System.Linq;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// The options for a migration runner
    /// </summary>
    public class RunnerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunnerOptions"/> class.
        /// </summary>
        public RunnerOptions()
        {
        }

        /// <summary>
        /// Gets or sets the task to execute
        /// </summary>
        [CanBeNull]
        public string Task { get; set; }

        /// <summary>
        /// Gets or sets the target version
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// Gets or sets the start version
        /// </summary>
        /// <remarks>This will be used only if <see cref="NoConnection"/> is set to true.
        /// Otherwise current state of target database will determine which migrations are needed.</remarks>
        public long StartVersion { get; set; }

        /// <summary>
        /// Gets or sets the number of versions to apply
        /// </summary>
        public int Steps { get; set; }

        /// <summary>
        /// Gets or sets the profile migrations to apply
        /// </summary>
        [CanBeNull]
        public string Profile { get; set; }

        /// <summary>
        /// Gets or sets the tags the migrations must match
        /// </summary>
        /// <remarks>All migrations are matched when no tags were specified</remarks>
        [CanBeNull, ItemNotNull]
        public string[] Tags { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the migration runner is allowed to apply breaking changes
        /// </summary>
        public bool AllowBreakingChange { get; set; }

        /// <summary>
        /// Use one transaction for the whole session
        /// </summary>
        /// <remarks>
        /// The default transaction behavior is to use one transaction per migration.
        /// </remarks>
        public bool TransactionPerSession { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether no connection should be used
        /// </summary>
        /// <remarks>
        /// The difference between this and PreviewOnly is, that
        /// the preview-only mode uses the connection to determine the current
        /// state of the database.
        /// </remarks>
        public bool NoConnection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether untagged maintenance items should always be loaded/executed.
        /// </summary>
        public bool IncludeUntaggedMaintenances { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether untagged migrations should always be loaded/executed.
        /// </summary>
        public bool IncludeUntaggedMigrations { get; set; } = true;

        /// <summary>
        /// Sets the values from the given runner context
        /// </summary>
        /// <param name="runnerContext">The runner context</param>
        [Obsolete]
        public void SetValuesFrom(IRunnerContext runnerContext)
        {
            Task = runnerContext.Task;
            Version = runnerContext.Version;
            StartVersion = runnerContext.StartVersion;
            Steps = runnerContext.Steps;
            Profile = runnerContext.Profile;
            Tags = runnerContext.Tags?.ToArray();
            AllowBreakingChange = runnerContext.AllowBreakingChange;
            TransactionPerSession = runnerContext.TransactionPerSession;
            NoConnection = runnerContext.NoConnection;
        }
    }
}
