using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Versioning;
using System.Linq;

namespace FluentMigrator.Runner
{
    public class MigrationVersionRunner
    {
        private IMigrationConventions _migrationConventions;
        private IMigrationProcessor _migrationProcessor;
        private IMigrationLoader _migrationLoader;
        private Assembly _migrationAssembly;
        private string _namespace;
        private VersionInfo _versionInfo;
        private MigrationRunner _migrationRunner;

        public MigrationVersionRunner(IMigrationConventions conventions, IMigrationProcessor processor, IMigrationLoader loader)
			: this(conventions, processor, loader, Assembly.GetCallingAssembly(), null)
		{
		}

		public MigrationVersionRunner(IMigrationConventions conventions, IMigrationProcessor processor, IMigrationLoader loader, Type getAssemblyByType)
			: this(conventions, processor, loader, getAssemblyByType.Assembly, null)
		{
		}

		public MigrationVersionRunner(IMigrationConventions conventions, IMigrationProcessor processor, IMigrationLoader loader, Assembly assembly, string @namespace)
		{
			_migrationConventions = conventions;
			_migrationProcessor = processor;
            _migrationAssembly = assembly;
		    _migrationLoader = loader;
		    _namespace = @namespace;
            _migrationRunner = new MigrationRunner(conventions, processor);
		}

        public Assembly MigrationAssembly
        {
            get { return _migrationAssembly; }
        }

        public VersionInfo VersionInfo
        {
            get
            {
                if (_versionInfo == null)
                    loadVersionInfo();
                return _versionInfo;
            }
        }

        private void loadVersionInfo()
        {
            if (!_migrationProcessor.TableExists(VersionInfo.TABLE_NAME))
            {
                var runner = new MigrationRunner(_migrationConventions, _migrationProcessor);
                runner.Up(new VersionMigration());
            }
            var dataSet = _migrationProcessor.ReadTableData(VersionInfo.TABLE_NAME);
            _versionInfo = new VersionInfo();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                _versionInfo.AddAppliedMigration(long.Parse(row[0].ToString()));
            }
        }

        private SortedList<long, Migration> _migrations;
        public SortedList<long, Migration> Migrations
        {
            get
            {
                if (_migrations == null)
                    loadMigrations();
                return _migrations;
            }
        }


        private void loadMigrations()
        {
            _migrations = new SortedList<long, Migration>();
            IEnumerable<MigrationMetadata> migrationList;
            
            if (string.IsNullOrEmpty(_namespace))
                migrationList = _migrationLoader.FindMigrationsIn(_migrationAssembly);
            else
                migrationList = _migrationLoader.FindMigrationsIn(_migrationAssembly, _namespace);

            if (migrationList == null)
                return;

            foreach (var migrationMetadata in migrationList)
            {
                if (_migrations.ContainsKey(migrationMetadata.Version))
                    throw new Exception(String.Format("Duplicate migration version {0}.", migrationMetadata.Version));

                var migration = migrationMetadata.Type.Assembly.CreateInstance(migrationMetadata.Type.FullName);
                _migrations.Add(migrationMetadata.Version, migration as Migration);
            }
        }

        public void MigrateUp()
        {
            var appliedMigrations = new List<long>();
            foreach (var migrationNumber in Migrations.Keys)
            {
                if (!VersionInfo.HasAppliedMigration(migrationNumber))
                {
                    _migrationRunner.Up(Migrations[migrationNumber]);
                    appliedMigrations.Add(migrationNumber);
                }
            }
            updateVersionInfoWithAppliedMigrations(appliedMigrations);
            _versionInfo = null;
        }

        private void updateVersionInfoWithAppliedMigrations(IEnumerable<long> migrations)
        {
            foreach (var migration in migrations)
            {
                var dataExpression = new InsertDataExpression();
                var data = new InsertionData {new KeyValuePair<string, object>(VersionInfo.COLUMN_NAME, migration)};
                dataExpression.Rows.Add(data);
                dataExpression.TableName = VersionInfo.TABLE_NAME;
                dataExpression.ExecuteWith(_migrationProcessor);
            }
        }

        public void Rollback(int steps)
        {
            foreach (var migrationNumber in Migrations.Keys.OrderByDescending(x => x).Take(steps))
            {
                _migrationRunner.Down(Migrations[migrationNumber]);
                _migrationProcessor.DeleteWhere(VersionInfo.TABLE_NAME, VersionInfo.COLUMN_NAME, migrationNumber.ToString());
            }
            _versionInfo = null;
        }

        public void RemoveVersionTable()
        {
            var expression = new DeleteTableExpression {TableName = VersionInfo.TABLE_NAME};
            expression.ExecuteWith(_migrationProcessor);
        }
    }
}
