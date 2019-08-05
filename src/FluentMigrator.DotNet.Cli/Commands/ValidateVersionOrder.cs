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

namespace FluentMigrator.DotNet.Cli.Commands
{
    [HelpOption]
    [Command("versions", Description = "Validate the order of applied migrations")]
    public class ValidateVersionOrder : ConnectionCommand
    {
        private int OnExecute(IConsole console)
        {
            var options = MigratorOptions.CreateValidateVersionOrder(this);
            return ExecuteMigrations(options, console);
        }
    }
}
