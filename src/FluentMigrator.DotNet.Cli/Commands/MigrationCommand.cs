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
using System.ComponentModel.DataAnnotations;

using McMaster.Extensions.CommandLineUtils;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace FluentMigrator.DotNet.Cli.Commands
{
    public class MigrationCommand : BaseCommand
    {
        [Option("-a|--assembly <ASSEMBLY_NAME>", Description = "The assemblies containing the migrations you want to execute.")]
        [Required]
        public IEnumerable<string> TargetAssemblies { get; set;}

        [Option("-v|--version-info-meta-data-json <VERSION_INFO_TABLE_META_DATA_JSON>", Description = "The json that describe metada (for example '{\"SchemaName\": \"custom_schema\", \"TableName\": \"custom_table_name\"}').")]
        public string VersionInfoTableMetadataJson { get; set;}

        [Option("-n|--namespace <NAMESPACE>", Description = "The namespace contains the migrations you want to run. Default is all migrations found within the Target Assembly will be run.")]
        public string Namespace { get; set; }

        [Option("--nested", Description = "Whether migrations in nested namespaces should be included. Used in conjunction with the namespace option.")]
        public bool NestedNamespaces { get; }

        [Option("--start-version", Description = "The specific version to start migrating from. Only used when NoConnection is true. Default is 0.")]
        public long? StartVersion { get; set; }

        [Option("--working-directory <WORKING_DIRECTORY>", Description = "The directory to load SQL scripts specified by migrations from.")]
        public string WorkingDirectory { get; set; }

        [Option("-t|--tag", Description = "Filters the migrations to be run by tag.")]
        public IEnumerable<string> Tags { get; set; }

        [Option("-b|--allow-breaking-changes", Description = "Allows execution of migrations marked as breaking changes.")]
        public bool AllowBreakingChanges { get; }

        [Option("--default-schema-name", Description = "Set default schema name for VersionInfo table and the migrations.")]
        public string SchemaName { get; internal set; } = null;

        [Option("--strip", "Strip comments from the SQL scripts. Default is true.", CommandOptionType.SingleOrNoValue)]
        public (bool hasValue, bool? value) StripComments { get; set; }

        [Option("--include-untagged-migrations", "Include untagged migrations.", CommandOptionType.SingleOrNoValue)]
        public (bool hasValue, bool? value) IncludeUntaggedMigrations { get; set; }

        [Option("--include-untagged-maintenances", Description = "Include untagged maintenances.")]
        public bool IncludeUntaggedMaintenances { get; }

        [Option("--allowDirtyAssemblies", Description = "Allows dirty assemblies.")]
        public bool AllowDirtyAssemblies { get; }
    }
}
