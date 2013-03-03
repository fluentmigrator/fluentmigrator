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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Api
{
    /// <summary>
    /// Simple API for Fluent Migrations.
    /// Recommended order of calls:
    /// <list type="number">
    /// <item><see cref="Conventions"/> (derive from <see cref="MigrationConventionsBase"/>),
    /// <see cref="VersionTable"/> and other options, if necessary;</item>
    /// <item><see cref="LoadMigrations(Assembly,string,bool,IEnumerable{string})"/> or
    /// <see cref="LoadMigrations(string,string,bool,IEnumerable{string})"/> to load the assembly with migrations;</item>
    /// <item><see cref="OpenConnection"/> or <see cref="OpenNamedConnection"/> to connect to the database;</item>
    /// <item>Migration methods from <see cref="IMigrationRunner"/> interface and other;</item>
    /// <item><see cref="Dispose"/> to close the connection.</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Unsupported features:
    /// <list type="number">
    /// <item>Announcers (tracing with System.Diagnostics is planned);</item>
    /// <item>Detailed timing and counters (log to structures?);</item>
    /// <item>Silent failing (swallowing and ignoring exceptions is a bad idea; fix your code).</item>
    /// </list>
    /// TODO: Tracing compatible with System.Diagnostics
    /// TODO: Logging timing and counters to some structures
    /// </remarks>
    public class Migrator : IMigrationRunner, IDisposable
    {
        private readonly IAnnouncer _nullAnnouncer = new NullAnnouncer();
        private readonly FacadeRunnerContext _context;
        private IVersionLoader _versionLoader;
        private IMigrationInformationLoader _migrationLoader;

        public Migrator()
        {
            _context = new FacadeRunnerContext(this);
            Conventions = new MigrationConventionsBase();
            VersionTable = new VersionTableMetaData();
        }

        public void Dispose()
        {
            Processor.Dispose();
        }

        internal IMigrationProcessor Processor { get; private set; }

        /// <summary>Migrations conventions. Assign an object of class <see cref="MigrationConventionsBase"/> if you need to customize generation of names for schema objects or search for migration classes in assembly.</summary>
        public IMigrationConventions Conventions { get; set; }

        /// <summary>Version table options. By default, table is named "VersionInfo".</summary>
        public VersionTableMetaData VersionTable { get; private set; }

        IMigrationProcessor IMigrationRunner.Processor
        {
            get { return Processor; }
        }

        public Assembly MigrationAssembly
        {
            get { return _context.MigrationAssembly; }
        }

        public IEnumerable<string> AvailableEngines
        {
            get { return new MigrationProcessorFactoryProvider().ProcessorTypes; }
        }

        /// <summary>Directory to load SQL scripts from if relative path is specified. If null, application working directory is used.</summary>
        public string SqlScriptsDirectory
        {
            get { return Conventions.GetWorkingDirectory(); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Conventions.GetWorkingDirectory = DefaultMigrationConventions.GetWorkingDirectory;
                else
                    Conventions.GetWorkingDirectory = () => value;
            }
        }

        /// <summary>Arbitrary application context passed to the task runner.</summary>
        public object ApplicationContext
        {
            get { return _context.ApplicationContext; }
            set { _context.ApplicationContext = value; }
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

        /// <summary>Load migrations from a named assembly.</summary>
        /// <param name="assemblyName">Either assembly name or assembly file name.</param>
        /// <param name="ns">Namespace to load migration classes from.</param>
        /// <param name="loadNestedNamespaces">Wether to load migration classes from nested namesapces.</param>
        /// <param name="tagsToMatch">Filter migrations by tags.</param>
        public void LoadMigrations(string assemblyName, string ns,
            bool loadNestedNamespaces, IEnumerable<string> tagsToMatch)
        {
            _context.MigrationAssemblyName = assemblyName;
            LoadMigrations(_context.MigrationAssembly, ns, loadNestedNamespaces, tagsToMatch);
        }

        /// <summary>Load migrations from an assembly.</summary>
        /// <param name="assembly">Assembly object.</param>
        /// <param name="ns">Namespace to load migration classes from.</param>
        /// <param name="loadNestedNamespaces">Wether to load migration classes from nested namesapces.</param>
        /// <param name="tagsToMatch">Filter migrations by tags.</param>
        public void LoadMigrations(Assembly assembly, string ns,
            bool loadNestedNamespaces, IEnumerable<string> tagsToMatch)
        {
            _context.MigrationAssembly = assembly;
            _migrationLoader = new DefaultMigrationInformationLoader(Conventions, _context.MigrationAssembly, ns, loadNestedNamespaces, tagsToMatch);
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
        /// <param name="configPath">Configuration file path. ".config" extension is optional. If not specified, migration assembly name is used.</param>
        public void OpenNamedConnection(string engine, string connectionStringName = null, string configPath = null)
        {
            var manager = new ConnectionStringManager(new NetConfigManager(), _nullAnnouncer,
                connectionStringName, configPath, _context.MigrationAssemblyName, engine);
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

        private void Connect()
        {
            Processor = new MigrationProcessorFactoryProvider().GetFactory(_context.Database).Create(
                _context.Connection, _nullAnnouncer, new ProcessorOptions
                {
                    PreviewOnly = _context.PreviewOnly,
                    Timeout = _context.Timeout,
                });
            _versionLoader = new VersionLoader(this, _context.MigrationAssembly, Conventions);
        }

        public QueryMigration GetQuery()
        {
            return new QueryMigration(this, Processor);
        }

        /// <summary>Apply profile migrations. They are always run, no version checks are used. Only <see cref="Migration.Up"/> method is called. Usually profile migrations are executed after usual migrations.</summary>
        /// <param name="profile">Search for migrations with <see cref="ProfileAttribute.ProfileName"/> set to this value.</param>
        public void ApplyProfile(string profile)
        {
            if (profile == null)
                throw new ArgumentNullException("profile");
            _context.Profile = profile;
            var profileLoader = new ProfileLoader(_context, this, Conventions);
            profileLoader.ApplyProfiles();
        }

        /// <summary>Migrate up to the highest version.</summary>
        public void MigrateUp()
        {
            foreach (KeyValuePair<long, IMigrationInfo> migration in _migrationLoader.LoadMigrations())
                ApplyMigrationUp(migration.Value);
            _versionLoader.LoadVersionInfo();
        }

        /// <summary>Migrate up to a specific version.</summary>
        public void MigrateUp(long targetVersion)
        {
            var migrations = _migrationLoader.LoadMigrations()
                .Where(pair => IsMigrationStepNeededForUpMigration(pair.Key, targetVersion))
                .Select(pair => pair.Value);

            foreach (IMigrationInfo migration in migrations)
                ApplyMigrationUp(migration);
            _versionLoader.LoadVersionInfo();
        }

        /// <summary>Migrate down to a specific version.</summary>
        public void MigrateDown(long targetVersion)
        {
            var migrations = (IEnumerable<IMigrationInfo>)_migrationLoader.LoadMigrations()
                .Where(pair => IsMigrationStepNeededForDownMigration(pair.Key, targetVersion))
                .Select(pair => pair.Value)
                .OrderByDescending(x => x.Version);

            foreach (IMigrationInfo migration in migrations)
                ApplyMigrationDown(migration);
            _versionLoader.LoadVersionInfo();
        }

        /// <summary>Rollback specified number of migrations.</summary>
        public void Rollback(int steps)
        {
            IDictionary<long, IMigrationInfo> availableMigrations = _migrationLoader.LoadMigrations();
            IEnumerable<IMigrationInfo> migrationsToRollback = _versionLoader.VersionInfo.AppliedMigrations()
                .Where(availableMigrations.ContainsKey)
                .Select(version => availableMigrations[version])
                .Take(steps);

            foreach (IMigrationInfo migration in migrationsToRollback)
                ApplyMigrationDown(migration);
            _versionLoader.LoadVersionInfo();
            if (!_versionLoader.VersionInfo.AppliedMigrations().Any())
                _versionLoader.RemoveVersionTable();
        }

        public void RollbackToVersion(long targetVersion)
        {
            IDictionary<long, IMigrationInfo> availableMigrations = _migrationLoader.LoadMigrations();
            IEnumerable<IMigrationInfo> migrationsToRollback = _versionLoader.VersionInfo.AppliedMigrations()
                .Where(availableMigrations.ContainsKey)
                .Select(appliedVersion => availableMigrations[appliedVersion])
                .Where(migration => targetVersion < migration.Version);

            foreach (IMigrationInfo migration in migrationsToRollback)
                ApplyMigrationDown(migration);
            _versionLoader.LoadVersionInfo();
            if (targetVersion == 0 && !_versionLoader.VersionInfo.AppliedMigrations().Any())
                _versionLoader.RemoveVersionTable();
        }

        internal void ProcessQuery(QueryMigration migration, IMigrationContext context)
        {
            try
            {
                Processor.BeginTransaction();
                ExecuteMigration(migration, context);
                Processor.CommitTransaction();
            }
            catch
            {
                Processor.RollbackTransaction();
                throw;
            }
        }

        /// <summary>Run the specified migration up.</summary>
        public void Up(IMigration migration)
        {
            try
            {
                Processor.BeginTransaction();
                ExecuteMigration(migration, migration.GetUpExpressions);
                Processor.CommitTransaction();
            }
            catch
            {
                Processor.RollbackTransaction();
                throw;
            }
        }

        /// <summary>Run the specified migration down.</summary>
        public void Down(IMigration migration)
        {
            try
            {
                Processor.BeginTransaction();
                ExecuteMigration(migration, migration.GetDownExpressions);
                Processor.CommitTransaction();
            }
            catch
            {
                Processor.RollbackTransaction();
                throw;
            }
        }

        void IMigrationRunner.ValidateVersionOrder()
        {
            var unappliedVersions = GetUnappliedVersions();
            if (unappliedVersions.Any())
                throw new VersionOrderInvalidException(unappliedVersions);
        }

        public bool IsVersionOrderValid()
        {
            return !GetUnappliedVersions().Any();
        }

        void IMigrationRunner.ListMigrations()
        {
            // Never used, as announcer is null
        }

        /// <summary>Get all migrations. The key of KeyValuePair is wether the migration has been applied.</summary>
        public IEnumerable<KeyValuePair<bool, IMigrationInfo>> GetMigrations()
        {
            long currentVersion = _versionLoader.VersionInfo.Latest();
            return _migrationLoader.LoadMigrations()
                .Select(migration => new KeyValuePair<bool, IMigrationInfo>(migration.Key == currentVersion, migration.Value));
        }

        public IEnumerable<IMigrationInfo> GetAppliedMigrations()
        {
            return GetMigrations().Where(m => m.Key).Select(m => m.Value);
        }

        public IEnumerable<IMigrationInfo> GetUnappliedMigrations()
        {
            return GetMigrations().Where(m => !m.Key).Select(m => m.Value);
        }

        private void ExecuteMigration(object migration, Action<IMigrationContext> getExpressions)
        {
            var context = new MigrationContext(Conventions, Processor, _context.MigrationAssembly, ApplicationContext);
            getExpressions(context);
            ExecuteMigration(migration, context);
        }

        private void ExecuteMigration(object migration, IMigrationContext context)
        {
            var invalidExpressions = new Dictionary<string, string>();
            foreach (IMigrationExpression expression in context.Expressions)
            {
                expression.ApplyConventions(Conventions);
                var errors = new Collection<string>();
                expression.CollectValidationErrors(errors);
                if (errors.Count > 0)
                    invalidExpressions.Add(expression.GetType().Name, string.Join(" ", errors.ToArray()));
            }

            if (invalidExpressions.Count > 0)
                throw new InvalidMigrationException(migration, invalidExpressions);

            foreach (IMigrationExpression expression in context.Expressions)
                expression.ExecuteWith(Processor);
        }

        private void ApplyMigrationUp(IMigrationInfo migration)
        {
            if (_versionLoader.VersionInfo.HasAppliedMigration(migration.Version))
                return;

            ApplyMigration(migration, () =>
            {
                ExecuteMigration(migration.Migration, migration.Migration.GetUpExpressions);
                _versionLoader.UpdateVersionInfo(migration.Version);
            });
        }

        private void ApplyMigrationDown(IMigrationInfo migration)
        {
            ApplyMigration(migration, () =>
            {
                ExecuteMigration(migration.Migration, migration.Migration.GetDownExpressions);
                _versionLoader.DeleteVersion(migration.Version);
            });
        }

        private void ApplyMigration(IMigrationInfo migration, Action applyMigration)
        {
            bool useTransaction = migration.TransactionBehavior == TransactionBehavior.Default;
            try
            {
                if (useTransaction)
                    Processor.BeginTransaction();
                applyMigration();
                if (useTransaction)
                    Processor.CommitTransaction();
            }
            catch
            {
                if (useTransaction)
                    Processor.RollbackTransaction();
                throw;
            }
        }

        private List<KeyValuePair<long, IMigrationInfo>> GetUnappliedVersions()
        {
            return _migrationLoader.LoadMigrations()
                .Where(kvp => IsMigrationVersionLessThanGreatestAppliedMigration(kvp.Key))
                .ToList();
        }

        private bool IsMigrationStepNeededForUpMigration(long versionOfMigration, long targetVersion)
        {
            return versionOfMigration <= targetVersion
                && !_versionLoader.VersionInfo.HasAppliedMigration(versionOfMigration);
        }

        private bool IsMigrationStepNeededForDownMigration(long versionOfMigration, long targetVersion)
        {
            return versionOfMigration > targetVersion
                && _versionLoader.VersionInfo.HasAppliedMigration(versionOfMigration);
        }

        private bool IsMigrationVersionLessThanGreatestAppliedMigration(long version)
        {
            return !_versionLoader.VersionInfo.HasAppliedMigration(version)
                && version < _versionLoader.VersionInfo.Latest();
        }

        private class FacadeRunnerContext : IRunnerContext
        {
            private readonly Migrator _facade;

            public FacadeRunnerContext(Migrator facade)
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

            // LoadMigrations methods arguments (only assembly is set, others are never assigned)

            public Assembly MigrationAssembly { get; set; }
            public string Namespace { get; set; }
            public IEnumerable<string> Tags { get; set; }
            public bool NestedNamespaces { get; set; }

            public string MigrationAssemblyName
            {
                get { return MigrationAssembly != null ? MigrationAssembly.Location : null; }
                set { MigrationAssembly = new AssemblyLoaderFactory().GetAssemblyLoader(value).Load(); }
            }

            string IRunnerContext.Target
            {
                get { return MigrationAssemblyName; }
                set { MigrationAssemblyName = value; }
            }

            // OpenConnection methods arguments (implicit arguments for Connect method)

            public string Database { get; set; }
            public string Connection { get; set; }
            public string ConnectionStringConfigPath { get; set; }

            // Properties of facade runner

            public object ApplicationContext { get; set; }

            // Properties from options objects (proxied)

            public string WorkingDirectory { get; set; }
            public bool PreviewOnly { get; set; }
            public int Timeout { get; set; }

            // Migration methods arguments (never assigned)

            public string Task { get; set; }
            public long Version { get; set; }
            public int Steps { get; set; }
            public string Profile { get; set; }
        }

        public class VersionTableMetaData : IVersionTableMetaData
        {
            public VersionTableMetaData()
            {
                var def = new DefaultVersionTableMetaData();
                SchemaName = def.SchemaName;
                TableName = def.TableName;
                ColumnName = def.ColumnName;
                UniqueIndexName = def.UniqueIndexName;
            }

            public string SchemaName { get; set; }
            public string TableName { get; set; }
            public string ColumnName { get; set; }
            public string UniqueIndexName { get; set; }
        }
    }
}