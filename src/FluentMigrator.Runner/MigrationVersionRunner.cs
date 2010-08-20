#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Runner
{
	public class MigrationVersionRunner : IMigrationVersionRunner
	{
		private IMigrationConventions _migrationConventions;
		private IMigrationProcessor _migrationProcessor;
		private IMigrationLoader _migrationLoader;
		private Assembly _migrationAssembly;
		private string _namespace;
		private readonly IAnnouncer _announcer;
		private VersionInfo _versionInfo;
		private MigrationRunner _migrationRunner;
		private IMigration _versionMigration;
		private IVersionTableMetaData _versionTableMetaData;
		private SortedList<long, IMigration> _migrations;
		private IEnumerable<IMigration> _profiles;
		private string _profile;
		private bool _alreadyOutputPreviewOnlyModeWarning;
		private bool _alreadyCreatedVersionTable;

		public MigrationVersionRunner(IMigrationConventions conventions, IMigrationProcessor processor, IMigrationLoader loader, IAnnouncer announcer)
			: this(conventions, processor, loader, Assembly.GetCallingAssembly(), null, announcer, string.Empty)
		{
		}

		public MigrationVersionRunner(IMigrationConventions conventions, IMigrationProcessor processor, IMigrationLoader loader, Type getAssemblyByType, IAnnouncer announcer)
			: this(conventions, processor, loader, getAssemblyByType.Assembly, null, announcer, string.Empty)
		{
		}

		public MigrationVersionRunner(IMigrationConventions conventions, IMigrationProcessor processor, IMigrationLoader loader, Assembly assembly, string @namespace, IAnnouncer announcer, string profile)
		{
			_migrationConventions = conventions;
			_migrationProcessor = processor;
			_migrationAssembly = assembly;
			_migrationLoader = loader;
			_profile = profile;
			_namespace = @namespace;
			_announcer = announcer;
			_migrationRunner = new MigrationRunner(conventions, processor, announcer, new StopWatch());
			_versionTableMetaData = loader.GetVersionTableMetaData(assembly);
			_versionMigration = new VersionMigration(_versionTableMetaData);

			LoadVersionInfo();
		}

		public Assembly MigrationAssembly
		{
			get { return _migrationAssembly; }
		}

		public IMigration VersionMigration
		{
			get { return _versionMigration; }
			set { _versionMigration = value; }
		}

		public VersionInfo VersionInfo
		{
			get
			{
				if (_versionInfo == null)
					throw new ArgumentException("VersionInfo was never loaded");

				return _versionInfo;
			}
		}

		private void LoadVersionInfo()
		{
			if (_migrationProcessor.Options.PreviewOnly)
			{
				if (!_alreadyCreatedVersionTable)
				{
					new MigrationRunner(_migrationConventions, _migrationProcessor, _announcer, new StopWatch())
						.Up(_versionMigration);
					_versionInfo = new VersionInfo();
					_alreadyCreatedVersionTable = true;
				}
				else
					_versionInfo = new VersionInfo();

				return;
			}

			if (!_migrationProcessor.TableExists(_versionTableMetaData.TableName))
			{
				var runner = new MigrationRunner(_migrationConventions, _migrationProcessor, _announcer, new StopWatch());
				runner.Up(_versionMigration);
				_versionInfo = new VersionInfo();
				return;
			}

			var dataSet = _migrationProcessor.ReadTableData(_versionTableMetaData.TableName);
			_versionInfo = new VersionInfo();

			foreach (DataRow row in dataSet.Tables[0].Rows)
			{
				_versionInfo.AddAppliedMigration(long.Parse(row[0].ToString()));
			}
		}

		public SortedList<long, IMigration> Migrations
		{
			get
			{
				if (_migrations == null)
					LoadMigrations();
				return _migrations;
			}
		}

		private IEnumerable<IMigration> Profiles
		{
			get
			{
				if (_profiles == null)
					LoadProfiles();

				return _profiles;
			}
		}

		private void LoadMigrations()
		{
			_migrations = new SortedList<long, IMigration>();

			IEnumerable<MigrationMetadata> migrationList = _migrationLoader.FindMigrationsIn(_migrationAssembly, _namespace);

			if (migrationList == null)
				return;

			foreach (var migrationMetadata in migrationList)
			{
				if (_migrations.ContainsKey(migrationMetadata.Version))
					throw new Exception(String.Format("Duplicate migration version {0}.", migrationMetadata.Version));

				var migration = migrationMetadata.Type.Assembly.CreateInstance(migrationMetadata.Type.FullName);
				_migrations.Add(migrationMetadata.Version, migration as IMigration);
			}
		}

		private void LoadProfiles()
		{
			_profiles = new List<IMigration>();

			if (!string.IsNullOrEmpty(_profile))
				_profiles = _migrationLoader.FindProfilesIn(_migrationAssembly, _profile);
		}

		public void MigrateUp()
		{
			try
			{
				foreach (var version in Migrations.Keys)
				{
					MigrateUp(version);
				}

				ApplyProfiles();

				_migrationProcessor.CommitTransaction();

				LoadVersionInfo();
			}
			catch (Exception)
			{
				_migrationProcessor.RollbackTransaction();
				throw;
			}
		}

		public void MigrateUp(long version)
		{
			if (!_alreadyOutputPreviewOnlyModeWarning && _migrationProcessor.Options.PreviewOnly)
			{
				_announcer.Heading("PREVIEW-ONLY MODE");
				_alreadyOutputPreviewOnlyModeWarning = true;
			}

			ApplyMigrationUp(version);

			LoadVersionInfo();
		}

		private void ApplyMigrationUp(long version)
		{
			if (!VersionInfo.HasAppliedMigration(version))
			{
				_migrationRunner.Up(Migrations[version]);
				UpdateVersionInfoWithAppliedMigration(version);
			}
		}

		public void MigrateDown(long version)
		{
			try
			{
				ApplyMigrationDown(version);

				_migrationProcessor.CommitTransaction();

				LoadVersionInfo();
			}
			catch (Exception)
			{
				_migrationProcessor.RollbackTransaction();
				throw;
			}
		}

		private void ApplyProfiles()
		{
			// Run Profile if applicable
			foreach (var profile in Profiles)
			{
				_migrationRunner.Up(profile);
			}
		}

		private void ApplyMigrationDown(long version)
		{
			try
			{
				_migrationRunner.Down(Migrations[version]);
				_migrationProcessor.Execute("DELETE FROM {0} WHERE {1}='{2}'", this._versionTableMetaData.TableName, this._versionTableMetaData.ColumnName, version.ToString());
			}
			catch (KeyNotFoundException ex)
			{
				string msg = string.Format("VersionInfo references version {0} but no Migrator was found attributed with that version.", version);
				throw new Exception(msg, ex);
			}
			catch (Exception ex)
			{
				throw new Exception("Error rolling back version " + version, ex);
			}
		}

		private void UpdateVersionInfoWithAppliedMigration(long version)
		{
			var dataExpression = new InsertDataExpression();
			dataExpression.Rows.Add(CreateVersionInfoInsertionData(version));
			dataExpression.TableName = _versionTableMetaData.TableName;
			dataExpression.ExecuteWith(_migrationProcessor);
		}

		protected virtual InsertionDataDefinition CreateVersionInfoInsertionData(long version)
		{
			return new InsertionDataDefinition { new KeyValuePair<string, object>(this._versionTableMetaData.ColumnName, version) };
		}

		public void Rollback(int steps)
		{
			foreach (var migrationNumber in VersionInfo.AppliedMigrations().Take(steps))
			{
				ApplyMigrationDown(migrationNumber);
			}

			_migrationProcessor.CommitTransaction();

			LoadVersionInfo();
		}

		public void RollbackToVersion(long version)
		{
			// Get the migrations between current and the to version
			foreach (var migrationNumber in VersionInfo.AppliedMigrations())
			{
				if (version < migrationNumber || version == 0)
				{
					ApplyMigrationDown(migrationNumber);
				}
			}

			if (version == 0)
				RemoveVersionTable();

			_migrationProcessor.CommitTransaction();

			LoadVersionInfo();
		}

		public void RemoveVersionTable()
		{
			var expression = new DeleteTableExpression { TableName = this._versionTableMetaData.TableName };
			expression.ExecuteWith(_migrationProcessor);
		}
	}
}
