#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using System.IO;
using System.Reflection;

using FluentMigrator.Exceptions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Initialization.NetFramework;
using FluentMigrator.Runner.Processors;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.MSBuild
{
    [CLSCompliant(false)]
    public class Migrate :
#if NETFRAMEWORK
        AppDomainIsolatedTask
#else
        Task
#endif
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Migrate"/> class.
        /// </summary>
        public Migrate()
        {
            AppDomain.CurrentDomain.ResourceResolve += CurrentDomain_ResourceResolve;
        }

        private static Assembly CurrentDomain_ResourceResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine(@"Could Not Resolve {0}", args.Name);
            return null;
        }

        private string _databaseType;

        public string ApplicationContext { get; set; }

        [Required]
        public string Connection { get; set; }

        public string ConnectionStringConfigPath { get; set; }

        public string Target { get { return (Targets != null && Targets.Length == 1) ? Targets[0] : string.Empty; } set { Targets = new[] { value }; } }

        public string[] Targets { get; set; }
        public string MigrationAssembly { get { return (Targets != null && Targets.Length == 1) ? Targets[0] : string.Empty; } set { Targets = new[] {value}; } }

        public string Database { get { return _databaseType; } set { _databaseType = value; } }

        public string DatabaseType { get { return _databaseType; } set { _databaseType = value; } }

        public bool Verbose { get; set; }

        public string Namespace { get; set; }

        public bool Nested { get; set; }

        public string Task { get; set; }

        public long Version { get; set; }

        public int Steps { get; set; }

        public string WorkingDirectory { get; set; }

        public int? Timeout { get; set; }

        public string Profile { get; set; }

        public bool PreviewOnly { get; set; }

        public string Tags { get; set; }

        public bool Output { get; set; }

        public string OutputFilename { get; set; }

        public bool TransactionPerSession { get; set; }

        public bool AllowBreakingChange { get; set; }

        public string ProviderSwitches { get; set; }

        public override bool Execute()
        {

            if (string.IsNullOrEmpty(_databaseType))
            {
                Log.LogError("You must specify a database type. i.e. mysql or sqlserver");
                return false;
            }

            if (Targets == null || Targets.Length == 0)
            {
                Log.LogError("You must specify a migration assemblies ");
                return false;
            }

            IAnnouncer announcer = new ConsoleAnnouncer
            {
                ShowElapsedTime = Verbose,
                ShowSql = Verbose
            };

            StreamWriter outputWriter = null;
            if (Output)
            {
                if (string.IsNullOrEmpty(OutputFilename))
                    OutputFilename = Path.GetFileName(Target) + ".sql";

                outputWriter = new StreamWriter(OutputFilename);
                var fileAnnouncer = new TextWriterAnnouncer(outputWriter)
                {
                    ShowElapsedTime = false,
                    ShowSql = true
                };

                announcer = new CompositeAnnouncer(announcer, fileAnnouncer);
            }

            Log.LogMessage(MessageImportance.Low, "Executing Migration Runner");
            try
            {
                Log.LogMessage(MessageImportance.Low, "Creating Context");

                ExecuteMigrations(announcer);
            }
            catch (ProcessorFactoryNotFoundException ex)
            {
                Log.LogError("While executing migrations the following error was encountered: {0}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Log.LogError("While executing migrations the following error was encountered: {0}, {1}", ex.Message, ex.StackTrace);
                return false;
            }
            finally
            {
                outputWriter?.Dispose();
            }

            return true;
        }

        private void ExecuteMigrations(IAnnouncer announcer)
        {
            var conventionSet = new DefaultConventionSet(defaultSchemaName: null, WorkingDirectory);

            var services = CreateCoreServices()
                    .AddSingleton<IConventionSet>(conventionSet)
                    .AddScoped<TaskExecutor>()
                    .ConfigureRunner(r => r.WithAnnouncer(announcer))
                    .Configure<SelectingProcessorAccessorOptions>(opt => opt.ProcessorId = DatabaseType)
                    .Configure<AssemblySourceOptions>(opt => opt.AssemblyNames = Targets)
                    .Configure<AppConfigConnectionStringAccessorOptions>(
                        opt => opt.ConnectionStringConfigPath = ConnectionStringConfigPath)
                    .Configure<RunnerOptions>(
                        opt =>
                        {
                            opt.Task = Task;
                            opt.Version = Version;
                            opt.Steps = Steps;
                            opt.Profile = Profile;
                            opt.Tags = Tags.ToTags().ToArray();
#pragma warning disable 612
                            opt.ApplicationContext = ApplicationContext;
#pragma warning restore 612
                            opt.TransactionPerSession = TransactionPerSession;
                            opt.AllowBreakingChange = AllowBreakingChange;
                        })
                    .Configure<ProcessorOptions>(
                        opt =>
                        {
                            opt.ConnectionString = Connection;
                            opt.PreviewOnly = PreviewOnly;
                            opt.ProviderSwitches = ProviderSwitches;
                            opt.Timeout = Timeout == null ? null : (TimeSpan?) TimeSpan.FromSeconds(Timeout.Value);
                        });

            using (var serviceProvider = services.BuildServiceProvider(validateScopes: false))
            {
                var executor = serviceProvider.GetRequiredService<TaskExecutor>();
                executor.Execute();
            }
        }

        private static IServiceCollection CreateCoreServices()
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder => builder
                        .AddDb2()
                        .AddDb2ISeries()
                        .AddDotConnectOracle()
                        .AddFirebird()
                        .AddHana()
                        .AddMySql4()
                        .AddMySql5()
                        .AddOracle()
                        .AddOracleManaged()
                        .AddPostgres()
                        .AddRedshift()
                        .AddSqlAnywhere()
                        .AddSQLite()
                        .AddSqlServer()
                        .AddSqlServer2000()
                        .AddSqlServer2005()
                        .AddSqlServer2008()
                        .AddSqlServer2012()
                        .AddSqlServer2014()
                        .AddSqlServer2016()
                        .AddSqlServerCe());
            return services;
        }
    }
}
