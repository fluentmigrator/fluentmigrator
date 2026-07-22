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

using FluentMigrator.Exceptions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
#if NETFRAMEWORK
using FluentMigrator.Runner.Initialization.NetFramework;
#endif
using FluentMigrator.Runner.Logging;
using FluentMigrator.Runner.Processors;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using static FluentMigrator.Runner.ConsoleUtilities;

namespace FluentMigrator.Console
{
    public class MigratorConsole
    {
        // ── Public fields (preserved for backward compatibility) ─────────────
        public string Connection;
        public string ConnectionStringConfigPath;
        public string Namespace;
        public bool NestedNamespaces;
        public bool Output;
        public string OutputFilename;
        public bool OutputSemicolonDelimiter = false;
        public bool PreviewOnly;
        public string ProcessorType;
        public string Profile;
        public bool ShowHelp;
        public int Steps;
        public List<string> Tags = new List<string>();
        public bool IncludeUntaggedMaintenances;
        public bool IncludeUntaggedMigrations = true;
        public string TargetAssembly;
        public string Task;
        public int? Timeout;
        public bool Verbose;
        public bool StopOnError;
        public long Version;
        public long StartVersion;
        public bool NoConnection;
        public string WorkingDirectory;
        public bool TransactionPerSession;
        public bool AllowBreakingChange;
        public string ProviderSwitches;
        public bool StripComments = true;
        public string DefaultSchemaName { get; set; }

        // ── Option declarations ──────────────────────────────────────────────

        private static readonly Option<string> AssemblyOpt = new Option<string>(
            "--assembly", new[] { "-a", "--target" })
        {
            Description = "REQUIRED. The assembly containing the migrations you want to execute.",
            Required = true,
        };

        private static readonly Option<string> ProcessorOpt = new Option<string>(
            "--provider", new[] { "--dbType", "--dbtype", "--db" })
        {
            Description = "REQUIRED. The kind of database you are migrating against.",
            Required = true,
        };

        private static readonly Option<string> ConnectionOpt = new Option<string>(
            "--connectionString", new[] { "--connectionstring", "--connection", "--conn", "-c" })
        {
            Description = "The name of the connection string or the connection string itself.",
        };

        private static readonly Option<string> ConnectionStringConfigPathOpt = new Option<string>(
            "--connectionStringConfigPath", new[] { "--connectionstringconfigpath", "--configPath", "--configpath" })
        {
            Description = "The path of the machine.config where the named connection string is found.",
        };

        private static readonly Option<string> NamespaceOpt = new Option<string>(
            "--namespace", new[] { "--ns" })
        {
            Description = "The namespace containing the migrations you want to run.",
        };

        private static readonly Option<bool> NestedOpt = new Option<bool>("--nested")
        {
            Description = "Whether migrations in nested namespaces should be included.",
        };

        private static readonly Option<bool> OutputOpt = new Option<bool>(
            "--output", new[] { "--out", "-o" })
        {
            Description = "Output generated SQL to a file. Use --outputFilename to control the filename.",
        };

        private static readonly Option<bool> OutputSemicolonDelimiterOpt = new Option<bool>(
            "--outputSemicolonDelimiter", new[] { "--outputsemicolondelimiter", "--outsemdel", "--osd" })
        {
            Description = "Whether each command should be delimited with a semicolon.",
        };

        private static readonly Option<string> OutputFilenameOpt = new Option<string>(
            "--outputFilename", new[] { "--outputfilename", "--outfile", "--of" })
        {
            Description = "The name of the file to output the generated SQL to.",
        };

        private static readonly Option<bool> PreviewOpt = new Option<bool>("--preview")
        {
            Description = "Only output the migration steps - do not execute them.",
        };

        private static readonly Option<int> StepsOpt = new Option<int>("--steps")
        {
            Description = "The number of versions to rollback if the task is 'rollback'. Default is 1.",
        };

