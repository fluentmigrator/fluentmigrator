#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Processors;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FluentMigrator.Runner.Constraints;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// The default <see cref="IMigrationRunner"/> implementation
    /// </summary>
    public class MigrationRunner : IMigrationRunner
    {
        [NotNull]
        private readonly ILogger _logger;

        [NotNull]
        private readonly IStopWatch _stopWatch;

        [CanBeNull]
        private readonly IServiceProvider _serviceProvider;

        [NotNull]
        private readonly Lazy<IVersionLoader> _versionLoader;

        [CanBeNull]
        private readonly RunnerOptions _options;

        [NotNull]
        private readonly ProcessorOptions _processorOptions;
        private readonly MigrationValidator _migrationValidator;
        private readonly IMigrationScopeManager _migrationScopeManager;

        private IVersionLoader _currentVersionLoader;

        private bool _alreadyOutputPreviewOnlyModeWarning;

        private List<Exception> _caughtExceptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationRunner"/> class.
        /// </summary>
        /// <param name="options">The migration runner options</param>
        /// <param name="processorOptions">The migration processor options</param>
        /// <param name="profileLoader">The profile loader</param>
        /// <param name="processorAccessor">The migration processor accessor</param>
        /// <param name="maintenanceLoader">The maintenance loader</param>
        /// <param name="migrationLoader">The migration loader</param>
        /// <param name="logger">The logger</param>
        /// <param name="stopWatch">The stopwatch</param>
        /// <param name="migrationRunnerConventionsAccessor">The accessor for migration runner conventions</param>
        /// <param name="assemblySource">The assemblies to scan for migrations, etc...</param>
        /// <param name="migrationValidator">The validator for migrations</param>
        /// <param name="serviceProvider">The service provider</param>
        [Obsolete]
        public MigrationRunner(
            [NotNull] IOptions<RunnerOptions> options,
            [NotNull] IOptionsSnapshot<ProcessorOptions> processorOptions,
            [NotNull] IProfileLoader profileLoader,
            [NotNull] IProcessorAccessor processorAccessor,
            [NotNull] IMaintenanceLoader maintenanceLoader,
            [NotNull] IMigrationInformationLoader migrationLoader,
            [NotNull] ILogger<MigrationRunner> logger,
            [NotNull] IStopWatch stopWatch,
            [NotNull] IMigrationRunnerConventionsAccessor migrationRunnerConventionsAccessor,
            [NotNull] IAssemblySource assemblySource,
            [NotNull] MigrationValidator migrationValidator,
            [NotNull] IServiceProvider serviceProvider)
            : this(
                options,
                processorOptions,
                profileLoader,
                processorAccessor,
                maintenanceLoader,
                migrationLoader,
                logger,
                stopWatch,
                migrationRunnerConventionsAccessor,
                assemblySource,
                migrationValidator,
                serviceProvider,
                migrationScopeHandler: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationRunner"/> class.
        /// </summary>
        /// <param name="options">The migration runner options</param>
        /// <param name="processorOptions">The migration processor options</param>
        /// <param name="profileLoader">The profile loader</param>
        /// <param name="processorAccessor">The migration processor accessor</param>
        /// <param name="maintenanceLoader">The maintenance loader</param>
        /// <param name="migrationLoader">The migration loader</param>
        /// <param name="logger">The logger</param>
        /// <param name="stopWatch">The stopwatch</param>
        /// <param name="migrationRunnerConventionsAccessor">The accessor for migration runner conventions</param>
        /// <param name="migrationValidator">The validator for migrations</param>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="migrationScopeHandler">THe migration scope handler</param>
        public MigrationRunner(
            [NotNull] IOptions<RunnerOptions> options,
            [NotNull] IOptionsSnapshot<ProcessorOptions> processorOptions,
            [NotNull] IProfileLoader profileLoader,
            [NotNull] IProcessorAccessor processorAccessor,
            [NotNull] IMaintenanceLoader maintenanceLoader,
            [NotNull] IMigrationInformationLoader migrationLoader,
            [NotNull] ILogger<MigrationRunner> logger,
            [NotNull] IStopWatch stopWatch,
            [NotNull] IMigrationRunnerConventionsAccessor migrationRunnerConventionsAccessor,
            [NotNull] MigrationValidator migrationValidator,
            [NotNull] IServiceProvider serviceProvider,
            [CanBeNull] IMigrationScopeManager migrationScopeHandler)
        {
            Processor = processorAccessor.Processor;
            Conventions = migrationRunnerConventionsAccessor.MigrationRunnerConventions;
            ProfileLoader = profileLoader;
            MaintenanceLoader = maintenanceLoader;
            MigrationLoader = migrationLoader;

            _serviceProvider = serviceProvider;
            _options = options.Value;
            _logger = logger;
            _stopWatch = stopWatch;
            _processorOptions = processorOptions.Value;

            _migrationScopeManager = migrationScopeHandler ?? new MigrationScopeHandler(Processor, processorOptions.Value);
            _migrationValidator = migrationValidator;
            _versionLoader = new Lazy<IVersionLoader>(serviceProvider.GetRequiredService<IVersionLoader>);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationRunner"/> class.
        /// </summary>
        /// <param name="options">The migration runner options</param>
        /// <param name="processorOptions">The migration processor options</param>
        /// <param name="profileLoader">The profile loader</param>
        /// <param name="processorAccessor">The migration processor accessor</param>
        /// <param name="maintenanceLoader">The maintenance loader</param>
        /// <param name="migrationLoader">The migration loader</param>
        /// <param name="logger">The logger</param>
        /// <param name="stopWatch">The stopwatch</param>
        /// <param name="migrationRunnerConventionsAccessor">The accessor for migration runner conventions</param>
        /// <param name="assemblySource">The assemblies to scan for migrations, etc...</param>
        /// <param name="migrationValidator">The validator for migrations</param>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="migrationScopeHandler">The migration scope handler</param>
        [Obsolete]
        public MigrationRunner(
            [NotNull] IOptions<RunnerOptions> options,
            [NotNull] IOptionsSnapshot<ProcessorOptions> processorOptions,
            [NotNull] IProfileLoader profileLoader,
            [NotNull] IProcessorAccessor processorAccessor,
            [NotNull] IMaintenanceLoader maintenanceLoader,
            [NotNull] IMigrationInformationLoader migrationLoader,
            [NotNull] ILogger<MigrationRunner> logger,
            [NotNull] IStopWatch stopWatch,
            [NotNull] IMigrationRunnerConventionsAccessor migrationRunnerConventionsAccessor,
            // ReSharper disable once UnusedParameter.Local
            [NotNull] IAssemblySource assemblySource,
            [NotNull] MigrationValidator migrationValidator,
            [NotNull] IServiceProvider serviceProvider,
            [CanBeNull] IMigrationScopeManager migrationScopeHandler)
        {
            Processor = processorAccessor.Processor;
            Conventions = migrationRunnerConventionsAccessor.MigrationRunnerConventions;
            ProfileLoader = profileLoader;
            MaintenanceLoader = maintenanceLoader;
            MigrationLoader = migrationLoader;

            _serviceProvider = serviceProvider;
            _options = options.Value;
            _logger = logger;
            _stopWatch = stopWatch;
            _processorOptions = processorOptions.Value;

            _migrationScopeManager = migrationScopeHandler ?? new MigrationScopeHandler(Processor, processorOptions.Value);
            _migrationValidator = migrationValidator;
            _versionLoader = new Lazy<IVersionLoader>(serviceProvider.GetRequiredService<IVersionLoader>);
        }

#pragma warning disable 612
        /// <summary>
        /// Gets a value indicating whether a single transaction for the whole session should be used
        /// </summary>
        public bool TransactionPerSession => _options?.TransactionPerSession ?? false;
#pragma warning restore 612

        /// <summary>
        /// Gets a value indicating whether exceptions should be caught
        /// </summary>
        public bool SilentlyFail { get; set; }

        /// <summary>
        /// Gets the caught exceptions when <see cref="SilentlyFail"/> is <c>true</c>
        /// </summary>
        public IReadOnlyList<Exception> CaughtExceptions => _caughtExceptions;

        /// <inheritdoc />
        public IMigrationProcessor Processor { get; }

        /// <inheritdoc />
        public IMigrationInformationLoader MigrationLoader { get; set; }

        /// <summary>
        /// Gets or sets the profile loader
        /// </summary>
        public IProfileLoader ProfileLoader { get; set; }

        /// <summary>
        /// Gets the maintenance loader
        /// </summary>
        public IMaintenanceLoader MaintenanceLoader { get; }

        /// <summary>
        /// Gets the migration runner conventions
        /// </summary>
        public IMigrationRunnerConventions Conventions { get; }

        /// <summary>
        /// Gets or sets the currently active migration scope.
        /// Setter for <see cref="IMigrationScopeManager"/> was removed. Setter for this property will throw exception when custom migration scope handler is used
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when custom <see cref="IMigrationScopeManager"/> implementation is used</exception>
        public IMigrationScope CurrentScope
        {
            get => _migrationScopeManager.CurrentScope;
            set
            {
                if (_migrationScopeManager is MigrationScopeHandler msh)
                {
                    msh.CurrentScope = value;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        /// <inheritdoc />
        [Obsolete]
        public IRunnerContext RunnerContext { get; }

        /// <summary>
        /// Gets or sets the version loader
        /// </summary>
        public IVersionLoader VersionLoader
        {
            get => _currentVersionLoader ?? _versionLoader.Value;
            set => _currentVersionLoader = value;
        }

        private bool AllowBreakingChanges =>
#pragma warning disable 612
            _options?.AllowBreakingChange ?? false;
#pragma warning restore 612

        /// <summary>
        /// Apply all matching profiles
        /// </summary>
        public void ApplyProfiles()
        {

            ProfileLoader.ApplyProfiles(this);
        }

        /// <summary>
        /// Apply maintenance changes
        /// </summary>
        /// <param name="stage">The maintenance stage</param>
        /// <param name="useAutomaticTransactionManagement"><c>true</c> if automatic transaction management should be used</param>
        public void ApplyMaintenance(MigrationStage stage, bool useAutomaticTransactionManagement)
        {
            var maintenanceMigrations = MaintenanceLoader.LoadMaintenance(stage);
            foreach (var maintenanceMigration in maintenanceMigrations)
            {
                ApplyMigrationUp(maintenanceMigration, useAutomaticTransactionManagement && maintenanceMigration.TransactionBehavior == TransactionBehavior.Default);
            }
        }

        /// <inheritdoc />
        public void MigrateUp()
        {
            MigrateUp(true);
        }

        /// <summary>
        /// Apply migrations
        /// </summary>
        /// <param name="useAutomaticTransactionManagement"><c>true</c> if automatic transaction management should be used</param>
        public void MigrateUp(bool useAutomaticTransactionManagement)
        {
            MigrateUp(long.MaxValue, useAutomaticTransactionManagement);
        }

        /// <inheritdoc />
        public void MigrateUp(long targetVersion)
        {
            MigrateUp(targetVersion, true);
        }

        /// <summary>
        /// Apply migrations up to the given <paramref name="targetVersion"/>
        /// </summary>
        /// <param name="targetVersion">The target migration version</param>
        /// <param name="useAutomaticTransactionManagement"><c>true</c> if automatic transaction management should be used</param>
        public void MigrateUp(long targetVersion, bool useAutomaticTransactionManagement)
        {
            // Validate connection early to catch invalid connection strings before attempting migrations
            ValidateConnection();

            var migrationInfos = GetUpMigrationsToApply(targetVersion);

            using (IMigrationScope scope = _migrationScopeManager.CreateOrWrapMigrationScope(useAutomaticTransactionManagement && TransactionPerSession))
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
                    {
                        scope.Cancel(); // Some Database Providers needs explicit call to rollback transaction
                    }

                    throw;
                }
            }

            VersionLoader.LoadVersionInfo();
        }

        private IEnumerable<IMigrationInfo> GetUpMigrationsToApply(long version)
        {
            var migrations = MigrationLoader.LoadMigrations();

            return from pair in migrations
                   where IsMigrationStepNeededForUpMigration(pair.Value, version)
                   select pair.Value;
        }

        private bool IsMigrationStepNeededForUpMigration(IMigrationInfo migration, long targetVersion)
        {
            bool MeetsMigrationConstraints(Type migrationType)
            {
                return migrationType.GetCustomAttributes().OfType<MigrationConstraintAttribute>()
                    .All(a => a.ShouldRun(new MigrationConstraintContext
                {
                    RunnerOptions = _options,
                    VersionInfo = VersionLoader.VersionInfo
                }));
            }

            if (migration.Version <= targetVersion
                && !VersionLoader.VersionInfo.HasAppliedMigration(migration.Version)
                && MeetsMigrationConstraints(migration.Migration.GetType()))
            {
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void MigrateDown(long targetVersion)
        {
            MigrateDown(targetVersion, true);
        }

        /// <summary>
        /// Revert migrations down to the given <paramref name="targetVersion"/>
        /// </summary>
        /// <param name="targetVersion">The target version that should become the last applied migration version</param>
        /// <param name="useAutomaticTransactionManagement"><c>true</c> if automatic transaction management should be used</param>
        public void MigrateDown(long targetVersion, bool useAutomaticTransactionManagement)
        {
            var migrationInfos = GetDownMigrationsToApply(targetVersion);

            using (IMigrationScope scope = _migrationScopeManager.CreateOrWrapMigrationScope(useAutomaticTransactionManagement && TransactionPerSession))
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
                    {
                        scope.Cancel(); // Some Database Providers needs explicit call to rollback transaction
                    }

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

        /// <inheritdoc />
        public bool HasMigrationsToApplyUp(long? version = null)
        {
            if (version.HasValue)
            {
                return GetUpMigrationsToApply(version.Value).Any();
            }

            return MigrationLoader.LoadMigrations().Any(mi => !VersionLoader.VersionInfo.HasAppliedMigration(mi.Key));
        }

        /// <inheritdoc />
        public bool HasMigrationsToApplyDown(long version)
        {
            return GetDownMigrationsToApply(version).Any();
        }

        /// <inheritdoc />
        public bool HasMigrationsToApplyRollback()
        {
            return VersionLoader.VersionInfo.AppliedMigrations().Any();
        }

        /// <inheritdoc />
        public bool LoadVersionInfoIfRequired()
        {
            if (VersionLoader.AlreadyCreatedVersionTable && VersionLoader.AlreadyCreatedVersionSchema)
            {
                return false;
            }
            else
            {
                VersionLoader.LoadVersionInfo();
                return true;
            }
        }

        /// <summary>
        /// Apply the migration using the given migration information
        /// </summary>
        /// <param name="migrationInfo">The migration information</param>
        /// <param name="useTransaction"><c>true</c> when a transaction for this migration should be used</param>
        public virtual void ApplyMigrationUp([NotNull] IMigrationInfo migrationInfo, bool useTransaction)
        {
            if (migrationInfo == null)
            {
                throw new ArgumentNullException(nameof(migrationInfo));
            }

            if (!_alreadyOutputPreviewOnlyModeWarning && _processorOptions.PreviewOnly)
            {
                _logger.LogHeader("PREVIEW-ONLY MODE");
                _alreadyOutputPreviewOnlyModeWarning = true;
            }

            if (!migrationInfo.IsAttributed() || !VersionLoader.VersionInfo.HasAppliedMigration(migrationInfo.Version))
            {
                var name = migrationInfo.GetName();
                _logger.LogHeader($"{name} migrating");

                _stopWatch.Start();

                using (var scope = _migrationScopeManager.CreateOrWrapMigrationScope(useTransaction))
                {
                    try
                    {
                        if (migrationInfo.IsAttributed() && migrationInfo.IsBreakingChange &&
                            !_processorOptions.PreviewOnly && !AllowBreakingChanges)
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
                        {
                            scope.Cancel(); // Some Database Providers needs explicit call to rollback transaction
                        }

                        throw;
                    }

                    _stopWatch.Stop();

                    _logger.LogSay($"{name} migrated");
                    _logger.LogElapsedTime(_stopWatch.ElapsedTime());
                }
            }
        }

        /// <summary>
        /// Revert the migration using the given migration information
        /// </summary>
        /// <param name="migrationInfo">The migration information</param>
        /// <param name="useTransaction"><c>true</c> when a transaction for this operation should be used</param>
        public virtual void ApplyMigrationDown([NotNull] IMigrationInfo migrationInfo, bool useTransaction)
        {
            if (migrationInfo == null)
            {
                throw new ArgumentNullException(nameof(migrationInfo));
            }

            if (!_alreadyOutputPreviewOnlyModeWarning && _processorOptions.PreviewOnly)
            {
                _logger.LogHeader("PREVIEW-ONLY MODE");
                _alreadyOutputPreviewOnlyModeWarning = true;
            }

            var name = migrationInfo.GetName();
            _logger.LogHeader($"{name} reverting");

            _stopWatch.Start();

            using (var scope = _migrationScopeManager.CreateOrWrapMigrationScope(useTransaction))
            {
                try
                {
                    ExecuteMigration(migrationInfo.Migration, (m, c) => m.GetDownExpressions(c));
                    if (migrationInfo.IsAttributed())
                    {
                        VersionLoader.DeleteVersion(migrationInfo.Version);
                    }

                    scope.Complete();
                }
                catch
                {
                    if (useTransaction && scope.IsActive)
                    {
                        scope.Cancel(); // Some Database Providers needs explicit call to rollback transaction
                    }

                    throw;
                }

                _stopWatch.Stop();

                _logger.LogSay($"{name} reverted");
                _logger.LogElapsedTime(_stopWatch.ElapsedTime());
            }
        }

        /// <inheritdoc />
        public void Rollback(int steps)
        {
            Rollback(steps, true);
        }

        /// <summary>
        /// Rollback the last <paramref name="steps"/>
        /// </summary>
        /// <param name="steps">The number of migrations to rollback</param>
        /// <param name="useAutomaticTransactionManagement"><c>true</c> if automatic transaction management should be used</param>
        public void Rollback(int steps, bool useAutomaticTransactionManagement)
        {
            var availableMigrations = MigrationLoader.LoadMigrations();
            var migrationsToRollback = new List<IMigrationInfo>();

            foreach (var version in VersionLoader.VersionInfo.AppliedMigrations())
            {
                if (availableMigrations.TryGetValue(version, out var migrationInfo))
                {
                    migrationsToRollback.Add(migrationInfo);
                }
            }

            using (var scope = _migrationScopeManager.CreateOrWrapMigrationScope(useAutomaticTransactionManagement && TransactionPerSession))
            {
                try
                {
                    foreach (var migrationInfo in migrationsToRollback.Take(steps))
                    {
                        ApplyMigrationDown(migrationInfo, useAutomaticTransactionManagement && migrationInfo.TransactionBehavior == TransactionBehavior.Default);
                    }

                    scope.Complete();
                }
                catch
                {
                    if (scope.IsActive)
                    {
                        scope.Cancel(); // Some Database Providers needs explicit call to rollback transaction
                    }

                    throw;
                }
            }

            VersionLoader.LoadVersionInfo();

            if (!VersionLoader.VersionInfo.AppliedMigrations().Any())
            {
                VersionLoader.RemoveVersionTable();
            }
        }

        /// <inheritdoc />
        public void RollbackToVersion(long version)
        {
            RollbackToVersion(version, true);
        }

        /// <summary>
        /// Rollback to a given <paramref name="version"/>
        /// </summary>
        /// <param name="version">The version to rollback to (exclusive)</param>
        /// <param name="useAutomaticTransactionManagement"><c>true</c> if automatic transaction management should be used</param>
        public void RollbackToVersion(long version, bool useAutomaticTransactionManagement)
        {
            var availableMigrations = MigrationLoader.LoadMigrations();
            var migrationsToRollback = new List<IMigrationInfo>();

            foreach (var appliedVersion in VersionLoader.VersionInfo.AppliedMigrations())
            {
                if (availableMigrations.TryGetValue(appliedVersion, out var migrationInfo))
                {
                    migrationsToRollback.Add(migrationInfo);
                }
            }

            using (IMigrationScope scope = _migrationScopeManager.CreateOrWrapMigrationScope(useAutomaticTransactionManagement && TransactionPerSession))
            {
                try
                {
                    foreach (IMigrationInfo migrationInfo in migrationsToRollback)
                    {
                        if (version >= migrationInfo.Version)
                        {
                            continue;
                        }

                        ApplyMigrationDown(migrationInfo, useAutomaticTransactionManagement && migrationInfo.TransactionBehavior == TransactionBehavior.Default);
                    }

                    scope.Complete();
                }
                catch
                {
                    if (scope.IsActive)
                    {
                        scope.Cancel(); // Some Database Providers needs explicit call to rollback transaction
                    }

                    throw;
                }
            }

            VersionLoader.LoadVersionInfo();

            if (version == 0 && !VersionLoader.VersionInfo.AppliedMigrations().Any())
            {
                VersionLoader.RemoveVersionTable();
            }
        }

        /// <inheritdoc />
        public void Up(IMigration migration)
        {
            var migrationInfoAdapter = new NonAttributedMigrationToMigrationInfoAdapter(migration);

            ApplyMigrationUp(migrationInfoAdapter, true);
        }

        private void ExecuteMigration(IMigration migration, Action<IMigration, IMigrationContext> getExpressions)
        {
            _caughtExceptions = new List<Exception>();

            MigrationContext context;

            if (_serviceProvider == null)
            {
                throw new InvalidOperationException(
                    $"The caller forgot to configure an {nameof(IServiceProvider)} in the constructor of this class.");
            }
            
            var connectionStringAccessor = _serviceProvider.GetRequiredService<IConnectionStringAccessor>();
            context = new MigrationContext(
                Processor,
                _serviceProvider,
                connectionStringAccessor.ConnectionString);
            

            getExpressions(migration, context);

            _migrationValidator.ApplyConventionsToAndValidateExpressions(migration, context.Expressions);
            ExecuteExpressions(context.Expressions);
        }

        /// <inheritdoc />
        public void Down(IMigration migration)
        {
            var migrationInfoAdapter = new NonAttributedMigrationToMigrationInfoAdapter(migration);

            ApplyMigrationDown(migrationInfoAdapter, true);
        }

        /// <summary>
        /// Execute each migration expression in the expression collection
        /// </summary>
        /// <param name="expressions">The expressions to execute</param>
        protected void ExecuteExpressions(ICollection<IMigrationExpression> expressions)
        {
            long insertTicks = 0;
            var insertCount = 0;
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
                    _logger.LogError(er, er.Message);

                    //catch the error and move onto the next expression
                    if (SilentlyFail)
                    {
                        _caughtExceptions.Add(er);
                        continue;
                    }
                    throw;
                }
            }

            if (insertCount > 0)
            {
                var avg = new TimeSpan(insertTicks / insertCount);
                var msg = string.Format("-> {0} Insert operations completed in {1} taking an average of {2}", insertCount, new TimeSpan(insertTicks), avg);
                _logger.LogSay(msg);
            }
        }

        private void AnnounceTime(string message, Action action)
        {
            _logger.LogSay(message);
            _logger.LogElapsedTime(_stopWatch.Time(action));
        }

        /// <inheritdoc />
        public void ValidateVersionOrder()
        {
            var unappliedVersions = MigrationLoader.LoadMigrations().Where(kvp => MigrationVersionLessThanGreatestAppliedMigration(kvp.Key)).ToList();
            if (unappliedVersions.Any())
            {
                throw new VersionOrderInvalidException(unappliedVersions);
            }

            _logger.LogSay("Version ordering valid.");
        }

        /// <inheritdoc />
        public void ListMigrations()
        {
            var currentVersionInfo = VersionLoader.VersionInfo;
            var currentVersion = currentVersionInfo.Latest();

            _logger.LogHeader("Migrations");

            foreach (var migration in MigrationLoader.LoadMigrations())
            {
                var migrationName = migration.Value.GetName();
                var status = GetStatus(migration, currentVersion);
                var statusString = string.Join(", ", GetStatusStrings(status));
                var message = $"{migrationName}{(string.IsNullOrEmpty(statusString) ? string.Empty : $" ({statusString})")}";

                var isCurrent = (status & MigrationStatus.AppliedMask) == MigrationStatus.Current;
                var isBreaking = (status & MigrationStatus.Breaking) == MigrationStatus.Breaking;
                if (isBreaking)
                {
                    const string skull = "💀";
                    _logger.LogEmphasized($"{skull} {message}");
                }
                else if (isCurrent)
                {
                    const string placeOfInterestSign = "⌘";
                    _logger.LogSay($"{placeOfInterestSign} {message}");
                }
                else
                {
                    _logger.LogSay(message);
                }
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
            {
                yield return "BREAKING";
            }
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

        /// <summary>
        /// Validates that the database connection can be established
        /// </summary>
        /// <exception cref="UndeterminableConnectionException">Thrown when the connection string is invalid or the database is unreachable</exception>
        private void ValidateConnection()
        {
            // Skip connection validation in preview mode as no actual database connection is needed
            if (_processorOptions.PreviewOnly)
            {
                return;
            }

            try
            {
                // Attempt to begin and immediately rollback a transaction to test the connection
                // This will force the processor to open the connection and validate it
                Processor.BeginTransaction();
                Processor.RollbackTransaction();
            }
            catch (Exception ex)
            {
                // Wrap the exception in UndeterminableConnectionException to provide consistent error handling
                throw new UndeterminableConnectionException($"Failed to establish database connection. Please verify your connection string and ensure the database server is accessible. Error: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public IMigrationScope BeginScope()
        {
            return _migrationScopeManager.BeginScope();
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
