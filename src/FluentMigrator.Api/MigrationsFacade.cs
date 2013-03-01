#region License

// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.IO;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Api
{
    public class MigrationsFacade : IMigrationRunner
    {
        private readonly IAnnouncer _nullAnnouncer = new NullAnnouncer();
        private readonly IRunnerContext _context;

        public MigrationsFacade()
        {
            _context = new FacadeRunnerContext(this);
        }

        public IMigrationProcessor Processor { get; private set; }
        private IMigrationRunner Runner { get; set; }

        /// <summary>Assembly with migrations.</summary>
        public Assembly MigrationAssembly { get; set; }

        public IEnumerable<string> AvailableEngines
        {
            get { return new MigrationProcessorFactoryProvider().ProcessorTypes; }
        }

        /// <summary>File name of assembly with migrations. If assigned an assembly name, it is resolved.</summary>
        public string MigrationAssemblyName
        {
            get { return MigrationAssembly != null ? MigrationAssembly.Location : null; }
            set { MigrationAssembly = new AssemblyLoaderFactory().GetAssemblyLoader(value).Load(); }
        }

        /// <summary>Only preview migrations without actually executing them.</summary>
        public bool PreviewOnly
        {
            get { return _context.PreviewOnly; }
            set
            {
                _context.PreviewOnly = value;
                if (Processor != null)
                    Processor.Options.PreviewOnly = value;
            }
        }

        /// <summary>SQL commands timeout.</summary>
        public int Timeout
        {
            get { return _context.Timeout; }
            set
            {
                _context.Timeout = value;
                if (Processor != null)
                    Processor.Options.Timeout = value;
            }
        }

        /// <summary>Connect to the database.</summary>
        /// <param name="engine">Database provider name. <see cref="AvailableEngines"/>.</param>
        /// <param name="connectionString">Connection string.</param>
        public void OpenConnection(string engine, string connectionString)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");

            var manager = new ConnectionStringManager(new NetConfigManager(), _nullAnnouncer,
                connectionString, null, null, engine);
            manager.LoadConnectionString();
            if (manager.ConnectionString != connectionString)
                throw new InvalidOperationException("Connection string is not used."); // ConnectionStringManager managed to load connection string from somewhere else. Unintended, so fail.

            _context.Database = engine;
            _context.Connection = manager.ConnectionString;
            Connect();
        }

        /// <summary>Connect to the database using a named connection from a config file.</summary>
        /// <param name="engine">Database provider name. <see cref="AvailableEngines"/>.</param>
        /// <param name="connectionStringName">Connection string name. If not specified, <see cref="Environment.MachineName"/> is used.</param>
        /// <param name="configPath">Configuration file path. ".config" extension is optional. If not specified, <see cref="MigrationAssemblyName"/> is used.</param>
        public void OpenNamedConnection(string engine, string connectionStringName = null, string configPath = null)
        {
            var manager = new ConnectionStringManager(new NetConfigManager(), _nullAnnouncer,
                connectionStringName, configPath, MigrationAssemblyName, engine);
            manager.LoadConnectionString();
            if (manager.ConnectionString == connectionStringName)
                throw new FileNotFoundException("Configuration file not found.", configPath);

            _context.Database = engine;
            _context.Connection = manager.ConnectionString;
            _context.ConnectionStringConfigPath = configPath;
            Connect();
        }

        /// <summary>Connect to the database using a named connection from a machine config file.</summary>
        /// <param name="engine">Database provider name. <see cref="AvailableEngines"/>.</param>
        /// <param name="connectionStringName">Connection string name. If not specified, <see cref="Environment.MachineName"/> is used.</param>
        public void OpenMachineNamedConnection(string engine, string connectionStringName = null)
        {
            // TODO Other methods opening connection will fallback to machine config. Five levels of fallbacks are bad, but who cares...
            var manager = new ConnectionStringManager(new NetConfigManager(), _nullAnnouncer,
                connectionStringName, null, null, engine);
            manager.LoadConnectionString();

            _context.Database = engine;
            _context.Connection = manager.ConnectionString;
            Connect();
        }

        public void Up(IMigration migration)
        {
            Runner.Up(migration);
        }

        public void MigrateUp()
        {
            Runner.MigrateUp();
        }

        public void MigrateUp(long version)
        {
            Runner.MigrateUp(version);
        }

        public void Rollback(int steps)
        {
            Runner.Rollback(steps);
        }

        public void RollbackToVersion(long version)
        {
            Runner.RollbackToVersion(version);
        }

        public void MigrateDown(long version)
        {
            Runner.MigrateDown(version);
        }

        public void ValidateVersionOrder()
        {
            Runner.ValidateVersionOrder();
        }

        public void ListMigrations()
        {
            Runner.ListMigrations();
        }

        private void Connect()
        {
            Processor = new MigrationProcessorFactoryProvider().GetFactory(_context.Database).Create(
                _context.Connection, _nullAnnouncer, new ProcessorOptions
                {
                    PreviewOnly = _context.PreviewOnly,
                    Timeout = _context.Timeout,
                });
            Runner = new MigrationRunner(MigrationAssembly, _context, Processor);
        }

        private class FacadeRunnerContext : IRunnerContext
        {
            private readonly MigrationsFacade _facade;

            public FacadeRunnerContext(MigrationsFacade facade)
            {
                _facade = facade;
                StopWatch = new StopWatch();
                Timeout = 30;
            }

            public IStopWatch StopWatch { get; private set; }

            public IAnnouncer Announcer
            {
                get { return _facade._nullAnnouncer; }
            }

            public string Target
            {
                get { return _facade.MigrationAssemblyName; }
                set { throw new NotSupportedException(); }
            }

            public string Database { get; set; }
            public string Connection { get; set; }
            public string ConnectionStringConfigPath { get; set; }

            public bool PreviewOnly { get; set; }
            public int Timeout { get; set; }

            public string Namespace { get; set; }
            public bool NestedNamespaces { get; set; }
            public string Task { get; set; }
            public long Version { get; set; }
            public int Steps { get; set; }
            public string WorkingDirectory { get; set; }
            public string Profile { get; set; }
            public IEnumerable<string> Tags { get; set; }
            public object ApplicationContext { get; set; }
        }
    }
}