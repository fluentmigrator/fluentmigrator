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
using System.ComponentModel.DataAnnotations;

using McMaster.Extensions.CommandLineUtils;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace FluentMigrator.DotNet.Cli.Commands
{
    public class ConnectionCommand : MigrationCommand
    {
        [Option("-c|--connection <CONNECTION_STRING_OR_NAME>", Description = "The name of the connection string (falls back to machine name) or the connection string itself to the server and database you want to execute your migrations against.")]
        public string ConnectionString { get; set; }

        [Option("-m|--transaction-mode <MODE>", Description = "Overrides the transaction behavior of migrations, so that all migrations to be executed will run in one transaction.")]
        public TransactionMode TransactionMode { get; set; }

        [Option("--no-connection", Description = "Indicates that migrations will be generated without consulting a target database. Should only be used when generating an output file.")]
        public bool NoConnection { get; }

        [Option("-p|--processor <PROCESSOR_NAME>", Description = "The kind of database you are migrating against. Available choices can be shown with \"list processors\".")]
        [Required]
        public string ProcessorType { get; set; }

        [Option("-s|--processor-switches <PROCESSOR_SWITCHES>", Description = "Provider specific switches.")]
        public string ProcessorSwitches { get; set; }

        [Option("--preview", Description = "Only output the migration steps - do not execute them. Add the `--verbose` switch to also see the SQL statements. Default is false.")]
        public bool Preview { get; }

        [Option("-V|--verbose", Description = "Show the SQL statements generated and execution time in the console. Default is false.")]
        public bool Verbose { get; }

        [Option("--profile <PROFILE>", Description = "The profile to run after executing migrations.")]
        public string Profile { get; set; }

        [Option("--timeout <TIMEOUT_SEC>", Description = "Overrides the default database command timeout of 30 seconds.")]
        public int Timeout { get; set; }

        [Option("-o|--output=<FILENAME>", CommandOptionType.SingleOrNoValue, Description = "Output generated SQL to a file. Default is no output. A filename may be specified, otherwise [targetAssemblyName].sql is the default.")]
        public (bool, string) Output { get; set; }
    }
}
