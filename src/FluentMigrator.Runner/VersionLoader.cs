using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Runner
{
	public class VersionLoader : IVersionLoader
	{
		public VersionLoader(IMigrationRunner runner, IRunnerContext runnerContext, Assembly assembly, IMigrationConventions conventions)
		{
			Runner = runner;
			RunnerContext = runnerContext;
			Processor = runnerContext.Processor;
			Announcer = runnerContext.Announcer;
			Assembly = assembly;

			Conventions = conventions;
			VersionTableMetaData = GetVersionTableMetaData();
			VersionMigration = new VersionMigration(VersionTableMetaData);
		}

		public IMigrationRunner Runner { get; set; }
		private IRunnerContext RunnerContext { get; set; }
		protected Assembly Assembly { get; set; }
		public IVersionTableMetaData VersionTableMetaData { get; set; }
		private IMigrationConventions Conventions { get; set; }
		private IMigrationProcessor Processor { get; set; }
		private IMigration VersionMigration { get; set; }
		private IAnnouncer Announcer { get; set; }

		public void UpdateVersionInfo( long version )
		{
			var dataExpression = new InsertDataExpression();
			dataExpression.Rows.Add( CreateVersionInfoInsertionData( version ) );
			dataExpression.TableName = VersionTableMetaData.TableName;
			dataExpression.ExecuteWith( Processor );
		}

		public IVersionTableMetaData GetVersionTableMetaData()
		{
			Type matchedType = Assembly.GetExportedTypes().Where(t => Conventions.TypeIsVersionTableMetaData(t)).FirstOrDefault();

			if (matchedType == null)
			{
				return new DefaultVersionTableMetaData();
			}

			return (IVersionTableMetaData)Activator.CreateInstance(matchedType);
		}

		protected virtual InsertionDataDefinition CreateVersionInfoInsertionData( long version )
		{
			return new InsertionDataDefinition { new KeyValuePair<string, object>( this.VersionTableMetaData.ColumnName, version ) };
		}

		private VersionInfo _versionInfo;

		public VersionInfo VersionInfo
		{
			get
			{
				if ( _versionInfo == null )
					throw new ArgumentException( "VersionInfo was never loaded" );

				return _versionInfo;
			}
			set
			{
				_versionInfo = VersionInfo;
			}
		}

		public bool AlreadyCreatedVersionTable
		{
			get
			{
				return Processor.TableExists(VersionTableMetaData.TableName);
			}
		}

		public void LoadVersionInfo()
		{
			if ( Processor.Options.PreviewOnly )
			{
				if ( !AlreadyCreatedVersionTable )
				{
					Runner.Up( VersionMigration );
					VersionInfo = new VersionInfo();
				}
				else
					VersionInfo = new VersionInfo();

				return;
			}

			if ( !AlreadyCreatedVersionTable )
			{
				Runner.Up( VersionMigration );
				_versionInfo = new VersionInfo();
				return;
			}

			var dataSet = Processor.ReadTableData( VersionTableMetaData.TableName );
			_versionInfo = new VersionInfo();

			foreach ( DataRow row in dataSet.Tables[ 0 ].Rows )
			{
				_versionInfo.AddAppliedMigration( long.Parse( row[ 0 ].ToString() ) );
			}
		}

		public void RemoveVersionTable()
		{
			var expression = new DeleteTableExpression { TableName = VersionTableMetaData.TableName };
			expression.ExecuteWith( Processor );
		}

		public void DeleteVersion(long version)
		{
			Processor.Execute("DELETE FROM {0} WHERE {1}='{2}'", VersionTableMetaData.TableName, VersionTableMetaData.ColumnName, version.ToString());
		}
	}
}