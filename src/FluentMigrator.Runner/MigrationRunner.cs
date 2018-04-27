#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using System.Linq;
using System.Reflection;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;

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
        public IMigrationRunnerConventions Conventions { get; private set; }
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

        public MigrationRunner(Assembly assembly, IRunnerContext runnerContext, IMigrationProcessor processor, IConventionSet convSet = null)
          : this(new SingleAssembly(assembly), runnerContext, processor, convSet: convSet)
        {
        }

        public MigrationRunner(
            IAssemblyCollection assemblies, IRunnerContext runnerContext,
            IMigrationProcessor processor, IVersionTableMetaData versionTableMetaData = null,
            IMigrationRunnerConventions migrationRunnerConventions = null, IConventionSet convSet = null)
        {
            _migrationAssemblies = assemblies;
            _announcer = runnerContext.Announcer;
            Processor = processor;
            _stopWatch = runnerContext.StopWatch;
            RunnerContext = runnerContext;

            SilentlyFail = false;
            CaughtExceptions = null;

            Conventions = migrationRunnerConventions ?? GetMigrationRunnerConventions(runnerContext);

            if (convSet == null)
                convSet = new DefaultConventionSet(runnerContext);

            _migrationScopeHandler = new MigrationScopeHandler(Processor);
            _migrationValidator = new MigrationValidator(_announcer, convSet);
            MigrationLoader = new DefaultMigrationInformationLoader(Conventions, _migrationAssemblies,
                                                                    runnerContext.Namespace,
                                                                    runnerContext.NestedNamespaces, runnerContext.Tags);
            ProfileLoader = new ProfileLoader(runnerContext, this, Conventions);
            MaintenanceLoader = new MaintenanceLoader(_migrationAssemblies, runnerContext.Tags, Conventions);

            if (runnerContext.NoConnection)
            {
                VersionLoader = new ConnectionlessVersionLoader(
                    this, _migrationAssemblies, convSet, Conventions,
                    runnerContext.StartVersion, runnerContext.Version, versionTableMetaData);
            }
            else
            {
                VersionLoader = new VersionLoader(this, _migrationAssemblies, convSet, Conventions, versionTableMetaData);
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

        private IMigrationRunnerConventions GetMigrationRunnerConventions(IRunnerContext runnerContext)
        {
            var matchedType = _migrationAssemblies
                .GetExportedTypes()
                .FirstOrDefault(t => typeof(IMigrationRunnerConventions).IsAssignableFrom(t));

            if (matchedType != null)
            {
                return (IMigrationRunnerConventions)Activator.CreateInstance(matchedType);
            }

            return new MigrationRunnerConventions();
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

        public bool HasMigrationsToApplyUp(long? version = null)
        {
            if (version.HasValue)
                return GetUpMigrationsToApply(version.Value).Any();

            return MigrationLoader.LoadMigrations().Any(mi => !VersionLoader.VersionInfo.HasAppliedMigration(mi.Key));
        }

        public bool HasMigrationsToApplyDown(long version)
        {
            return GetDownMigrationsToApply(version).Any();
        }

        public bool HasMigrationsToApplyRollback()
        {
            return VersionLoader.VersionInfo.AppliedMigrations().Any();
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
                        if (migrationInfo.IsAttributed() && migrationInfo.IsBreakingChange &&
                            !RunnerContext.PreviewOnly && !RunnerContext.AllowBreakingChange)
                        {
                            throw new InvalidOperationException(
                                string.Format(
                                    "The migration {0} is identified as a breaking change, and will not be executed unless the necessary flag (allow-breaking-changes|abc) is passed to the runner.",
                                    migrationInfo.GetName()));
                        }

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
            {
                VersionLoader.RemoveVersionTable();
            }
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
            var context = new MigrationContext(Processor, MigrationAssemblies, RunnerContext.ApplicationContext, Processor.ConnectionString);

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
            var currentVersionInfo = this.VersionLoader.VersionInfo;
            var currentVersion = currentVersionInfo.Latest();

            _announcer.Heading("Migrations");

            foreach (var migration in MigrationLoader.LoadMigrations())
            {
                var migrationName = migration.Value.GetName();
                var status = GetStatus(migration, currentVersion);
                var statusString = string.Join(", ", GetStatusStrings(status));
                var message = $"{migrationName}{(string.IsNullOrEmpty(statusString) ? string.Empty : $" ({statusString})")}";

                var isCurrent = (status & MigrationStatus.AppliedMask) == MigrationStatus.Current;
                var isBreaking = (status & MigrationStatus.Breaking) == MigrationStatus.Breaking;
                if (isCurrent || isBreaking)
                    _announcer.Emphasize(message);
                else
                    _announcer.Say(message);
            }
        }

        private IEnumerable<string> GetStatusStrings(MigrationStatus status)
        {
            switch (status & MigrationStatus.AppliedMask)
            {
                case MigrationStatus.Applied:
                    break;
                case MigrationStatus.Current:
                    yield return "current";
                    break;
                default:
                    yield return "not applied";
                    break;
            }

            if ((status & MigrationStatus.Breaking) == MigrationStatus.Breaking)
                yield return "BREAKING";
        }

        private MigrationStatus GetStatus(KeyValuePair<long, IMigrationInfo> migration, long currentVersion)
        {
            MigrationStatus status;

            if (migration.Key == currentVersion)
            {
                status = MigrationStatus.Current;
            }
            else if (VersionLoader.VersionInfo.HasAppliedMigration(migration.Value.Version))
            {
                status = MigrationStatus.Applied;
            }
            else
            {
                status = MigrationStatus.NotApplied;
            }

            if (migration.Value.IsBreakingChange)
            {
                status |= MigrationStatus.Breaking;
            }

            return status;
        }

        private bool MigrationVersionLessThanGreatestAppliedMigration(long version)
        {
            return !VersionLoader.VersionInfo.HasAppliedMigration(version) && version < VersionLoader.VersionInfo.Latest();
        }

        public IMigrationScope BeginScope()
        {
            return _migrationScopeHandler.BeginScope();
        }

        [Flags]
        private enum MigrationStatus
        {
            Applied = 0,
            Current = 1,
            NotApplied = 2,
            AppliedMask = 3,
            Breaking = 4,
        }
    }
}
