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

using McMaster.Extensions.CommandLineUtils;

namespace FluentMigrator.DotNet.Cli.Commands
{
    [HelpOption(Description = "Execute FluentMigrator actions")]
    [Command("dotnet-fm", Description = "The external FluentMigrator runner that integrates into the .NET Core CLI tooling")]
    [Subcommand(typeof(Migrate), typeof(Rollback), typeof(Validate), typeof(ListCommand), typeof(GenerateFromSqlProj))]
    public class Root
    {
        protected int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.Error.WriteLine("You must specify a subcommand.");
            app.ShowHelp();
            return 1;
        }
    }
}
