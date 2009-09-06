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
	public interface IMigrationVersionRunner
	{
		Assembly MigrationAssembly { get; }
		VersionInfo VersionInfo { get; }
		void MigrateUp();
		void MigrateUp(long version);
		void Rollback(int steps);
		void MigrateDown(long version);
		void RemoveVersionTable();
	}

	public class MigrationVersionRunner : IMigrationVersionRunner
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
			foreach (var version in Migrations.Keys)
			{
				MigrateUp(version);
			}
			_versionInfo = null;
		}

		public void MigrateUp(long version)
		{
			migrateUp(version);
			_versionInfo = null;
		}

		private void migrateUp(long version)
		{
			if (!VersionInfo.HasAppliedMigration(version))
			{
				_migrationRunner.Up(Migrations[version]);
				updateVersionInfoWithAppliedMigration(version);
			}
		}

		private void updateVersionInfoWithAppliedMigration(long version)
		{
			var dataExpression = new InsertDataExpression();
			var data = new InsertionData {new KeyValuePair<string, object>(VersionInfo.COLUMN_NAME, version)};
			dataExpression.Rows.Add(data);
			dataExpression.TableName = VersionInfo.TABLE_NAME;
			dataExpression.ExecuteWith(_migrationProcessor);
		}

		public void Rollback(int steps)
		{
			foreach (var migrationNumber in VersionInfo.AppliedMigrations().Take(steps))
			{
				migrateDown(migrationNumber);
			}
			_versionInfo = null;
		}

		public void MigrateDown(long version)
		{
			migrateDown(version);
			_versionInfo = null;
		}

		private void migrateDown(long version)
		{
			_migrationRunner.Down(Migrations[version]);
			_migrationProcessor.Execute("DELETE FROM {0} WHERE {1}='{2}'", VersionInfo.TABLE_NAME, VersionInfo.COLUMN_NAME, version.ToString());
		}

		public void RemoveVersionTable()
		{
			var expression = new DeleteTableExpression {TableName = VersionInfo.TABLE_NAME};
			expression.ExecuteWith(_migrationProcessor);
		}
	}
}
