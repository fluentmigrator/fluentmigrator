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

using System.ComponentModel.DataAnnotations;

using McMaster.Extensions.CommandLineUtils;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace FluentMigrator.DotNet.Cli.Commands
{
    [HelpOption]
    [Command("by", Description = "Rollback migrations")]
    public class RollbackBy : BaseCommand
    {
        public Rollback Parent { get; set; }

        [Argument(0, "steps", "The number of versions to rollback.")]
        [Required]
        public int Steps { get; set; }

        private int OnExecute(IConsole console)
        {
            var options = MigratorOptions.CreateRollbackBy(Parent, Steps);
            return ExecuteMigrations(options, console);
        }
    }
}
