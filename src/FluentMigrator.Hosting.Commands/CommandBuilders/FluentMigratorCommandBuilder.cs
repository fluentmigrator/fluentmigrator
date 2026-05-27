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
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;

using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Hosting.Commands.CommandBuilders
{
    /// <summary>
    /// Builds the System.CommandLine command tree for FluentMigrator.
    /// <para>
    /// Usage from Program.cs:
    /// <code>
    /// var root = FluentMigratorCommandBuilder.BuildRootCommand();
    /// return root.Parse(args).Invoke(new InvocationConfiguration());
    /// </code>
    /// </para>
    /// </summary>
    public static class FluentMigratorCommandBuilder
    {
        // ─────────────────────────────────────────────────────────────────────
        // Shared option declarations
        // (static readonly so that handler lambdas can close over them and
        //  ParseResult.GetValue() can locate them at any command level)
        // ─────────────────────────────────────────────────────────────────────

        // Migration-level options
        public static readonly Option<string[]> AssemblyOption = new Option<string[]>(
            "--assembly", new[] { "-a" })
        {
            Description = "The assemblies containing the migrations you want to execute.",
            Arity = ArgumentArity.OneOrMore,
            Required = true,
        };

        public static readonly Option<string> NamespaceOption = new Option<string>(
            "--namespace", new[] { "-n" })
        {
            Description = "The namespace containing the migrations you want to run. Default: all migrations in the target assembly.",
        };

        public static readonly Option<bool> NestedOption = new Option<bool>("--nested")
        {
            Description = "Whether migrations in nested namespaces should be included (used with --namespace).",
        };

        public static readonly Option<long?> StartVersionOption = new Option<long?>("--start-version")
        {
            Description = "The specific version to start migrating from. Only used when --no-connection is set. Default: 0.",
        };

        public static readonly Option<string> WorkingDirectoryOption = new Option<string>(
            "--working-directory", new[] { "-wd" })
        {
            Description = "The directory to load SQL scripts specified by migrations from.",
        };

        public static readonly Option<string[]> TagsOption = new Option<string[]>(
            "--tag", new[] { "-t" })
        {
            Description = "Filters the migrations to be run by tag. Can be specified multiple times.",
            Arity = ArgumentArity.ZeroOrMore,
        };

        public static readonly Option<bool> AllowBreakingChangesOption = new Option<bool>(
            "--allow-breaking-changes", new[] { "-b" })
        {
            Description = "Allows execution of migrations marked as breaking changes.",
        };

        public static readonly Option<string> SchemaNameOption = new Option<string>(
            "--default-schema-name")
        {
            Description = "Set default schema name for the VersionInfo table and migrations.",
        };

        /// <summary>Strip SQL comments (default true). Pass --strip false to disable.</summary>
        public static readonly Option<bool> StripCommentsOption = new Option<bool>("--strip")
        {
            Description = "Strip comments from the SQL scripts. Default is true.",
            DefaultValueFactory = _ => true,
        };

        /// <summary>Include untagged migrations (default true). Pass --include-untagged-migrations false to disable.</summary>
        public static readonly Option<bool> IncludeUntaggedMigrationsOption = new Option<bool>(
            "--include-untagged-migrations")
        {
            Description = "Include untagged migrations. Default is true.",
            DefaultValueFactory = _ => true,
        };

        public static readonly Option<bool> IncludeUntaggedMaintenancesOption = new Option<bool>(
            "--include-untagged-maintenances")
        {
            Description = "Include untagged maintenances. Default is false.",
        };

        // --allow-dirty-assemblies is handled before System.CommandLine is invoked (Program.cs),
        // but we declare it here so it shows up in --help.
        public static readonly Option<bool> AllowDirtyAssembliesOption = new Option<bool>(
            "--allow-dirty-assemblies", new[] { "--allowDirtyAssemblies" })
        {
            Description = "Allows dirty (shadow-copied) assemblies to be loaded.",
        };

        // Connection-level options
        public static readonly Option<string> ConnectionOption = new Option<string>(
            "--connection", new[] { "-c" })
        {
            Description = "Connection string or name (falls back to machine name) for the target database.",
        };

        public static readonly Option<TransactionMode> TransactionModeOption = new Option<TransactionMode>(
            "--transaction-mode", new[] { "-m" })
        {
            Description = "Transaction behaviour: Migration (one transaction per migration) or Session (one transaction for all).",
            DefaultValueFactory = _ => TransactionMode.Migration,
        };

        public static readonly Option<bool> NoConnectionOption = new Option<bool>("--no-connection")
        {
            Description = "Generate SQL without consulting a target database. Requires --output.",
        };

        public static readonly Option<string> ProcessorOption = new Option<string>(
            "--processor", new[] { "-p" })
        {
            Description = "Database type to migrate against (e.g. sqlserver2016, postgres, sqlite). Use \"list processors\" to see choices.",
            Required = true,
        };

        public static readonly Option<string> ProcessorSwitchesOption = new Option<string>(
            "--processor-switches", new[] { "-s" })
        {
            Description = "Provider-specific switches.",
        };

        public static readonly Option<bool> PreviewOption = new Option<bool>("--preview")
        {
            Description = "Output migration steps without executing them. Use --verbose to also see SQL.",
        };

        public static readonly Option<bool> VerboseOption = new Option<bool>(
            "--verbose", new[] { "-V" })
        {
            Description = "Show SQL statements and execution time in the console.",
        };

        public static readonly Option<string> ProfileOption = new Option<string>("--profile")
        {
            Description = "The profile to run after executing migrations.",
        };

        public static readonly Option<int?> TimeoutOption = new Option<int?>("--timeout")
        {
            Description = "Override the default database command timeout (seconds).",
        };

        /// <summary>
        /// <c>--output</c> / <c>-o</c>: ZeroOrOne arity so that presence alone means "enable output"
        /// and an optional value specifies the file name.
        /// </summary>
        public static readonly Option<string> OutputOption = new Option<string>(
            "--output", new[] { "-o" })
        {
            Description = "Output generated SQL to a file. Optionally specify a filename (default: [assembly].sql).",
            Arity = ArgumentArity.ZeroOrOne,
        };

        // ─────────────────────────────────────────────────────────────────────
        // Sub-command-specific options
        // ─────────────────────────────────────────────────────────────────────

        private static readonly Option<long?> MigrateTargetOption = new Option<long?>(
            "--target", new[] { "-t" })
        {
            Description = "Migrate to this specific version.",
        };

        private static readonly Option<long> MigrateDownTargetOption = new Option<long>(
            "--target", new[] { "-t" })
        {
            Description = "Revert migrations back to (but not including) this version.",
            Required = true,
        };

        private static readonly Option<long> RollbackToVersionOption = new Option<long>(
            "--version", new[] { "-v" })
        {
            Description = "Rollback to this target version.",
            Required = true,
        };

        private static readonly Argument<int> RollbackStepsArgument = new Argument<int>("steps")
        {
            Description = "Number of versions to roll back.",
        };

        // ─────────────────────────────────────────────────────────────────────
        // Public entry point
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Builds and returns the fully configured root command.</summary>
        public static RootCommand BuildRootCommand()
        {
            var root = new RootCommand(
                "The external FluentMigrator runner that integrates into the .NET Core CLI tooling");

            root.Add(BuildMigrateCommand());
            root.Add(BuildRollbackCommand());
            root.Add(BuildValidateCommand());
            root.Add(BuildListCommand());

            return root;
        }

        // ─────────────────────────────────────────────────────────────────────
        // migrate
        // ─────────────────────────────────────────────────────────────────────

        private static Command BuildMigrateCommand()
        {
            var migrate = new Command("migrate", "Apply migrations");
            AddConnectionOptions(migrate);

            // migrate (no sub-verb) ≡ migrate up
            migrate.SetAction((ParseResult result) =>
                ExecuteMigrations(BuildConnectionOptions(result, "migrate:up")));

            // migrate up
            var up = new Command("up", "Apply migrations up to an optional target version");
            AddConnectionOptions(up);
            up.Add(MigrateTargetOption);
            up.SetAction((ParseResult result) =>
            {
                var opts = BuildConnectionOptions(result, "migrate:up");
                opts.TargetVersion = result.GetValue(MigrateTargetOption);
                return ExecuteMigrations(opts);
            });
            migrate.Add(up);

            // migrate down
            var down = new Command("down", "Revert migrations");
            AddConnectionOptions(down);
            down.Add(MigrateDownTargetOption);
            down.SetAction((ParseResult result) =>
            {
                var opts = BuildConnectionOptions(result, "migrate:down");
                opts.TargetVersion = result.GetValue(MigrateDownTargetOption);
                return ExecuteMigrations(opts);
            });
            migrate.Add(down);

            return migrate;
        }

        // ─────────────────────────────────────────────────────────────────────
        // rollback
        // ─────────────────────────────────────────────────────────────────────

        private static Command BuildRollbackCommand()
        {
            var rollback = new Command("rollback", "Rollback last migration");
            AddConnectionOptions(rollback);

            // rollback (no sub-verb) ≡ rollback by 1
            rollback.SetAction((ParseResult result) =>
                ExecuteMigrations(BuildConnectionOptions(result, "rollback")));

            // rollback by <steps>
            var by = new Command("by", "Rollback a specified number of migrations");
            AddConnectionOptions(by);
            by.Add(RollbackStepsArgument);
            by.SetAction((ParseResult result) =>
            {
                var opts = BuildConnectionOptions(result, "rollback");
                opts.Steps = result.GetValue(RollbackStepsArgument);
                return ExecuteMigrations(opts);
            });
            rollback.Add(by);

            // rollback to <version>
            var to = new Command("to", "Rollback migrations up to a given version");
            AddConnectionOptions(to);
            to.Add(RollbackToVersionOption);
            to.SetAction((ParseResult result) =>
            {
                var opts = BuildConnectionOptions(result, "rollback:toversion");
                opts.TargetVersion = result.GetValue(RollbackToVersionOption);
                return ExecuteMigrations(opts);
            });
            rollback.Add(to);

            // rollback all
            var all = new Command("all", "Rollback all migrations");
            AddConnectionOptions(all);
            all.SetAction((ParseResult result) =>
                ExecuteMigrations(BuildConnectionOptions(result, "rollback:all")));
            rollback.Add(all);

            return rollback;
        }

        // ─────────────────────────────────────────────────────────────────────
        // validate
        // ─────────────────────────────────────────────────────────────────────

        private static Command BuildValidateCommand()
        {
            var validate = new Command("validate", "Validation commands");
            validate.SetAction((ParseResult result) =>
            {
                result.InvocationConfiguration.Error.Write("You must specify a subcommand." + Environment.NewLine);
                return 1;
            });

            var versions = new Command("versions", "Validate the order of applied migrations");
            AddConnectionOptions(versions);
            versions.SetAction((ParseResult result) =>
                ExecuteMigrations(BuildConnectionOptions(result, "validateversionorder")));
            validate.Add(versions);

            return validate;
        }

        // ─────────────────────────────────────────────────────────────────────
        // list
        // ─────────────────────────────────────────────────────────────────────

        private static Command BuildListCommand()
        {
            var list = new Command("list", "List available information");
            list.SetAction((ParseResult result) =>
            {
                result.InvocationConfiguration.Error.Write("You must specify a subcommand." + Environment.NewLine);
                return 1;
            });

            // list migrations
            var migrations = new Command("migrations", "List migrations");
            AddConnectionOptions(migrations);
            migrations.SetAction((ParseResult result) =>
                ExecuteMigrations(BuildConnectionOptions(result, "listmigrations")));
            list.Add(migrations);

            // list processors (no connection options needed)
            var processors = new Command("processors", "List available database processors");
            processors.SetAction((ParseResult result) =>
            {
                var serviceProvider = Setup.BuildServiceProvider(new MigratorOptions());
                var processorList = serviceProvider
                    .GetRequiredService<IEnumerable<IMigrationProcessor>>()
                    .Select(p => p.DatabaseType)
                    .Distinct()
                    .OrderBy(x => x);

                foreach (var processorType in processorList)
                {
                    result.InvocationConfiguration.Output.Write(processorType + Environment.NewLine);
                }

                return 0;
            });
            list.Add(processors);

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helper: add all connection + migration options to a command
        // ─────────────────────────────────────────────────────────────────────

        private static void AddConnectionOptions(Command cmd)
        {
            // Migration-level
            cmd.Add(AssemblyOption);
            cmd.Add(NamespaceOption);
            cmd.Add(NestedOption);
            cmd.Add(StartVersionOption);
            cmd.Add(WorkingDirectoryOption);
            cmd.Add(TagsOption);
            cmd.Add(AllowBreakingChangesOption);
            cmd.Add(SchemaNameOption);
            cmd.Add(StripCommentsOption);
            cmd.Add(IncludeUntaggedMigrationsOption);
            cmd.Add(IncludeUntaggedMaintenancesOption);
            cmd.Add(AllowDirtyAssembliesOption);

            // Connection-level
            cmd.Add(ConnectionOption);
            cmd.Add(TransactionModeOption);
            cmd.Add(NoConnectionOption);
            cmd.Add(ProcessorOption);
            cmd.Add(ProcessorSwitchesOption);
            cmd.Add(PreviewOption);
            cmd.Add(VerboseOption);
            cmd.Add(ProfileOption);
            cmd.Add(TimeoutOption);
            cmd.Add(OutputOption);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helper: read all connection + migration options from ParseResult
        // ─────────────────────────────────────────────────────────────────────

        private static MigratorOptions BuildConnectionOptions(ParseResult result, string task)
        {
            // --output: ZeroOrOne arity — presence means enabled, optional value is filename.
            bool outputEnabled = result.GetResult(OutputOption) != null;
            string outputFileName = result.GetValue(OutputOption);

            return new MigratorOptions
            {
                Task = task,

                // Migration-level
                TargetAssemblies = result.GetValue(AssemblyOption) ?? new string[0],
                Namespace = result.GetValue(NamespaceOption),
                NestedNamespaces = result.GetValue(NestedOption),
                StartVersion = result.GetValue(StartVersionOption),
                WorkingDirectory = result.GetValue(WorkingDirectoryOption),
                Tags = result.GetValue(TagsOption) ?? new string[0],
                AllowBreakingChanges = result.GetValue(AllowBreakingChangesOption),
                SchemaName = result.GetValue(SchemaNameOption),
                StripComments = result.GetValue(StripCommentsOption),
                IncludeUntaggedMigrations = result.GetValue(IncludeUntaggedMigrationsOption),
                IncludeUntaggedMaintenances = result.GetValue(IncludeUntaggedMaintenancesOption),

                // Connection-level
                ConnectionString = result.GetValue(ConnectionOption),
                TransactionMode = result.GetValue(TransactionModeOption),
                NoConnection = result.GetValue(NoConnectionOption),
                ProcessorType = result.GetValue(ProcessorOption),
                ProcessorSwitches = result.GetValue(ProcessorSwitchesOption),
                Preview = result.GetValue(PreviewOption),
                Verbose = result.GetValue(VerboseOption),
                Profile = result.GetValue(ProfileOption),
                Timeout = result.GetValue(TimeoutOption),
                Output = outputEnabled,
                OutputFileName = outputFileName,
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helper: execute migrations and return exit code
        // ─────────────────────────────────────────────────────────────────────

        private static int ExecuteMigrations(MigratorOptions options)
        {
            using var serviceProvider = Setup.BuildServiceProvider(options);
            var executor = serviceProvider.GetRequiredService<TaskExecutor>();
            executor.Execute();
            return 0;
        }
    }
}
