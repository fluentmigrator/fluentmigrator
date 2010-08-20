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
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
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



	public class MigrationRunner
	{
		private IAnnouncer _announcer;
		public IMigrationConventions Conventions { get; private set; }
		public IMigrationProcessor Processor { get; private set; }
		public IList<Exception> CaughtExceptions { get; private set; }
		public bool SilentlyFail { get; set; }
		private IStopWatch _stopWatch;

		public MigrationRunner(IMigrationConventions conventions, IMigrationProcessor processor, IAnnouncer announcer, IStopWatch stopWatch)
		{
			_announcer = announcer;
			SilentlyFail = false;
			CaughtExceptions = null;
			Conventions = conventions;
			Processor = processor;
			_stopWatch = stopWatch;
		}

		public void Up(IMigration migration)
		{
			var name = migration.GetType().Name;
			_announcer.Heading(name + ": migrating");

			CaughtExceptions = new List<Exception>();

			var context = new MigrationContext(Conventions, Processor);
			migration.GetUpExpressions(context);

			_stopWatch.Start();
			ExecuteExpressions(context.Expressions);
			_stopWatch.Stop();

			_announcer.Say(name + ": migrated");
			_announcer.ElapsedTime(_stopWatch.ElapsedTime());
		}

		public void Down(IMigration migration)
		{
			var name = migration.GetType().Name;
			_announcer.Heading(name + ": reverting");

			CaughtExceptions = new List<Exception>();

			var context = new MigrationContext(Conventions, Processor);
			migration.GetDownExpressions(context);

			_stopWatch.Start();
			ExecuteExpressions(context.Expressions);
			_stopWatch.Stop();

			_announcer.Say(name + ": reverted");
			_announcer.ElapsedTime(_stopWatch.ElapsedTime());
		}

		/// <summary>
		/// execute each migration expression in the expression collection
		/// </summary>
		/// <param name="expressions"></param>
		protected void ExecuteExpressions(ICollection<IMigrationExpression> expressions)
		{
			long insertTicks = 0;
			int insertCount = 0;
			foreach (IMigrationExpression expression in expressions)
			{
				try
				{
					expression.ApplyConventions(Conventions);
					if (expression is InsertDataExpression)
					{
						insertTicks += Time(() => expression.ExecuteWith(Processor));
						insertCount++;
					}
					else
					{
						AnnounceTime(expression.ToString(), () => expression.ExecuteWith(Processor));
					}
				}
				catch (Exception er)
				{
					_announcer.Error(er.Message);

					//catch the error and move onto the next expression
					if (SilentlyFail)
					{
						CaughtExceptions.Add(er);
						continue;
					}
					throw;
				}
			}

			if (insertCount > 0)
			{
				var avg = new TimeSpan(insertTicks / insertCount);
				var msg = string.Format("-> {0} Insert operations completed in {1} taking an average of {2}", insertCount, new TimeSpan(insertTicks), avg);
				_announcer.Say(msg);
			}

		}

		private void AnnounceTime(string message, Action action)
		{
			_announcer.Say(message);

			_stopWatch.Start();
			action();
			_stopWatch.Stop();

			_announcer.ElapsedTime(_stopWatch.ElapsedTime());
		}

		private long Time(Action action)
		{
			_stopWatch.Start();

			action();

			_stopWatch.Stop();

			return _stopWatch.ElapsedTime().Ticks;
		}
	}
}