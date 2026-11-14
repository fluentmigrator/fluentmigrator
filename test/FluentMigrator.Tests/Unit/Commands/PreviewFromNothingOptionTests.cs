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

#if !NETFRAMEWORK
using FluentMigrator.DotNet.Cli;
using FluentMigrator.DotNet.Cli.Commands;
using FluentMigrator.Runner.Initialization;

using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Commands
{
    /// <summary>
    /// <c>--preview-from-nothing</c> has to travel from the command line to <see cref="RunnerOptions"/> (#2087).
    /// </summary>
    [TestFixture]
    public class PreviewFromNothingOptionTests
    {
        [TestCase(new[] { "-p", "SQLite", "--no-connection", "--preview-from-nothing" }, true)]
        [TestCase(new[] { "-p", "SQLite", "--no-connection" }, false)]
        public void SwitchReachesRunnerOptions(string[] args, bool expected)
        {
            var app = new CommandLineApplication<Migrate>();
            app.Conventions.UseDefaultConventions();
            app.Parse(args);
            app.Model.TargetAssemblies = new string[0];

            var options = MigratorOptions.CreateMigrateUp(app.Model);
            using var serviceProvider = Setup.BuildServiceProvider(options, PhysicalConsole.Singleton);

            options.PreviewFromNothing.ShouldBe(expected);
            serviceProvider.GetRequiredService<IOptions<RunnerOptions>>().Value.PreviewFromNothing.ShouldBe(expected);
        }
    }
}
#endif