        private static readonly Option<string> TaskOpt = new Option<string>(
            "--task", new[] { "-t" })
        {
            Description = "The task you want FluentMigrator to perform.",
        };

        // Note: built-in --version is removed from RootCommand; this defines the migration version.
        private static readonly Option<long> VersionOpt = new Option<long>("--version")
        {
            Description = "The specific version to migrate. Default is 0, which will run all migrations.",
        };

        private static readonly Option<long> StartVersionOpt = new Option<long>(
            "--startVersion", new[] { "--startversion", "--start-version" })
        {
            Description = "The specific version to start migrating from. Only used when NoConnection is true.",
        };

        private static readonly Option<bool> NoConnectionOpt = new Option<bool>(
            "--noConnection", new[] { "--noconnection", "--no-connection" })
        {
            Description = "Indicates that migrations will be generated without consulting a target database.",
        };

        private static readonly Option<bool> VerboseOpt = new Option<bool>("--verbose")
        {
            Description = "Show the SQL statements generated and execution time in the console.",
        };

        private static readonly Option<bool> StopOnErrorOpt = new Option<bool>(
            "--stopOnError", new[] { "--stoponerror", "--stop-on-error" })
        {
            Description = "Pauses migration execution until the user input if any error occurred.",
        };

        private static readonly Option<string> WorkingDirectoryOpt = new Option<string>(
            "--workingdirectory", new[] { "--working-directory", "--wd" })
        {
            Description = "The directory to load SQL scripts specified by migrations from.",
        };

        private static readonly Option<string> ProfileOpt = new Option<string>("--profile")
        {
            Description = "The profile to run after executing migrations.",
        };

        private static readonly Option<int?> TimeoutOpt = new Option<int?>("--timeout")
        {
            Description = "Overrides the default SqlCommand timeout of 30 seconds.",
        };

        private static readonly Option<string[]> TagOpt = new Option<string[]>("--tag")
        {
            Description = "Filters the migrations to be run by tag.",
            Arity = ArgumentArity.ZeroOrMore,
        };

        /// <summary>
        /// <c>--include-untagged</c>: optional value. When present with no value, enables both
        /// migrations and maintenances. With a comma-separated value, each item may be
        /// <c>ma</c>/<c>maintenance</c> or <c>mi</c>/<c>migrations</c> optionally followed by
        /// <c>+</c> (enable) or <c>-</c> (disable).
        /// </summary>
        private static readonly Option<string> IncludeUntaggedOpt = new Option<string>(
            "--include-untagged", new[] { "--include-untagged-migrations" })
        {
            Description = "Include untagged migrations and/or maintenance objects. " +
                "With no value, includes both. Accepts: ma, maintenance, mi, migrations (with optional +/-).",
            Arity = ArgumentArity.ZeroOrOne,
        };

        private static readonly Option<string> ProviderSwitchesOpt = new Option<string>(
            "--providerSwitches", new[] { "--provider-switches", "--providerswitches" })
        {
            Description = "Provider specific switches.",
        };

        private static readonly Option<bool> StripOpt = new Option<bool>(
            "--strip", new[] { "--strip-comments" })
        {
            Description = "Strip comments from the SQL scripts. Default is true.",
            DefaultValueFactory = _ => true,
        };

        private static readonly Option<bool> TransactionPerSessionOpt = new Option<bool>(
            "--transaction-per-session", new[] { "--tps" })
        {
            Description = "All migrations to be executed will run in one transaction.",
        };

        private static readonly Option<bool> AllowBreakingChangesOpt = new Option<bool>(
            "--allow-breaking-changes", new[] { "--abc" })
        {
            Description = "Allows execution of migrations marked as breaking changes.",
        };

        private static readonly Option<string> DefaultSchemaNameOpt = new Option<string>(
            "--default-schema-name")
        {
            Description = "Set default schema name for the VersionInfo table and the migrations.",
        };

        // ── Entry point ──────────────────────────────────────────────────────

