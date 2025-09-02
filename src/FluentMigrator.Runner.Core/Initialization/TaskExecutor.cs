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
using System.Reflection;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Represents the main task execution mechanism within the FluentMigrator runner.
    /// </summary>
    /// <remarks>
    /// This class is responsible for managing the initialization and execution of migration tasks.
    /// It provides functionality to determine if migrations need to be applied and handles the execution
    /// of those migrations within a controlled scope.
    /// </remarks>
    public class TaskExecutor
    {
        [NotNull]
        private readonly ILogger _logger;

        [NotNull]
        private readonly IAssemblySource _assemblySource;

        private readonly RunnerOptions _runnerOptions;

        [NotNull, ItemNotNull]
        private readonly Lazy<IServiceProvider> _lazyServiceProvider;

        private IReadOnlyCollection<Assembly> _assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskExecutor"/> class.
        /// </summary>
        /// <param name="logger">The logger instance used for logging execution details.</param>
        /// <param name="assemblySource">The source of assemblies containing migrations.</param>
        /// <param name="runnerOptions">The configuration options for the migration runner.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <remarks>
        /// This constructor sets up the necessary dependencies for the <see cref="TaskExecutor"/> 
        /// to manage and execute migration tasks effectively.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the required parameters (<paramref name="logger"/>, 
        /// <paramref name="assemblySource"/>, <paramref name="runnerOptions"/>, 
        /// or <paramref name="serviceProvider"/>) are <c>null</c>.
        /// </exception>
        public TaskExecutor(
            [NotNull] ILogger<TaskExecutor> logger,
            [NotNull] IAssemblySource assemblySource,
            [NotNull] IOptions<RunnerOptions> runnerOptions,
            [NotNull] IServiceProvider serviceProvider)
        {
            _logger = logger;
            _assemblySource = assemblySource;
            _runnerOptions = runnerOptions.Value;
            _lazyServiceProvider = new Lazy<IServiceProvider>(() => serviceProvider);
        }

        /// <summary>
        /// Gets the current migration runner
        /// </summary>
        /// <remarks>
        /// This will only be set during a migration operation
        /// </remarks>
        [CanBeNull]
        protected IMigrationRunner Runner { get; set; }

        /// <summary>
        /// Gets the service provider used to instantiate all migration services
        /// </summary>
        [NotNull]
        protected IServiceProvider ServiceProvider => _lazyServiceProvider.Value;

        /// <summary>
        /// Retrieves the target assemblies that contain migration classes.
        /// </summary>
        /// <remarks>
        /// This method fetches the assemblies from the configured <see cref="IAssemblySource"/>.
        /// If the assemblies have already been retrieved, the cached collection is returned.
        /// </remarks>
        /// <returns>
        /// A collection of <see cref="Assembly"/> instances representing the target assemblies.
        /// </returns>
        /// <seealso cref="IAssemblySource.Assemblies"/>
        [Obsolete]
        protected virtual IEnumerable<Assembly> GetTargetAssemblies()
        {
            return _assemblies ?? (_assemblies = _assemblySource.Assemblies);
        }

        /// <summary>
        /// Will be called during the runner scope initialization
        /// </summary>
        /// <remarks>
        /// The <see cref="Runner"/> isn't initialized yet.
        /// </remarks>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// Executes the specified migration task based on the configured options.
        /// </summary>
        /// <remarks>
        /// This method determines the type of migration task to execute (e.g., migrate, rollback, validate version order, etc.)
        /// and performs the corresponding operation using the <see cref="IMigrationRunner"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the migration task cannot be executed due to invalid or missing configuration.
        /// </exception>
        /// <example>
        /// Example usage:
        /// <code>
        /// var executor = new TaskExecutor(logger, assemblySource, runnerOptions, serviceProvider);
        /// executor.Execute();
        /// </code>
        /// </example>
        public void Execute()
        {
            using (var scope = new RunnerScope(this))
            {
                switch (_runnerOptions.Task)
                {
                    case null:
                    case "":
                    case "migrate":
                    case "migrate:up":
                        if (_runnerOptions.Version != 0)
                            scope.Runner.MigrateUp(_runnerOptions.Version);
                        else
                            scope.Runner.MigrateUp();
                        break;
                    case "rollback":
                        if (_runnerOptions.Steps == 0)
                            _runnerOptions.Steps = 1;
                        scope.Runner.Rollback(_runnerOptions.Steps);
                        break;
                    case "rollback:toversion":
                        scope.Runner.RollbackToVersion(_runnerOptions.Version);
                        break;
                    case "rollback:all":
                        scope.Runner.RollbackToVersion(0);
                        break;
                    case "migrate:down":
                        scope.Runner.MigrateDown(_runnerOptions.Version);
                        break;
                    case "validateversionorder":
                        scope.Runner.ValidateVersionOrder();
                        break;
                    case "listmigrations":
                        scope.Runner.ListMigrations();
                        break;
                }
            }

            _logger.LogSay("Task completed.");
        }

        /// <summary>
        /// Checks whether the current task will actually run any migrations.
        /// Can be used to decide whether it's necessary perform a backup before the migrations are executed.
        /// </summary>
        public bool HasMigrationsToApply()
        {
            using (var scope = new RunnerScope(this))
            {
                switch (_runnerOptions.Task)
                {
                    case null:
                    case "":
                    case "migrate":
                    case "migrate:up":
                        if (_runnerOptions.Version != 0)
                            return scope.Runner.HasMigrationsToApplyUp(_runnerOptions.Version);

                        return scope.Runner.HasMigrationsToApplyUp();
                    case "rollback":
                    case "rollback:all":
                        // Number of steps doesn't matter as long as there's at least
                        // one migration applied (at least that one will be rolled back)
                        return scope.Runner.HasMigrationsToApplyRollback();
                    case "rollback:toversion":
                    case "migrate:down":
                        return scope.Runner.HasMigrationsToApplyDown(_runnerOptions.Version);
                    default:
                        return false;
                }
            }
        }

        private class RunnerScope : IDisposable
        {
            [NotNull]
            private readonly TaskExecutor _executor;

            [CanBeNull]
            private readonly IServiceScope _serviceScope;

            private readonly bool _hasCustomRunner;

            public RunnerScope([NotNull] TaskExecutor executor)
            {
                _executor = executor;

                executor.Initialize();

                if (executor.Runner != null)
                {
                    Runner = executor.Runner;
                    _hasCustomRunner = true;
                }
                else
                {
                    var serviceScope = executor.ServiceProvider.CreateScope();
                    _serviceScope = serviceScope;
                    _executor.Runner = Runner = serviceScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                }
            }

            public IMigrationRunner Runner { get; }

            public void Dispose()
            {
                if (_hasCustomRunner)
                {
                    Runner.Processor.Dispose();
                }
                else
                {
                    _executor.Runner = null;
                    _serviceScope?.Dispose();
                }
            }
        }
    }
}
