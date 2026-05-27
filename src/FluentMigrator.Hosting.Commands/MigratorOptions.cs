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

using System.Collections.Generic;

namespace FluentMigrator.Hosting.Commands
{
    /// <summary>Parsed command-line options used to configure a FluentMigrator run.</summary>
    public class MigratorOptions
    {
        // ── Task ────────────────────────────────────────────────────────────
        /// <summary>Internal task name (e.g. "migrate:up", "rollback", …).</summary>
        public string Task { get; set; }

        // ── Assembly / namespace ─────────────────────────────────────────────
        public IReadOnlyCollection<string> TargetAssemblies { get; set; } = new string[0];
        public string Namespace { get; set; }
        public bool NestedNamespaces { get; set; }
        public long? StartVersion { get; set; }
        public string WorkingDirectory { get; set; }

        // ── Tags ──────────────────────────────────────────────────────────────
        public IEnumerable<string> Tags { get; set; } = new string[0];
        public bool AllowBreakingChanges { get; set; }
        public string SchemaName { get; set; }
        public bool StripComments { get; set; } = true;
        public bool IncludeUntaggedMigrations { get; set; } = true;
        public bool IncludeUntaggedMaintenances { get; set; }

        // ── Connection ───────────────────────────────────────────────────────
        public string ConnectionString { get; set; }
        public string ProcessorType { get; set; }
        public string ProcessorSwitches { get; set; }
        public TransactionMode TransactionMode { get; set; }
        public bool NoConnection { get; set; }
        public bool Preview { get; set; }
        public bool Verbose { get; set; }
        public string Profile { get; set; }
        public int? Timeout { get; set; }

        // ── Output ────────────────────────────────────────────────────────────
        public bool Output { get; set; }
        public string OutputFileName { get; set; }

        // ── Migration-specific ────────────────────────────────────────────────
        public long? TargetVersion { get; set; }
        public int? Steps { get; set; }
    }
}
