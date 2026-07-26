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
using System.Linq;

using FluentMigrator.DotNet.Cli.Commands;

namespace FluentMigrator.DotNet.Cli
{
    public class MigratorOptions
    {
        public MigratorOptions()
        {
        }

        private MigratorOptions(string task)
        {
            Task = task;
        }

        public string Task { get; }

        // The setters below are internal rather than private so that the Mapperly-generated
        // MigratorOptionsMapper can assign them. They remain invisible outside this assembly.
        public string ConnectionString { get; internal set; }
        public string ProcessorType { get; internal set; }
        public string ProcessorSwitches { get; internal set; }
        public IReadOnlyCollection<string> TargetAssemblies { get; internal set; }
        public long? TargetVersion { get; internal set; }
        public int? Steps { get; internal set; }
        public string Namespace { get; internal set; }
        public bool NestedNamespaces { get; internal set; }
        public long? StartVersion { get; internal set; }
        public bool NoConnection { get; internal set; }
        public string WorkingDirectory { get; internal set; }
        public IEnumerable<string> Tags { get; internal set; }
        public bool Preview { get; internal set; }
        public bool Verbose { get; internal set; }
        public string Profile { get; internal set; }
        public int? Timeout { get; internal set; }
        public TransactionMode TransactionMode { get; internal set; }
        public bool Output { get; internal set; }
        public string OutputFileName { get; internal set; }
        public bool AllowBreakingChanges { get; internal set; }
        public string SchemaName { get; internal set; }
        public bool StripComments { get; internal set; }
        public bool IncludeUntaggedMaintenances { get; internal set; }
        public bool IncludeUntaggedMigrations { get; internal set; } = true;

        public static MigratorOptions CreateListMigrations(ListMigrations cmd)
        {
            var result = new MigratorOptions("listmigrations")
                .Init(cmd);
            return result;
        }

        public static MigratorOptions CreateMigrateUp(Migrate cmd, long? targetVersion = null)
        {
            var result = new MigratorOptions("migrate:up")
                .Init(cmd);
            result.TargetVersion = targetVersion;
            result.TransactionMode = cmd.TransactionMode;
            return result;
        }

        public static MigratorOptions CreateMigrateDown(MigrateDown cmd)
        {
            var result = new MigratorOptions("migrate:down")
                .Init(cmd.Parent);
            result.TargetVersion = cmd.TargetVersion;
            result.TransactionMode = cmd.Parent.TransactionMode;
            return result;
        }

        public static MigratorOptions CreateRollbackBy(Rollback cmd, int? steps)
        {
            var result = new MigratorOptions("rollback")
                .Init(cmd);
            result.Steps = steps;
            result.TransactionMode = cmd.TransactionMode;
            return result;
        }

        public static MigratorOptions CreateRollbackTo(RollbackTo cmd)
        {
            var result = new MigratorOptions("rollback:toversion")
                .Init(cmd.Parent);
            result.TargetVersion = cmd.Version;
            result.TransactionMode = cmd.Parent.TransactionMode;
            return result;
        }

        public static MigratorOptions CreateRollbackAll(RollbackAll cmd)
        {
            var result = new MigratorOptions("rollback:all")
                .Init(cmd.Parent);
            result.TransactionMode = cmd.Parent.TransactionMode;
            return result;
        }

        public static MigratorOptions CreateValidateVersionOrder(ValidateVersionOrder cmd)
        {
            return new MigratorOptions("validateversionorder")
                .Init(cmd);
        }

        private MigratorOptions Init(ConnectionCommand cmd)
        {
            ConnectionString = cmd.ConnectionString;
            NoConnection = cmd.NoConnection;
            ProcessorType = cmd.ProcessorType;
            ProcessorSwitches = cmd.ProcessorSwitches;
            Preview = cmd.Preview;
            Verbose = cmd.Verbose;
            Profile = cmd.Profile;
            Timeout = cmd.Timeout;
            StripComments = !cmd.StripComments.hasValue || (cmd.StripComments.value ?? true);
            (Output, OutputFileName) = cmd.Output;
            return Init((MigrationCommand)cmd);
        }

        private MigratorOptions Init(MigrationCommand cmd)
        {
            TargetAssemblies = cmd.TargetAssemblies.ToList();
            Namespace = cmd.Namespace;
            NestedNamespaces = cmd.NestedNamespaces;
            StartVersion = cmd.StartVersion;
            WorkingDirectory = cmd.WorkingDirectory;
            Tags = cmd.Tags?.ToList() ?? new List<string>();
            AllowBreakingChanges = cmd.AllowBreakingChanges;
            SchemaName = cmd.SchemaName;
            IncludeUntaggedMigrations = !cmd.IncludeUntaggedMigrations.hasValue || (cmd.IncludeUntaggedMigrations.value ?? true);
            IncludeUntaggedMaintenances = cmd.IncludeUntaggedMaintenances;
            return this;
        }
    }
}
