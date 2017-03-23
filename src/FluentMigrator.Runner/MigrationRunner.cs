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
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.Infrastructure.Extensions;

namespace FluentMigrator.Runner
{
    public class MigrationRunner : IMigrationRunner
    {
        private IAssemblyCollection _migrationAssemblies;
        private IAnnouncer _announcer;
        private IStopWatch _stopWatch;
        private bool _alreadyOutputPreviewOnlyModeWarning;
        private readonly MigrationValidator _migrationValidator;
        private readonly MigrationScopeHandler _migrationScopeHandler;

        public bool TransactionPerSession 
        {
            get { return RunnerContext.TransactionPerSession; }
        }

        public bool SilentlyFail { get; set; }

        public IMigrationProcessor Processor { get; private set; }
        public IMigrationInformationLoader MigrationLoader { get; set; }
        public IProfileLoader ProfileLoader { get; set; }
        public IMaintenanceLoader MaintenanceLoader { get; set; }
        public IMigrationConventions Conventions { get; private set; }
        public IList<Exception> CaughtExceptions { get; private set; }

        public IMigrationScope CurrentScope
        {
            get
            {
                return _migrationScopeHandler.CurrentScope;
            }
            set
            {
                _migrationScopeHandler.CurrentScope = value;
            }
        }

        public IRunnerContext RunnerContext { get; private set; }

        public MigrationRunner(Assembly assembly, IRunnerContext runnerContext, IMigrationProcessor processor)
          : this(new SingleAssembly(assembly), runnerContext, processor)
        {

        }

        public MigrationRunner(IAssemblyCollection assemblies, IRunnerContext runnerContext, IMigrationProcessor processor)
        {
            _migrationAssemblies = assemblies;
            _announcer = runnerContext.Announcer;
            Processor = processor;
            _stopWatch = runnerContext.StopWatch;
            RunnerContext = runnerContext;

            SilentlyFail = false;
            CaughtExceptions = null;

            Conventions = new MigrationConventions();
            if (!string.IsNullOrEmpty(runnerContext.WorkingDirectory))
                Conventions.GetWorkingDirectory = () => runnerContext.WorkingDirectory;

            _migrationScopeHandler = new MigrationScopeHandler(Processor);
            _migrationValidator = new MigrationValidator(_announcer, Conventions);
            MigrationLoader = new DefaultMigrationInformationLoader(Conventions, _migrationAssemblies, runnerContext.Namespace, runnerContext.NestedNamespaces, runnerContext.Tags);
            ProfileLoader = new ProfileLoader(runnerContext, this, Conventions);
            MaintenanceLoader = new MaintenanceLoader(_migrationAssemblies, runnerContext.Tags, Conventions);

            if (runnerContext.NoConnection){
                VersionLoader = new ConnectionlessVersionLoader(this, _migrationAssemblies, Conventions, runnerContext.StartVersion, runnerContext.Version);
            }
            else{
                VersionLoader = new VersionLoader(this, _migrationAssemblies, Conventions);
            }
        }

        public IVersionLoader VersionLoader { get; set; }

        public void ApplyProfiles()
        {
            ProfileLoader.ApplyProfiles();
        }

        public void ApplyMaintenance(MigrationStage stage, bool useAutomaticTransactionManagement)
        {
            var maintenanceMigrations = MaintenanceLoader.LoadMaintenance(stage);
            foreach (var maintenanceMigration in maintenanceMigrations)
            {
                ApplyMigrationUp(maintenanceMigration, useAutomaticTransactionManagement && maintenanceMigration.TransactionBehavior == TransactionBehavior.Default);
            }
        }

        public void MigrateUp()
        {
            MigrateUp(true);
        }

        public void MigrateUp(bool useAutomaticTransactionManagement)
        {
            MigrateUp(long.MaxValue, useAutomaticTransactionManagement);
        }

        public void MigrateUp(long targetVersion)
        {
            MigrateUp(targetVersion, true);
        }

