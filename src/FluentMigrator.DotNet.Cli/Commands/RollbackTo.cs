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

using System.ComponentModel.DataAnnotations;

using McMaster.Extensions.CommandLineUtils;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace FluentMigrator.DotNet.Cli.Commands
{
    [HelpOption]
    [Command("to", Description = "Rollback migrations up to a given version")]
    public class RollbackTo : BaseCommand
    {
        public Rollback Parent { get; set; }

        [Argument(0, "version", "The target version to rollback to.")]
        [Required]
        public long Version { get; set; }

        private int OnExecute(IConsole console)
        {
            var options = MigratorOptions.CreateRollbackTo(this);
            return ExecuteMigrations(options, console);
        }
    }
}
