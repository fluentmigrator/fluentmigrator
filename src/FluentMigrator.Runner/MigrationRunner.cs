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

namespace FluentMigrator.Runner
{
	public class MigrationRunner : IMigrationRunner
	{
		private Assembly _migrationAssembly;
		private IAnnouncer _announcer;
		private IStopWatch _stopWatch;
		private bool _alreadyOutputPreviewOnlyModeWarning;
		public bool SilentlyFail { get; set; }

		public IMigrationProcessor Processor { get; private set; }
		public IMigrationLoader MigrationLoader { get; set; }
		public IProfileLoader ProfileLoader { get; set; }
		public IMigrationConventions Conventions { get; private set; }
		public IList<Exception> CaughtExceptions { get; private set; }

		public MigrationRunner(Assembly assembly, IRunnerContext runnerContext, IMigrationProcessor processor)
		{
			_migrationAssembly = assembly;
			_announcer = runnerContext.Announcer;
			Processor = processor;
			_stopWatch = runnerContext.StopWatch;

			SilentlyFail = false;
			CaughtExceptions = null;

			Conventions = new MigrationConventions();

			ProcessWorkingDirectory(runnerContext);

			ProcessAutoReverse(runnerContext);

			VersionLoader = new VersionLoader(this, _migrationAssembly, Conventions);
			MigrationLoader = new MigrationLoader(Conventions, _migrationAssembly, runnerContext.Namespace);
			ProfileLoader = new ProfileLoader(runnerContext, this, Conventions);
		}

		private void ProcessWorkingDirectory(IRunnerContext runnerContext)
		{
			if (!string.IsNullOrEmpty(runnerContext.WorkingDirectory))
				Conventions.GetWorkingDirectory = () => runnerContext.WorkingDirectory;
		}

		private void ProcessAutoReverse(IRunnerContext runnerContext)
		{
			Conventions.GetAutoReverse = () => System.Convert.ToBoolean(runnerContext.AutoReverse);
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
			catch (Exception)
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

			VersionLoader.LoadVersionInfo();
			Processor.CommitTransaction();
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

			VersionLoader.LoadVersionInfo();
			Processor.CommitTransaction();
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