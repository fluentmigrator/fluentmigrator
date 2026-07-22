#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.CommandLine;
using System.Linq;

using FluentMigrator.Hosting.Commands.CommandBuilders;
using FluentMigrator.Runner;

namespace FluentMigrator.DotNet.Cli
{
    public static class Program
    {
        static Program()
        {
            Microsoft.Data.Sqlite.SqliteFactory.Instance.CreateParameter();
        }

        public static int Main(string[] args)
        {
            // Pre-check for --allow-dirty-assemblies / legacy --allowDirtyAssemblies so that the
            // assembly resolver is in place before System.CommandLine triggers any assembly loads.
            bool useDirtyHelper = args.Any(a =>
                string.Equals(a, "--allow-dirty-assemblies", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(a, "--allowDirtyAssemblies", StringComparison.OrdinalIgnoreCase));

            var rootCommand = FluentMigratorCommandBuilder.BuildRootCommand();
            var parseResult = rootCommand.Parse(args);

            if (useDirtyHelper)
            {
                using (DirtyAssemblyResolveHelper.Create())
                {
                    return parseResult.Invoke(new InvocationConfiguration());
                }
            }

            return parseResult.Invoke(new InvocationConfiguration());
        }
    }
}
