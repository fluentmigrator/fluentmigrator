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

using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner.Initialization
{
    [CLSCompliant(false)]
    public class TaskExecutor
    {
        [NotNull]
        private readonly IRunnerContext _runnerContext;

        [NotNull]
        private readonly AssemblyLoaderFactory _assemblyLoaderFactory;

        [NotNull, ItemNotNull]
        private readonly Lazy<IServiceProvider> _lazyServiceProvider;

        private IReadOnlyCollection<Assembly> _assemblies;

        public TaskExecutor([NotNull] IRunnerContext runnerContext)
        {
            _runnerContext = runnerContext ?? throw new ArgumentNullException(nameof(runnerContext));
            _assemblyLoaderFactory = new AssemblyLoaderFactory();
            ConnectionStringProvider = new DefaultConnectionStringProvider();
            _lazyServiceProvider = new Lazy<IServiceProvider>(() =>
            {
                return runnerContext.CreateServices(
                        (loader, context) => GetTargetAssemblies(),
                        ConnectionStringProvider,
                        _assemblyLoaderFactory)
                    .BuildServiceProvider(validateScopes: true);
            });
        }

        public TaskExecutor([NotNull] IRunnerContext runnerContext, [NotNull] IServiceProvider serviceProvider)
        {
            _runnerContext = runnerContext ?? throw new ArgumentNullException(nameof(runnerContext));
            ConnectionStringProvider = serviceProvider.GetService<IConnectionStringProvider>();
            _assemblyLoaderFactory = serviceProvider.GetRequiredService<AssemblyLoaderFactory>();
            _lazyServiceProvider = new Lazy<IServiceProvider>(() => serviceProvider);
        }

        [Obsolete("Ony the statically provided factories are accessed")]
        public TaskExecutor(
            [NotNull] IRunnerContext runnerContext,
            [CanBeNull] IConnectionStringProvider connectionStringProvider,
            [NotNull] AssemblyLoaderFactory assemblyLoaderFactory,
            MigrationProcessorFactoryProvider _)
            : this(
                runnerContext,
                assemblyLoaderFactory,
                connectionStringProvider)
        {
        }

        public TaskExecutor(
            [NotNull] IRunnerContext runnerContext,
            [NotNull] AssemblyLoaderFactory assemblyLoaderFactory,
            [CanBeNull] IConnectionStringProvider connectionStringProvider = null)
        {
            _runnerContext = runnerContext ?? throw new ArgumentNullException(nameof(runnerContext));
            ConnectionStringProvider = connectionStringProvider;
            _assemblyLoaderFactory = assemblyLoaderFactory ?? throw new ArgumentNullException(nameof(assemblyLoaderFactory));
            _lazyServiceProvider = new Lazy<IServiceProvider>(
                () => runnerContext
                    .CreateServices(
                        (loader, context) => GetTargetAssemblies(),
                        connectionStringProvider,
                        _assemblyLoaderFactory)
                    .BuildServiceProvider(validateScopes: true));
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
        /// Gets the connection string provider
        /// </summary>
        [CanBeNull]
        protected IConnectionStringProvider ConnectionStringProvider { get; }

        /// <summary>
        /// Gets the service provider used to instantiate all migration services
        /// </summary>
        [NotNull]
        protected IServiceProvider ServiceProvider => _lazyServiceProvider.Value;

        protected virtual IEnumerable<Assembly> GetTargetAssemblies()
        {
            return _assemblies ?? (_assemblies = _assemblyLoaderFactory.GetTargetAssemblies(_runnerContext.Targets).ToList());
        }

        /// <summary>
        /// Will be called durin the runner scope intialization
        /// </summary>
        /// <remarks>
        /// The <see cref="Runner"/> isn't initialized yet.
        /// </remarks>
        protected virtual void Initialize()
        {
        }

        public void Execute()
        {
            using (var scope = new RunnerScope(this))
            {
                switch (_runnerContext.Task)
                {
                    case null:
                    case "":
                    case "migrate":
                    case "migrate:up":
                        if (_runnerContext.Version != 0)
                            scope.Runner.MigrateUp(_runnerContext.Version);
                        else
                            scope.Runner.MigrateUp();
                        break;
                    case "rollback":
                        if (_runnerContext.Steps == 0)
                            _runnerContext.Steps = 1;
                        scope.Runner.Rollback(_runnerContext.Steps);
                        break;
                    case "rollback:toversion":
                        scope.Runner.RollbackToVersion(_runnerContext.Version);
                        break;
                    case "rollback:all":
                        scope.Runner.RollbackToVersion(0);
                        break;
                    case "migrate:down":
                        scope.Runner.MigrateDown(_runnerContext.Version);
                        break;
                    case "validateversionorder":
                        scope.Runner.ValidateVersionOrder();
                        break;
                    case "listmigrations":
                        scope.Runner.ListMigrations();
                        break;
                }
            }

            _runnerContext.Announcer.Say("Task completed.");
        }

        /// <summary>
        /// Checks whether the current task will actually run any migrations.
        /// Can be used to decide whether it's necessary perform a backup before the migrations are executed.
        /// </summary>
        public bool HasMigrationsToApply()
        {
            using (var scope = new RunnerScope(this))
            {
                switch (_runnerContext.Task)
                {
                    case null:
                    case "":
                    case "migrate":
                    case "migrate:up":
                        if (_runnerContext.Version != 0)
                            return scope.Runner.HasMigrationsToApplyUp(_runnerContext.Version);

                        return scope.Runner.HasMigrationsToApplyUp();
                    case "rollback":
                    case "rollback:all":
                        // Number of steps doesn't matter as long as there's at least
                        // one migration applied (at least that one will be rolled back)
                        return scope.Runner.HasMigrationsToApplyRollback();
                    case "rollback:toversion":
                    case "migrate:down":
                        return scope.Runner.HasMigrationsToApplyDown(_runnerContext.Version);
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
                    Runner.Processor?.Dispose();
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
