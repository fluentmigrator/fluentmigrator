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
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using FluentMigrator.SqlProj;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace FluentMigrator.MigrationGenerator.Commands
{
    [HelpOption]
    [Command("from-sqlproj", Description = "Generate FluentMigrator migration from SQL Server Database Project (.sqlproj)")]
    public class GenerateFromSqlProj
    {
        [Option("-p|--project <PATH>", Description = "Path to the .sqlproj file")]
        public string SqlProjPath { get; set; } = string.Empty;

        [Option("-o|--output <PATH>", Description = "Output directory for generated migration file")]
        public string OutputPath { get; set; } = string.Empty;

        [Option("-n|--name <NAME>", Description = "Name for the migration class")]
        public string MigrationName { get; set; } = string.Empty;

        [Option("--namespace <NAMESPACE>", Description = "Namespace for the migration class (default: Migrations)")]
        public string Namespace { get; set; } = "Migrations";

        [Option("-v|--version <VERSION>", Description = "Migration version number (default: timestamp)")]
        public long? Version { get; set; }

        [Option("--verbose", Description = "Show verbose output")]
        public bool Verbose { get; }

        private int OnExecute(IConsole console)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(SqlProjPath))
                {
                    console.Error.WriteLine("Error: SqlProj file path is required. Use -p or --project to specify the path.");
                    return 1;
                }

                if (!File.Exists(SqlProjPath))
                {
                    console.Error.WriteLine($"Error: SqlProj file not found: {SqlProjPath}");
                    return 1;
                }

                if (string.IsNullOrWhiteSpace(OutputPath))
                {
                    console.Error.WriteLine("Error: Output directory is required. Use -o or --output to specify the path.");
                    return 1;
                }

                if (string.IsNullOrWhiteSpace(MigrationName))
                {
                    console.Error.WriteLine("Error: Migration name is required. Use -n or --name to specify the name.");
                    return 1;
                }

                // Generate migration
                console.WriteLine($"Generating migration from {SqlProjPath}...");
                
                var generator = new SqlProjMigrationGenerator(Namespace);
                var outputFile = generator.GenerateMigrationFromSqlProj(
                    SqlProjPath,
                    OutputPath,
                    MigrationName,
                    Version);

                console.WriteLine($"Migration generated successfully: {outputFile}");
                return 0;
            }
            catch (Exception ex)
            {
                console.Error.WriteLine($"Error generating migration: {ex.Message}");
                if (Verbose)
                {
                    console.Error.WriteLine(ex.StackTrace);
                }
                return 1;
            }
        }
    }
}
