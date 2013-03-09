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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Versioning;

namespace FluentMigrator.Runner
{
    public class MigrationRunner : IMigrationRunner
    {
        private Assembly _migrationAssembly;
        private IAnnouncer _announcer;
        private IStopWatch _stopWatch;
        private bool _alreadyOutputPreviewOnlyModeWarning;

        /// <summary>The arbitrary application context passed to the task runner.</summary>
        public object ApplicationContext { get; private set; }

        public bool SilentlyFail { get; set; }

        public IMigrationProcessor Processor { get; private set; }
        public IMigrationInformationLoader MigrationLoader { get; set; }
        public IProfileLoader ProfileLoader { get; set; }
        public IMigrationConventions Conventions { get; private set; }
        public IList<Exception> CaughtExceptions { get; private set; }

        public MigrationRunner(Assembly assembly, IRunnerContext runnerContext, IMigrationProcessor processor)
        {
            _migrationAssembly = assembly;
            _announcer = runnerContext.Announcer;
            Processor = processor;
            _stopWatch = runnerContext.StopWatch;
            ApplicationContext = runnerContext.ApplicationContext;

            SilentlyFail = false;
            CaughtExceptions = null;

            Conventions = new MigrationConventions();
            if (!string.IsNullOrEmpty(runnerContext.WorkingDirectory))
                Conventions.GetWorkingDirectory = () => runnerContext.WorkingDirectory;

            VersionLoader = new VersionLoader(this, _migrationAssembly, Conventions);
            MigrationLoader = new DefaultMigrationInformationLoader(Conventions, _migrationAssembly, runnerContext.Namespace, runnerContext.NestedNamespaces, runnerContext.Tags);
            ProfileLoader = new ProfileLoader(runnerContext, this, Conventions);
        }

        public IVersionLoader VersionLoader { get; set; }

        public void ApplyProfiles()
        {
            ProfileLoader.ApplyProfiles();
        }

        public void MigrateUp()
        {
            MigrateUp(true);
        }

        public void MigrateUp(bool useAutomaticTransactionManagement)
        {
            var migrations = MigrationLoader.LoadMigrations();

            foreach (var pair in migrations)
            {
                ApplyMigrationUp(pair.Value, useAutomaticTransactionManagement && pair.Value.TransactionBehavior == TransactionBehavior.Default);
            }

            ApplyProfiles();

            VersionLoader.LoadVersionInfo();
        }

        public void MigrateUp(long targetVersion)
        {
            MigrateUp(targetVersion, true);
        }

        public void MigrateUp(long targetVersion, bool useAutomaticTransactionManagement)
        {
            var migrationInfos = GetUpMigrationsToApply(targetVersion);

            foreach (var migrationInfo in migrationInfos)
            {
                ApplyMigrationUp(migrationInfo, useAutomaticTransactionManagement && migrationInfo.TransactionBehavior == TransactionBehavior.Default);
            }

            VersionLoader.LoadVersionInfo();
        }

        private IEnumerable<IMigrationInfo> GetUpMigrationsToApply(long version)
        {
            var migrations = MigrationLoader.LoadMigrations();

            return from pair in migrations 
                   where IsMigrationStepNeededForUpMigration(pair.Key, version) 
                   select pair.Value;
        }

        private bool IsMigrationStepNeededForUpMigration(long versionOfMigration, long targetVersion)
        {
            if (versionOfMigration <= targetVersion && !VersionLoader.VersionInfo.HasAppliedMigration(versionOfMigration))
            {
                return true;
            }
            return false;

        }

        public void MigrateDown(long targetVersion)
        {
            MigrateDown(targetVersion, true);
        }

        public void MigrateDown(long targetVersion, bool useAutomaticTransactionManagement)
        {
            var migrationInfos = GetDownMigrationsToApply(targetVersion);

            foreach (var migrationInfo in migrationInfos)
            {
                ApplyMigrationDown(migrationInfo, useAutomaticTransactionManagement && migrationInfo.TransactionBehavior == TransactionBehavior.Default);
            }

            VersionLoader.LoadVersionInfo();
        }

        private IEnumerable<IMigrationInfo> GetDownMigrationsToApply(long targetVersion)
        {
            var migrations = MigrationLoader.LoadMigrations();

            var migrationsToApply = (from pair in migrations 
                                     where IsMigrationStepNeededForDownMigration(pair.Key, targetVersion) 
                                     select pair.Value)
                                     .ToList();

            return migrationsToApply.OrderByDescending(x => x.Version);
        }


