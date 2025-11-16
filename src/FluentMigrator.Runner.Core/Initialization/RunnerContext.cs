#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Provides context information for running migrations.
    /// </summary>
    [Obsolete]
    public class RunnerContext : IRunnerContext
    {
        /// <inheritdoc />
        public RunnerContext(IAnnouncer announcer)
        {
            Announcer = announcer;
        }

        /// <inheritdoc />
        public string Database { get; set; }
        /// <inheritdoc />
        public string Connection { get; set; }
        /// <inheritdoc />
        public string[] Targets { get; set; }
        /// <inheritdoc />
        public bool PreviewOnly { get; set; }
        /// <inheritdoc />
        public string Namespace { get; set; }
        /// <inheritdoc />
        public bool NestedNamespaces { get; set; }
        /// <inheritdoc />
        public string Task { get; set; }
        /// <inheritdoc />
        public long Version { get; set; }
        /// <inheritdoc />
        public long StartVersion { get; set; }
        /// <inheritdoc />
        public bool NoConnection { get; set; }
        /// <inheritdoc />
        public int Steps { get; set; }
        /// <inheritdoc />
        public string WorkingDirectory { get; set; }
        /// <inheritdoc />
        public string Profile { get; set; }
        /// <inheritdoc />
        public int? Timeout { get; set; }
        /// <inheritdoc />
        public string ConnectionStringConfigPath { get; set; }
        /// <inheritdoc />
        public IEnumerable<string> Tags { get; set; }
        /// <inheritdoc />
        public bool TransactionPerSession { get; set; }
        /// <inheritdoc />
        public bool AllowBreakingChange { get; set; }
        /// <inheritdoc />
        public string ProviderSwitches { get; set; }
        /// <inheritdoc />
        public IAnnouncer Announcer { get; }
        /// <inheritdoc />
        public IStopWatch StopWatch { get; } = new StopWatch();
        /// <inheritdoc />
        public string DefaultSchemaName { get; set; }
        /// <inheritdoc />
        public bool StripComments { get; set; }
    }
}
