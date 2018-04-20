#region License
// Copyright (c) 2007-2018, Sean Chambers and the FluentMigrator Project
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

using McMaster.Extensions.CommandLineUtils;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace FluentMigrator.DotNet.Cli.Commands
{
    [HelpOption]
    [Command(Description = "Rollback last migration")]
    [Subcommand("by", typeof(RollbackBy))]
    [Subcommand("to", typeof(RollbackTo))]
    [Subcommand("all", typeof(RollbackAll))]
    public class Rollback : ConnectionCommand
    {
        [Option("-m|--transaction-mode <MODE>", Description = "Overrides the transaction behavior of migrations, so that all migrations to be executed will run in one transaction.")]
        public TransactionMode TransactionMode { get; }

        private int OnExecute(IConsole console)
        {
            var options = MigratorOptions.CreateRollbackBy(this, null);
            return ExecuteMigrations(options, console);
        }
    }
}