        private bool IsMigrationStepNeededForDownMigration(long versionOfMigration, long targetVersion)
        {
            if (versionOfMigration > targetVersion && VersionLoader.VersionInfo.HasAppliedMigration(versionOfMigration))
            {
                return true;
            }
            return false;

        }

        private void ApplyMigrationUp(IMigrationInfo migrationInfo, bool useTransaction)
        {
            if (!_alreadyOutputPreviewOnlyModeWarning && Processor.Options.PreviewOnly)
            {
                _announcer.Heading("PREVIEW-ONLY MODE");
                _alreadyOutputPreviewOnlyModeWarning = true;
            }

            if (!VersionLoader.VersionInfo.HasAppliedMigration(migrationInfo.Version))
            {
                var name = GetMigrationName(migrationInfo);
                _announcer.Heading(string.Format("{0} migrating", name));

                try
                {
                    _stopWatch.Start();

                    if (useTransaction) Processor.BeginTransaction();

                    ExecuteMigration(migrationInfo.Migration, (m, c) => m.GetUpExpressions(c));
                    VersionLoader.UpdateVersionInfo(migrationInfo.Version);

                    if (useTransaction) Processor.CommitTransaction();

                    _stopWatch.Stop();

                    _announcer.Say(string.Format("{0} migrated", name));
                    _announcer.ElapsedTime(_stopWatch.ElapsedTime());
                }
                catch (Exception)
                {
                    if (useTransaction) Processor.RollbackTransaction();
                    throw;
                }
            }
        }

        private void ApplyMigrationDown(IMigrationInfo migrationInfo, bool useTransaction)
        {
            var name = GetMigrationName(migrationInfo);
            _announcer.Heading(string.Format("{0} reverting", name));

            try
            {
                _stopWatch.Start();

                if (useTransaction) Processor.BeginTransaction();
                
                ExecuteMigration(migrationInfo.Migration, (m, c) => m.GetDownExpressions(c));
                VersionLoader.DeleteVersion(migrationInfo.Version);

                if (useTransaction) Processor.CommitTransaction();

                _stopWatch.Stop();

                _announcer.Say(string.Format("{0} reverted", name));
                _announcer.ElapsedTime(_stopWatch.ElapsedTime());
            }
            catch (Exception)
            {
                if (useTransaction) Processor.RollbackTransaction();
                throw;
            }
        }

        public void Rollback(int steps)
        {
            Rollback(steps, true);
        }

        public void Rollback(int steps, bool useAutomaticTransactionManagement)
        {
            var availableMigrations = MigrationLoader.LoadMigrations();
            var migrationsToRollback = new List<IMigrationInfo>();

            foreach (long version in VersionLoader.VersionInfo.AppliedMigrations())
            {
                IMigrationInfo migrationInfo;
                if (availableMigrations.TryGetValue(version, out migrationInfo)) migrationsToRollback.Add(migrationInfo);
            }

            foreach (IMigrationInfo migrationInfo in migrationsToRollback.Take(steps))
            {
                ApplyMigrationDown(migrationInfo, useAutomaticTransactionManagement && migrationInfo.TransactionBehavior == TransactionBehavior.Default);
            }

            VersionLoader.LoadVersionInfo();

            if (!VersionLoader.VersionInfo.AppliedMigrations().Any())
                VersionLoader.RemoveVersionTable();
        }

        public void RollbackToVersion(long version)
        {
            RollbackToVersion(version, true);
        }

        public void RollbackToVersion(long version, bool useAutomaticTransactionManagement)
        {
            var availableMigrations = MigrationLoader.LoadMigrations();
            var migrationsToRollback = new List<IMigrationInfo>();

            foreach (long appliedVersion in VersionLoader.VersionInfo.AppliedMigrations())
            {
                IMigrationInfo migrationInfo;
                if (availableMigrations.TryGetValue(appliedVersion, out migrationInfo)) migrationsToRollback.Add(migrationInfo);
            }

            foreach (IMigrationInfo migrationInfo in migrationsToRollback)
            {
                if (version >= migrationInfo.Version) continue;

                ApplyMigrationDown(migrationInfo, useAutomaticTransactionManagement && migrationInfo.TransactionBehavior == TransactionBehavior.Default);
            }

            VersionLoader.LoadVersionInfo();

            if (version == 0 && !VersionLoader.VersionInfo.AppliedMigrations().Any())
                VersionLoader.RemoveVersionTable();
        }

        public Assembly MigrationAssembly
        {
            get { return _migrationAssembly; }
        }

        private string GetMigrationName(IMigration migration)
        {
            if (migration == null) throw new ArgumentNullException("migration");

            return string.Format("{0}", migration.GetType().Name);
        }

