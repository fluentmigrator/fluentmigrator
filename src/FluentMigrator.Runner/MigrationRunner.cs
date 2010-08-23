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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner
{
	public class MigrationRunner : IMigrationRunner
	{
		private Assembly _migrationAssembly;
		private IAnnouncer _announcer;
		private IStopWatch _stopWatch;
		private bool _alreadyOutputPreviewOnlyModeWarning;
		public bool SilentlyFail { get; set; }

		public IMigrationProcessor Processor { get; set; }
		public IMigrationLoader MigrationLoader { get; set; }
		public IProfileLoader ProfileLoader { get; set; }
		public IMigrationConventions Conventions { get; private set; }
		public IList<Exception> CaughtExceptions { get; private set; }

		public MigrationRunner(Assembly assembly, IRunnerContext runnerContext)
		{
			_migrationAssembly = assembly;
			_announcer = runnerContext.Announcer;
			
			_stopWatch = runnerContext.StopWatch;

			InitializeProcessor(runnerContext);

			SilentlyFail = false;
			CaughtExceptions = null;

			Conventions = new MigrationConventions();
			if (!string.IsNullOrEmpty(runnerContext.WorkingDirectory))
				Conventions.GetWorkingDirectory = () => runnerContext.WorkingDirectory;

			Processor.BeginTransaction();

			VersionLoader = new VersionLoader(this, runnerContext, _migrationAssembly, Conventions);
			MigrationLoader = new MigrationLoader(Conventions, _migrationAssembly, runnerContext.Namespace);
			ProfileLoader = new ProfileLoader(runnerContext, this, Conventions);
		}

		private string ConfigFile { get; set; }
		private string ConnectionString;
		private void InitializeProcessor(IRunnerContext runnerContext)
		{
			var configFile = Path.Combine( Environment.CurrentDirectory, runnerContext.Target );
			if ( File.Exists( configFile + ".config" ) )
			{
				var config = ConfigurationManager.OpenExeConfiguration( configFile );
				var connections = config.ConnectionStrings.ConnectionStrings;

				if ( connections.Count > 1 )
				{
					if ( string.IsNullOrEmpty( runnerContext.Connection ) )
					{
						ReadConnectionString(runnerContext, connections[ Environment.MachineName ], config.FilePath );
					}
					else
					{
						ReadConnectionString(runnerContext, connections[ runnerContext.Connection ], config.FilePath );
					}
				}
				else if ( connections.Count == 1 )
				{
					ReadConnectionString( runnerContext, connections[ 0 ], config.FilePath );
				}
			}

			if ( NotUsingConfig && !string.IsNullOrEmpty( runnerContext.Connection ) )
			{
				ConnectionString = runnerContext.Connection;
			}

			if ( string.IsNullOrEmpty( ConnectionString ) )
			{
				throw new ArgumentException( "Connection String or Name is required \"/connection\"" );
			}

			if ( string.IsNullOrEmpty( runnerContext.Database ) )
			{
				throw new ArgumentException(
					"Database Type is required \"/db [db type]\". Available db types is [sqlserver], [sqlite]" );
			}

			if ( NotUsingConfig )
			{
				Console.WriteLine( "Using Database {0} and Connection String {1}", runnerContext.Database, ConnectionString );
			}
			else
			{
				Console.WriteLine( "Using Connection {0} from Configuration file {1}", runnerContext.Connection, ConfigFile );
			}

			if ( runnerContext.Timeout == 0 )
			{
				runnerContext.Timeout = 30;
			}

			var processorFactory = ProcessorFactory.GetFactory( runnerContext.Database );
			Processor = processorFactory.Create( ConnectionString, _announcer, new ProcessorOptions
			{
				PreviewOnly = runnerContext.PreviewOnly,
				Timeout = runnerContext.Timeout
			});
		}

		private bool NotUsingConfig
		{
			get { return string.IsNullOrEmpty( ConfigFile ); }
		}


		private void ReadConnectionString( IRunnerContext runnerContext, ConnectionStringSettings connection, string configurationFile )
		{
			if ( connection != null )
			{
				var factory = ProcessorFactory.Factories.Where( f => f.IsForProvider( connection.ProviderName ) ).FirstOrDefault();
				if ( factory != null )
				{
					runnerContext.Database = factory.Name;
					runnerContext.Connection = connection.Name;
					ConnectionString = connection.ConnectionString;
					ConfigFile = configurationFile;
				}
			}
			else
			{
				Console.WriteLine( "connection is null!" );
			}
		}

		public VersionLoader VersionLoader { get; set; }

		public void ApplyProfiles()
		{
			ProfileLoader.ApplyProfiles();
		}

		public void MigrateUp()
		{
			try
			{
				foreach (var version in MigrationLoader.Migrations.Keys)
				{
					MigrateUp(version);
				}

				ApplyProfiles();

				Processor.CommitTransaction();
				VersionLoader.LoadVersionInfo();
			}
			catch (Exception ex)
			{
				Processor.RollbackTransaction();
				throw;
			}
		}

		public void MigrateUp(long version)
		{
			if (!_alreadyOutputPreviewOnlyModeWarning && Processor.Options.PreviewOnly)
			{
				_announcer.Heading("PREVIEW-ONLY MODE");
				_alreadyOutputPreviewOnlyModeWarning = true;
			}

			ApplyMigrationUp(version);
			VersionLoader.LoadVersionInfo();
		}

		public void MigrateDown(long version)
		{
			try
			{
				ApplyMigrationDown(version);

				Processor.CommitTransaction();

				VersionLoader.LoadVersionInfo();
			}
			catch (Exception)
			{
				Processor.RollbackTransaction();
				throw;
			}
		}

		private void ApplyMigrationUp(long version)
		{
			if (!VersionLoader.VersionInfo.HasAppliedMigration(version))
			{
				Up(MigrationLoader.Migrations[version]);
				VersionLoader.UpdateVersionInfo(version);
			}
		}

		private void ApplyMigrationDown(long version)
		{
			try
			{
				Down(MigrationLoader.Migrations[version]);
				VersionLoader.DeleteVersion(version);
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

		public void Rollback(int steps)
		{
			foreach (var migrationNumber in VersionLoader.VersionInfo.AppliedMigrations().Take(steps))
			{
				ApplyMigrationDown(migrationNumber);
			}

			Processor.CommitTransaction();
			VersionLoader.LoadVersionInfo();
		}

		public void RollbackToVersion(long version)
		{
			// Get the migrations between current and the to version
			foreach (var migrationNumber in VersionLoader.VersionInfo.AppliedMigrations())
			{
				if (version < migrationNumber || version == 0)
				{
					ApplyMigrationDown(migrationNumber);
				}
			}

			if (version == 0)
				VersionLoader.RemoveVersionTable();

			Processor.CommitTransaction();

			VersionLoader.LoadVersionInfo();
		}

		public Assembly MigrationAssembly
		{
			get { return _migrationAssembly; }
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