        public void MigrateUp(long targetVersion, bool useAutomaticTransactionManagement)
        {
            var migrationInfos = GetUpMigrationsToApply(targetVersion);

            using (IMigrationScope scope = _migrationScopeHandler.CreateOrWrapMigrationScope(useAutomaticTransactionManagement && TransactionPerSession))
            {
                try
                {
                    ApplyMaintenance(MigrationStage.BeforeAll, useAutomaticTransactionManagement);

                    foreach (var migrationInfo in migrationInfos)
                    {
                        ApplyMaintenance(MigrationStage.BeforeEach, useAutomaticTransactionManagement);
                        ApplyMigrationUp(migrationInfo, useAutomaticTransactionManagement && migrationInfo.TransactionBehavior == TransactionBehavior.Default);
                        ApplyMaintenance(MigrationStage.AfterEach, useAutomaticTransactionManagement);
                    }

                    ApplyMaintenance(MigrationStage.BeforeProfiles, useAutomaticTransactionManagement);

                    ApplyProfiles();

                    ApplyMaintenance(MigrationStage.AfterAll, useAutomaticTransactionManagement);

                    scope.Complete();
                }
                catch
                {
                    if (scope.IsActive)
                        scope.Cancel();  // SQLAnywhere needs explicit call to rollback transaction

                    throw;
                }
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

            using (IMigrationScope scope = _migrationScopeHandler.CreateOrWrapMigrationScope(useAutomaticTransactionManagement && TransactionPerSession))
            {
                try
                {
                    foreach (var migrationInfo in migrationInfos)
                    {
                        ApplyMigrationDown(migrationInfo, useAutomaticTransactionManagement && migrationInfo.TransactionBehavior == TransactionBehavior.Default);
                    }

                    ApplyProfiles();

                    scope.Complete();
                }
                catch
                {
                    if (scope.IsActive)
                        scope.Cancel();  // SQLAnywhere needs explicit call to rollback transaction

                    throw;
                }
            }

            VersionLoader.LoadVersionInfo();
        }

        private IEnumerable<IMigrationInfo> GetDownMigrationsToApply(long targetVersion)
        {
            var migrations = MigrationLoader.LoadMigrations();

            var migrationsToApply = (from pair in migrations 
                                     where IsMigrationStepNeededForDownMigration(pair.Key, targetVersion) 
                                     select pair.Value);

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

        public virtual void ApplyMigrationUp(IMigrationInfo migrationInfo, bool useTransaction)
        {
            if (migrationInfo == null) throw new ArgumentNullException("migrationInfo");

            if (!_alreadyOutputPreviewOnlyModeWarning && Processor.Options.PreviewOnly)
            {
                _announcer.Heading("PREVIEW-ONLY MODE");
                _alreadyOutputPreviewOnlyModeWarning = true;
            }

            if (!migrationInfo.IsAttributed() || !VersionLoader.VersionInfo.HasAppliedMigration(migrationInfo.Version))
            {
                var name = migrationInfo.GetName();
                _announcer.Heading(string.Format("{0} migrating", name));

                _stopWatch.Start();

                using (IMigrationScope scope = _migrationScopeHandler.CreateOrWrapMigrationScope(useTransaction))
                {
                    try
                    {
                        ExecuteMigration(migrationInfo.Migration, (m, c) => m.GetUpExpressions(c));

                        if (migrationInfo.IsAttributed())
                        {
                            VersionLoader.UpdateVersionInfo(migrationInfo.Version, migrationInfo.Description ?? migrationInfo.Migration.GetType().Name);
                        }

                        scope.Complete();
                    }
                    catch
                    {
                        if (useTransaction && scope.IsActive)
                            scope.Cancel();  // SQLAnywhere needs explicit call to rollback transaction

                        throw;
                    }

                    _stopWatch.Stop();

                    _announcer.Say(string.Format("{0} migrated", name));
                    _announcer.ElapsedTime(_stopWatch.ElapsedTime());
                }
            }
        }

        public virtual void ApplyMigrationDown(IMigrationInfo migrationInfo, bool useTransaction)
        {
            if (migrationInfo == null) throw new ArgumentNullException("migrationInfo");

            var name = migrationInfo.GetName();
            _announcer.Heading(string.Format("{0} reverting", name));

            _stopWatch.Start();

            using (IMigrationScope scope = _migrationScopeHandler.CreateOrWrapMigrationScope(useTransaction))
            {
                try
                {
                    ExecuteMigration(migrationInfo.Migration, (m, c) => m.GetDownExpressions(c));
                    if (migrationInfo.IsAttributed()) VersionLoader.DeleteVersion(migrationInfo.Version);

                    scope.Complete();
                }
                catch
                {
                    if (useTransaction && scope.IsActive)
                        scope.Cancel();  // SQLAnywhere needs explicit call to rollback transaction

                    throw;
                }

                _stopWatch.Stop();

                _announcer.Say(string.Format("{0} reverted", name));
                _announcer.ElapsedTime(_stopWatch.ElapsedTime());
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

            using (IMigrationScope scope = _migrationScopeHandler.CreateOrWrapMigrationScope(useAutomaticTransactionManagement && TransactionPerSession))
            {
                try
                {
                    foreach (IMigrationInfo migrationInfo in migrationsToRollback.Take(steps))
                    {
                        ApplyMigrationDown(migrationInfo, useAutomaticTransactionManagement && migrationInfo.TransactionBehavior == TransactionBehavior.Default);
                    }

                    scope.Complete();
                }
                catch
                {
                    if (scope.IsActive)
                        scope.Cancel();  // SQLAnywhere needs explicit call to rollback transaction

                    throw;
                }
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

            using (IMigrationScope scope = _migrationScopeHandler.CreateOrWrapMigrationScope(useAutomaticTransactionManagement && TransactionPerSession))
            {
                try
                {
                    foreach (IMigrationInfo migrationInfo in migrationsToRollback)
                    {
                        if (version >= migrationInfo.Version) continue;

                        ApplyMigrationDown(migrationInfo, useAutomaticTransactionManagement && migrationInfo.TransactionBehavior == TransactionBehavior.Default);
                    }

                    scope.Complete();
                }
                catch
                {
                    if (scope.IsActive)
                        scope.Cancel();  // SQLAnywhere needs explicit call to rollback transaction

                    throw;
                }
            }

            VersionLoader.LoadVersionInfo();

            if (version == 0 && !VersionLoader.VersionInfo.AppliedMigrations().Any())
                VersionLoader.RemoveVersionTable();
        }

        public IAssemblyCollection MigrationAssemblies
        {
            get { return _migrationAssemblies; }
        }

        public void Up(IMigration migration)
        {
            var migrationInfoAdapter = new NonAttributedMigrationToMigrationInfoAdapter(migration);

            ApplyMigrationUp(migrationInfoAdapter, true);
        }

        private void ExecuteMigration(IMigration migration, Action<IMigration, IMigrationContext> getExpressions)
        {
            CaughtExceptions = new List<Exception>();
            var context = new MigrationContext(Conventions, Processor, MigrationAssemblies, RunnerContext.ApplicationContext, Processor.ConnectionString);
            
            getExpressions(migration, context);

            _migrationValidator.ApplyConventionsToAndValidateExpressions(migration, context.Expressions);
            ExecuteExpressions(context.Expressions);
        }

        public void Down(IMigration migration)
        {
            var migrationInfoAdapter = new NonAttributedMigrationToMigrationInfoAdapter(migration);

            ApplyMigrationDown(migrationInfoAdapter, true);
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
                        insertTicks += _stopWatch.Time(() => expression.ExecuteWith(Processor)).Ticks;
                        insertCount++;
                    }
                    else
                    {
                        AnnounceTime(expression.ToString(), () => expression.ExecuteWith(Processor));
                    }
                }
                catch (Exception er)
                {
                    _announcer.Error(er);

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
            _announcer.ElapsedTime(_stopWatch.Time(action));
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
                string migrationName = migration.Value.GetName();
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

        public IMigrationScope BeginScope()
        {
            return _migrationScopeHandler.BeginScope();
        }
    }
}
