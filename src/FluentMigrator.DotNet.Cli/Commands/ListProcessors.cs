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

using System.Collections.Generic;
using System.Linq;

using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMember.Local
#pragma warning disable RCS1213 // Remove unused member declaration.

namespace FluentMigrator.DotNet.Cli.Commands
{
    [HelpOption]
    [Command("processors", Description = "List processors")]
    public class ListProcessors
    {
        private int OnExecute(IConsole console)
        {
            var migratorOptions = new MigratorOptions();
            var serviceProvider = Setup.BuildServiceProvider(migratorOptions, console);
            var processors = serviceProvider.GetRequiredService<IEnumerable<IMigrationProcessor>>().ToList();
            var processorIds = processors.Select(p => p.DatabaseType).Distinct().OrderBy(x => x);
            foreach (var processorType in processorIds)
            {
                console.WriteLine(processorType);
            }

            return 0;
        }
    }
}