        /// <summary>
        /// Normalizes legacy Mono.Options-style arguments to System.CommandLine style.
        /// Converts <c>/option</c> prefix (case-insensitive) to <c>--option</c> (lower-case),
        /// preserving the value portion unchanged so existing scripts continue to work.
        /// </summary>
        public static string[] NormalizeArgs(string[] args)
        {
            var result = new string[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                // Convert /option or /option=value → --option or --option=value
                if (arg.Length > 1 && arg[0] == '/' && char.IsLetter(arg[1]))
                {
                    var rest = arg.Substring(1);
                    // Exclude file/UNC paths (contain a subsequent slash)
                    if (rest.IndexOf('/') < 0 && rest.IndexOf('\\') < 0)
                    {
                        var eqIdx = rest.IndexOf('=');
                        if (eqIdx >= 0)
                        {
                            // /Option=value → --option=value  (option name lowercased, value preserved)
                            arg = "--" + rest.Substring(0, eqIdx).ToLowerInvariant() + rest.Substring(eqIdx);
                        }
                        else
                        {
                            // /Option → --option
                            arg = "--" + rest.ToLowerInvariant();
                        }
                    }
                }
                result[i] = arg;
            }
            return result;
        }

        public int Run(params string[] args)
        {
            System.Console.Out.WriteHeader();

            try
            {
                var root = BuildRootCommand();
                var parseResult = root.Parse(NormalizeArgs(args));

                return parseResult.Invoke(new InvocationConfiguration());
            }
            catch (MissingMigrationsException ex)
            {
                AsError(() => System.Console.Error.WriteException(ex));
                return 6;
            }
            catch (RunnerException ex)
            {
                AsError(() => System.Console.Error.WriteException(ex));
                return 5;
            }
            catch (FluentMigratorException ex)
            {
                AsError(() => System.Console.Error.WriteException(ex));
                return 4;
            }
            catch (Exception ex)
            {
                AsError(() => System.Console.Error.WriteException(ex));
                return 3;
            }
        }

        private RootCommand BuildRootCommand()
        {
            var dbChoicesList = new List<string>();
            using (var sp = CreateCoreServices()
                       .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader("No connection"))
                       .BuildServiceProvider(validateScopes: false))
            {
                dbChoicesList.AddRange(
                    sp.GetRequiredService<IEnumerable<IMigrationProcessor>>()
                      .Select(p => p.DatabaseType)
                      .Distinct(StringComparer.OrdinalIgnoreCase)
                      .OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
            }

            string dbChoices = string.Join(", ", dbChoicesList);

            var root = new RootCommand("Console runner for FluentMigrator");

            // Remove the built-in --version option so that our migration-version option can use --version.
            var builtInVersion = root.Options.FirstOrDefault(o => o.Name == "--version");
            if (builtInVersion != null)
            {
                root.Options.Remove(builtInVersion);
            }

            // Adjust processor description with live DB choices
            var processorOpt = new Option<string>("--provider", new[] { "--dbType", "--db" })
            {
                Description = $"REQUIRED. The kind of database you are migrating against. Available choices are: {dbChoices}.",
                Required = true,
            };

            root.Add(AssemblyOpt);
            root.Add(processorOpt);
            root.Add(ConnectionOpt);
            root.Add(ConnectionStringConfigPathOpt);
            root.Add(NamespaceOpt);
            root.Add(NestedOpt);
            root.Add(OutputOpt);
            root.Add(OutputSemicolonDelimiterOpt);
            root.Add(OutputFilenameOpt);
            root.Add(PreviewOpt);
            root.Add(StepsOpt);
            root.Add(TaskOpt);
            root.Add(VersionOpt);
            root.Add(StartVersionOpt);
            root.Add(NoConnectionOpt);
            root.Add(VerboseOpt);
            root.Add(StopOnErrorOpt);
            root.Add(WorkingDirectoryOpt);
            root.Add(ProfileOpt);
            root.Add(TimeoutOpt);
            root.Add(TagOpt);
            root.Add(IncludeUntaggedOpt);
            root.Add(ProviderSwitchesOpt);
            root.Add(StripOpt);
            root.Add(TransactionPerSessionOpt);
            root.Add(AllowBreakingChangesOpt);
            root.Add(DefaultSchemaNameOpt);

            root.SetAction((ParseResult result) =>
            {
                PopulateFields(result, processorOpt);
                return ExecuteMigrations();
            });

            return root;
        }

