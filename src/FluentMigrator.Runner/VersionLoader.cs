using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Versioning;

namespace FluentMigrator.Runner
{
	public class VersionLoader : IVersionLoader
	{
		private void UpdateVersionInfo( long version )
		{
			var dataExpression = new InsertDataExpression();
			dataExpression.Rows.Add( CreateVersionInfoInsertionData( version ) );
			dataExpression.TableName = _versionTableMetaData.TableName;
			dataExpression.ExecuteWith( _migrationProcessor );
		}

		protected virtual InsertionDataDefinition CreateVersionInfoInsertionData( long version )
		{
			return new InsertionDataDefinition { new KeyValuePair<string, object>( this._versionTableMetaData.ColumnName, version ) };
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
				if ( _versionInfo == null )
					throw new ArgumentException( "VersionInfo was never loaded" );

				return _versionInfo;
			}
		}

		private void LoadVersionInfo()
		{
			if ( _migrationProcessor.Options.PreviewOnly )
			{
				if ( !_alreadyCreatedVersionTable )
				{
					new MigrationRunner( _migrationConventions, _migrationProcessor, _announcer, new StopWatch() )
						.Up( _versionMigration );
					_versionInfo = new VersionInfo();
					_alreadyCreatedVersionTable = true;
				}
				else
					_versionInfo = new VersionInfo();

				return;
			}

			if ( !_migrationProcessor.TableExists( _versionTableMetaData.TableName ) )
			{
				var runner = new MigrationRunner( _migrationConventions, _migrationProcessor, _announcer, new StopWatch() );
				runner.Up( _versionMigration );
				_versionInfo = new VersionInfo();
				return;
			}

			var dataSet = _migrationProcessor.ReadTableData( _versionTableMetaData.TableName );
			_versionInfo = new VersionInfo();

			foreach ( DataRow row in dataSet.Tables[ 0 ].Rows )
			{
				_versionInfo.AddAppliedMigration( long.Parse( row[ 0 ].ToString() ) );
			}
		}

		public void RemoveVersionTable()
		{
			var expression = new DeleteTableExpression { TableName = this._versionTableMetaData.TableName };
			expression.ExecuteWith( _migrationProcessor );
		}
	}
}