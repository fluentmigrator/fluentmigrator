#region License
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;

using FluentMigrator;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
#if NETFRAMEWORK
using FluentMigrator.Runner.Initialization.NetFramework;
#endif
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Validation;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up the migration runner services in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class FluentMigratorServiceCollectionExtensions
    {
        /// <summary>
        /// Adds migration runner (except the DB processor specific) services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The updated service collection</returns>
        [NotNull]
        public static IServiceCollection AddFluentMigratorCore(
            [NotNull] this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services
                // Add support for options
                .AddOptions()

                // Add logging support
                .AddLogging()
                .AddScoped<ILogger>(_ => NullLogger.Instance)

                // The default assembly loader factory
                .TryAddSingleton<AssemblyLoaderFactory>();

            services
                // Assembly loader engines
                .AddSingleton<IAssemblyLoadEngine, AssemblyNameLoadEngine>()
                .AddSingleton<IAssemblyLoadEngine, AssemblyFileLoadEngine>()

                // Defines the assemblies that are used to find migrations, profiles, maintenance code, etc...
                .TryAddSingleton<IAssemblySource, AssemblySource>();

            services
                // Configure the loader for migrations that should be executed during maintenance steps
                .TryAddSingleton<IMaintenanceLoader, MaintenanceLoader>();

            services
                // Add the default embedded resource provider
                .AddSingleton<IEmbeddedResourceProvider>(sp => new DefaultEmbeddedResourceProvider(sp.GetRequiredService<IAssemblySource>().Assemblies))

                // Configure the runner conventions
                .TryAddSingleton<IMigrationRunnerConventionsAccessor, AssemblySourceMigrationRunnerConventionsAccessor>();

            services
                .TryAddSingleton(sp => sp.GetRequiredService<IMigrationRunnerConventionsAccessor>().MigrationRunnerConventions);

            services
                // The IStopWatch implementation used to show query timing
                .TryAddSingleton<IStopWatch, StopWatch>();

            services
                // Source for migrations
                .TryAddScoped<IFilteringMigrationSource, MigrationSource>();

            services
                .TryAddScoped(
                    sp => sp.GetRequiredService<IFilteringMigrationSource>()
                     ?? ActivatorUtilities.CreateInstance<MigrationSource>(sp));

            services
                // Source for profiles
                .TryAddScoped<IProfileSource, ProfileSource>();

            services
                // Configure the accessor for the convention set
                .TryAddScoped<IConventionSetAccessor, AssemblySourceConventionSetAccessor>();

            services
                // The default set of conventions to be applied to migration expressions
                .TryAddScoped(sp =>
                    sp.GetRequiredService<IConventionSetAccessor>().GetConventionSet()
                    ?? ActivatorUtilities.CreateInstance<DefaultConventionSet>(sp)
                    );

            services
                // Configure the accessor for the version table metadata
                .TryAddScoped<IVersionTableMetaDataAccessor, AssemblySourceVersionTableMetaDataAccessor>();

            services
                // Configure the default version table metadata
                .TryAddScoped(sp => sp.GetRequiredService<IVersionTableMetaDataAccessor>().VersionTableMetaData ?? ActivatorUtilities.CreateInstance<DefaultVersionTableMetaData>(sp));

            services
                // Configure the migration information loader
                .TryAddScoped<IMigrationInformationLoader, DefaultMigrationInformationLoader>();

            services
                // Provide a way to get the migration generator selected by its options
                .TryAddScoped<IGeneratorAccessor, SelectingGeneratorAccessor>();

            services
                // Provide a way to get the migration accessor selected by its options
                .TryAddScoped<IProcessorAccessor, SelectingProcessorAccessor>();

            services
                // IQuerySchema is the base interface for the IMigrationProcessor
                .TryAddScoped<IQuerySchema>(sp => sp.GetRequiredService<IProcessorAccessor>().Processor);

            services
                // The profile loader needed by the migration runner
                .TryAddScoped<IProfileLoader, ProfileLoader>();

            services
                // Some services especially for the migration runner implementation
                .TryAddScoped<IMigrationExpressionValidator, DefaultMigrationExpressionValidator>();

            services
                .TryAddScoped<MigrationValidator>();
            services
                .TryAddScoped<MigrationScopeHandler>();
            // Register ProcessorOptions explicitly, required by MigrationScopeHandler
            services
                .TryAddScoped(sp => sp.GetRequiredService<IOptionsSnapshot<ProcessorOptions>>().Value);
            // The connection string readers
#if NETFRAMEWORK
            services
                .TryAddScoped<INetConfigManager, NetConfigManager>();
#endif
            services
                .AddScoped<IConnectionStringReader, ConfigurationConnectionStringReader>()

                // The connection string accessor that evaluates the readers
                .TryAddScoped<IConnectionStringAccessor, ConnectionStringAccessor>();

            services
                .TryAddScoped<IVersionLoader>(
                    sp =>
                    {
                        var options = sp.GetRequiredService<IOptions<RunnerOptions>>();
                        var connAccessor = sp.GetRequiredService<IConnectionStringAccessor>();
                        var hasConnection = !string.IsNullOrEmpty(connAccessor.ConnectionString);
                        if (options.Value.NoConnection || !hasConnection)
                        {
                            return ActivatorUtilities.CreateInstance<ConnectionlessVersionLoader>(sp);
                        }

                        return ActivatorUtilities.CreateInstance<VersionLoader>(sp);
                    });

            services
                // Configure the runner
                .TryAddScoped<IMigrationRunner, MigrationRunner>();

            services
                // Configure the task executor
                .TryAddScoped<TaskExecutor>();

            services
                // Migration context
                .TryAddTransient<IMigrationContext>(
                    sp =>
                    {
                        var querySchema = sp.GetRequiredService<IQuerySchema>();
                        var connectionStringAccessor = sp.GetRequiredService<IConnectionStringAccessor>();
                        var connectionString = connectionStringAccessor.ConnectionString;
                        return new MigrationContext(querySchema, sp, connectionString);
                    });

            return services;
        }

        /// <summary>
        /// Configures the migration runner
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="configure">The <see cref="IMigrationRunnerBuilder"/> configuration delegate.</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection ConfigureRunner(
            [NotNull] this IServiceCollection services,
            [NotNull] Action<IMigrationRunnerBuilder> configure)
        {
            var builder = new MigrationRunnerBuilder(services);
            configure.Invoke(builder);

            if (builder.DanglingAssemblySourceItem != null)
            {
                builder.Services
                    .AddSingleton(builder.DanglingAssemblySourceItem);
            }

            return services;
        }

        /// <inheritdoc />
        private class MigrationRunnerBuilder(IServiceCollection services) : IMigrationRunnerBuilder
        {
            /// <inheritdoc />
            public IServiceCollection Services { get; } = services;

            /// <inheritdoc />
            public IAssemblySourceItem DanglingAssemblySourceItem { get; set; } = null;
        }

        /// <inheritdoc />
        [UsedImplicitly]
        private class ConnectionlessProcessorAccessor(IServiceProvider serviceProvider) : IProcessorAccessor
        {
            /// <inheritdoc />
            public IMigrationProcessor Processor { get; } = ActivatorUtilities.CreateInstance<ConnectionlessProcessor>(serviceProvider);
        }
    }
}