        private void PopulateFields(ParseResult result, Option<string> processorOpt)
        {
            TargetAssembly = result.GetValue(AssemblyOpt);
            ProcessorType = result.GetValue(processorOpt);
            Connection = result.GetValue(ConnectionOpt);
            ConnectionStringConfigPath = result.GetValue(ConnectionStringConfigPathOpt);
            Namespace = result.GetValue(NamespaceOpt);
            NestedNamespaces = result.GetValue(NestedOpt);
            Output = result.GetValue(OutputOpt);
            OutputSemicolonDelimiter = result.GetValue(OutputSemicolonDelimiterOpt);
            OutputFilename = result.GetValue(OutputFilenameOpt);
            PreviewOnly = result.GetValue(PreviewOpt);
            Steps = result.GetValue(StepsOpt);
            Task = result.GetValue(TaskOpt) ?? "migrate";
            Version = result.GetValue(VersionOpt);
            StartVersion = result.GetValue(StartVersionOpt);
            NoConnection = result.GetValue(NoConnectionOpt);
            Verbose = result.GetValue(VerboseOpt);
            StopOnError = result.GetValue(StopOnErrorOpt);
            WorkingDirectory = result.GetValue(WorkingDirectoryOpt);
            Profile = result.GetValue(ProfileOpt);
            Timeout = result.GetValue(TimeoutOpt);
            Tags = new List<string>(result.GetValue(TagOpt) ?? Array.Empty<string>());
            ProviderSwitches = result.GetValue(ProviderSwitchesOpt);
            StripComments = result.GetValue(StripOpt);
            TransactionPerSession = result.GetValue(TransactionPerSessionOpt);
            AllowBreakingChange = result.GetValue(AllowBreakingChangesOpt);
            DefaultSchemaName = result.GetValue(DefaultSchemaNameOpt);

            // Parse --include-untagged: optional value
            var includeUntaggedResult = result.GetResult(IncludeUntaggedOpt);
            if (includeUntaggedResult != null)
            {
                string includeUntaggedValue = result.GetValue(IncludeUntaggedOpt);
                if (string.IsNullOrEmpty(includeUntaggedValue))
                {
                    IncludeUntaggedMigrations = IncludeUntaggedMaintenances = true;
                }
                else
                {
                    var items = includeUntaggedValue
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.ToLowerInvariant().Trim())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(x =>
                        {
                            bool hasModifier = x.EndsWith("+") || x.EndsWith("-");
                            bool enable = !x.EndsWith("-");
                            string name = hasModifier ? x.Substring(0, x.Length - 1) : x;
                            return new
                            {
                                FullName = x,
                                ShortName = name.Substring(Math.Min(2, name.Length)),
                                Enable = enable,
                            };
                        });

                    foreach (var item in items)
                    {
                        switch (item.ShortName)
                        {
                            case "ma":
                                IncludeUntaggedMaintenances = item.Enable;
                                break;
                            case "mi":
                                IncludeUntaggedMigrations = item.Enable;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(
                                    $"The argument {item.FullName} is not supported. "
                                  + "Valid values are: ma, maintenance, mi, migrations with an optional '+' or '-' at the end to enable or disable the option. "
                                  + "Multiple values may be given when separated by a comma.");
                        }
                    }
                }
            }
        }

        private bool ExecutingAgainstMsSql => ProcessorType != null
            && ProcessorType.StartsWith("SqlServer", StringComparison.InvariantCultureIgnoreCase);