        private string GetMigrationName(IMigrationInfo migration)
        {
            if (migration == null) throw new ArgumentNullException("migration");

            return string.Format("{0}: {1}", migration.Version, migration.Migration.GetType().Name);
        }

        public void Up(IMigration migration)
        {
            var name = GetMigrationName(migration);
            _announcer.Heading(string.Format("{0} migrating", name));

            try
            {
                _stopWatch.Start();

                Processor.BeginTransaction();
                ExecuteMigration(migration, (m, c) => m.GetUpExpressions(c));
                Processor.CommitTransaction();

                _stopWatch.Stop();

                _announcer.Say(string.Format("{0} migrated", name));
                _announcer.ElapsedTime(_stopWatch.ElapsedTime());
            }
            catch (Exception)
            {
                Processor.RollbackTransaction();
                throw;
            }
        }

        private void ExecuteMigration(IMigration migration, Action<IMigration, IMigrationContext> getExpressions)
        {
            CaughtExceptions = new List<Exception>();
            var context = new MigrationContext(Conventions, Processor, MigrationAssembly, ApplicationContext);
            
            getExpressions(migration, context);

            ApplyConventionsToAndValidateExpressions(migration, context.Expressions);
            ExecuteExpressions(context.Expressions);
        }

        public void Down(IMigration migration)
        {
            var name = GetMigrationName(migration);
            _announcer.Heading(string.Format("{0} reverting", name));

            try
            {
                _stopWatch.Start();

                Processor.BeginTransaction();
                ExecuteMigration(migration, (m, c) => m.GetDownExpressions(c));
                Processor.CommitTransaction();

                _stopWatch.Stop();

                _announcer.Say(string.Format("{0} reverted", name));
                _announcer.ElapsedTime(_stopWatch.ElapsedTime());
            }
            catch (Exception)
            {
                Processor.RollbackTransaction();
                throw;
            }
        }

        /// <summary>
        /// Validates each migration expression that has implemented the ICanBeValidated interface.
        /// It throws an InvalidMigrationException exception if validation fails.
        /// </summary>
        /// <param name="migration">The current migration being run</param>
        /// <param name="expressions">All the expressions contained in the up or down action</param>
        protected void ApplyConventionsToAndValidateExpressions(IMigration migration, IEnumerable<IMigrationExpression> expressions)
        {
            var invalidExpressions = new Dictionary<string, string>();
            foreach (var expression in expressions)
            {
                expression.ApplyConventions(Conventions);

                var errors = new Collection<string>();
                expression.CollectValidationErrors(errors);

                if(errors.Count > 0)
                    invalidExpressions.Add(expression.GetType().Name, string.Join(" ", errors.ToArray()));
            }

            if (invalidExpressions.Count > 0)
            {
                var errorMessage = DictToString(invalidExpressions, "{0}: {1}");
                _announcer.Error("The migration {0} contained the following Validation Error(s): {1}", migration.GetType().Name, errorMessage);
                throw new InvalidMigrationException(migration, errorMessage);
            }
        }

        private string DictToString<TKey, TValue>(Dictionary<TKey, TValue> items, string format)
        {
            format = String.IsNullOrEmpty(format) ? "{0}='{1}' " : format;
            return items.Aggregate(new StringBuilder(), (sb, kvp) => sb.AppendFormat(format, kvp.Key, kvp.Value).AppendLine()).ToString();
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

        public void ValidateVersionOrder()
        {
            var unappliedVersions = MigrationLoader.LoadMigrations().Where(kvp => MigrationVersionLessThanGreatestAppliedMigration(kvp.Key)).ToList();
            if (unappliedVersions.Any())
                throw new VersionOrderInvalidException(unappliedVersions);

            _announcer.Say("Version ordering valid.");
        }

        public void ListMigrations()
        {
            IVersionInfo currentVersionInfo = this.VersionLoader.VersionInfo;
            long currentVersion = currentVersionInfo.Latest();

            _announcer.Heading("Migrations");

            foreach(KeyValuePair<long, IMigrationInfo> migration in MigrationLoader.LoadMigrations())
            {
                string migrationName = GetMigrationName(migration.Value);
                bool isCurrent = migration.Key == currentVersion;
                string message = string.Format("{0}{1}",
                                                migrationName,
                                                isCurrent ? " (current)" : string.Empty);

                if(isCurrent)
                    _announcer.Emphasize(message);
                else
                    _announcer.Say(message);
            }
        }

        private bool MigrationVersionLessThanGreatestAppliedMigration(long version)
        {
            return !VersionLoader.VersionInfo.HasAppliedMigration(version) && version < VersionLoader.VersionInfo.Latest();
        }
    }
}