        private int ExecuteMigrations()
        {
            var conventionSet = new DefaultConventionSet(DefaultSchemaName, WorkingDirectory);

            var services = CreateCoreServices()
                .Configure<FluentMigratorLoggerOptions>(opt =>
                {
                    opt.ShowElapsedTime = Verbose;
                    opt.ShowSql = Verbose;
                })
                .AddSingleton<IConventionSet>(conventionSet)
                .Configure<SelectingProcessorAccessorOptions>(opt => opt.ProcessorId = ProcessorType)
                .Configure<AssemblySourceOptions>(opt => opt.AssemblyNames = new[] { TargetAssembly })
                .Configure<TypeFilterOptions>(opt =>
                {
                    opt.Namespace = Namespace;
                    opt.NestedNamespaces = NestedNamespaces;
                })
                .Configure<RunnerOptions>(opt =>
                {
                    opt.Task = Task;
                    opt.Version = Version;
                    opt.StartVersion = StartVersion;
                    opt.NoConnection = NoConnection;
                    opt.Steps = Steps;
                    opt.Profile = Profile;
                    opt.Tags = Tags.ToArray();
                    opt.TransactionPerSession = TransactionPerSession;
                    opt.AllowBreakingChange = AllowBreakingChange;
                    opt.IncludeUntaggedMaintenances = IncludeUntaggedMaintenances;
                    opt.IncludeUntaggedMigrations = IncludeUntaggedMigrations;
                })
                .Configure<ProcessorOptions>(opt =>
                {
                    opt.ConnectionString = Connection;
                    opt.PreviewOnly = PreviewOnly;
                    opt.ProviderSwitches = ProviderSwitches;
                    opt.StripComments = StripComments;
                    opt.Timeout = Timeout == null ? null : (TimeSpan?)TimeSpan.FromSeconds(Timeout.Value);
                });

            if (StopOnError)
            {
                services.AddSingleton<ILoggerProvider, StopOnErrorLoggerProvider>();
            }
            else
            {
                services.AddSingleton<ILoggerProvider, FluentMigratorConsoleLoggerProvider>();
            }

            if (Output)
            {
                services
                    .Configure<LogFileFluentMigratorLoggerOptions>(opt =>
                    {
                        opt.ShowSql = true;
                        opt.OutputFileName = OutputFilename;
                        opt.OutputGoBetweenStatements = ExecutingAgainstMsSql;
                        opt.OutputSemicolonDelimiter = OutputSemicolonDelimiter;
                    })
                    .AddSingleton<ILoggerProvider, LogFileFluentMigratorLoggerProvider>();
            }

            using (var serviceProvider = services.BuildServiceProvider(validateScopes: false))
            {
                var executor = serviceProvider.GetRequiredService<TaskExecutor>();
                executor.Execute();
            }

            return 0;
        }

        private static IServiceCollection CreateCoreServices()
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(builder => builder
                    .AddDb2()
                    .AddDb2ISeries()
                    .AddDotConnectOracle()
                    .AddDotConnectOracle12C()
                    .AddFirebird()
                    .AddHana()
                    .AddMySql()
                    .AddMySql4()
                    .AddMySql5()
                    .AddMySql8()
                    .AddOracle()
                    .AddOracle12C()
                    .AddOracleManaged()
                    .AddOracle12CManaged()
                    .AddPostgres()
                    .AddPostgres92()
                    .AddPostgres10_0()
                    .AddPostgres11_0()
                    .AddPostgres15_0()
                    .AddRedshift()
                    .AddSnowflake()
                    .AddSQLite()
                    .AddSqlServer()
                    .AddSqlServer2000()
                    .AddSqlServer2005()
                    .AddSqlServer2008()
                    .AddSqlServer2012()
                    .AddSqlServer2014()
                    .AddSqlServer2016());
        }
    }
}
