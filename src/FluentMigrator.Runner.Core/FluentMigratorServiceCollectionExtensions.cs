#region License
// Copyright (c) 2018, FluentMigrator Project
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
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Initialization.NetFramework;
using FluentMigrator.Runner.Logging;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Validation;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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

                // Add loggins support
                .AddLogging()

                // The default assembly loader factory
                .AddSingleton<AssemblyLoaderFactory>()

                // Assembly loader engines
                .AddSingleton<IAssemblyLoadEngine, AssemblyNameLoadEngine>()
                .AddSingleton<IAssemblyLoadEngine, AssemblyFileLoadEngine>()

                // Defines the assemblies that are used to find migrations, profiles, maintenance code, etc...
                .AddSingleton<IAssemblySource, AssemblySource>()

                // Configure the loader for migrations that should be executed during maintenance steps
                .AddSingleton<IMaintenanceLoader, MaintenanceLoader>()

                // Add the default embedded resource provider
                .AddSingleton<IEmbeddedResourceProvider>(sp => new DefaultEmbeddedResourceProvider(sp.GetRequiredService<IAssemblySource>().Assemblies))

                // Configure the runner conventions
                .AddSingleton<IMigrationRunnerConventionsAccessor, AssemblySourceMigrationRunnerConventionsAccessor>()
                .AddSingleton(sp => sp.GetRequiredService<IMigrationRunnerConventionsAccessor>().MigrationRunnerConventions)

                // The IStopWatch implementation used to show query timing
                .AddSingleton<IStopWatch, StopWatch>()

                // Source for migrations
#pragma warning disable 618
                .AddScoped<IMigrationSource, MigrationSource>()
                .AddScoped(
                    sp => sp.GetRequiredService<IMigrationSource>() as IFilteringMigrationSource
                     ?? ActivatorUtilities.CreateInstance<MigrationSource>(sp))
#pragma warning restore 618

                // Source for profiles
                .AddScoped<IProfileSource, ProfileSource>()

                // Configure the accessor for the convention set
                .AddScoped<IConventionSetAccessor, AssemblySourceConventionSetAccessor>()

                // The default set of conventions to be applied to migration expressions
                .TryAddScoped(sp =>
                    sp.GetRequiredService<IConventionSetAccessor>().GetConventionSet()
                    ?? ActivatorUtilities.CreateInstance<DefaultConventionSet>(sp)
                    );

            services
                // Configure the accessor for the version table metadata
                .AddScoped<IVersionTableMetaDataAccessor, AssemblySourceVersionTableMetaDataAccessor>()

                // Configure the default version table metadata
                .AddScoped(sp => sp.GetRequiredService<IVersionTableMetaDataAccessor>().VersionTableMetaData ?? ActivatorUtilities.CreateInstance<DefaultVersionTableMetaData>(sp))

                // Configure the migration information loader
                .AddScoped<IMigrationInformationLoader, DefaultMigrationInformationLoader>()

                // Provide a way to get the migration generator selected by its options
                .AddScoped<IGeneratorAccessor, SelectingGeneratorAccessor>()

                // Provide a way to get the migration accessor selected by its options
                .AddScoped<IProcessorAccessor, SelectingProcessorAccessor>()

                // IQuerySchema is the base interface for the IMigrationProcessor
                .AddScoped<IQuerySchema>(sp => sp.GetRequiredService<IProcessorAccessor>().Processor)

                // The profile loader needed by the migration runner
                .AddScoped<IProfileLoader, ProfileLoader>()

                // Some services especially for the migration runner implementation
                .AddScoped<IMigrationExpressionValidator, DefaultMigrationExpressionValidator>()
                .AddScoped<MigrationValidator>()
                .AddScoped<MigrationScopeHandler>()

                // The connection string readers
#if NETFRAMEWORK
                .AddScoped<INetConfigManager, NetConfigManager>()
#pragma warning disable 612
                .AddScoped<IConnectionStringReader, AppConfigConnectionStringReader>()
#pragma warning restore 612
#endif

                .AddScoped<IConnectionStringReader, ConfigurationConnectionStringReader>()

                // The connection string accessor that evaluates the readers
                .AddScoped<IConnectionStringAccessor, ConnectionStringAccessor>()

                .AddScoped<IVersionLoader>(
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
                    })

                // Configure the runner
                .AddScoped<IMigrationRunner, MigrationRunner>()

                // Configure the task executor
                .AddScoped<TaskExecutor>()

                // Migration context
                .AddTransient<IMigrationContext>(
                    sp =>
                    {
                        var querySchema = sp.GetRequiredService<IQuerySchema>();
                        var options = sp.GetRequiredService<IOptions<RunnerOptions>>();
                        var connectionStringAccessor = sp.GetRequiredService<IConnectionStringAccessor>();
                        var connectionString = connectionStringAccessor.ConnectionString;
#pragma warning disable 612
                        var appContext = options.Value.ApplicationContext;
#pragma warning restore 612
                        return new MigrationContext(querySchema, sp, appContext, connectionString);
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

        /// <summary>
        /// Creates services for a given runner context, connection string provider and assembly loader factory.
        /// </summary>
        /// <param name="runnerContext">The runner context</param>
        /// <param name="connectionStringProvider">The connection string provider</param>
        /// <param name="defaultAssemblyLoaderFactory">The assembly loader factory</param>
        /// <param name="configureRunner">The runner builder config deletage</param>
        /// <returns>The new service collection</returns>
        [NotNull]
        [Obsolete]
        internal static IServiceCollection CreateServices(
            [NotNull] this IRunnerContext runnerContext,
            [CanBeNull] IConnectionStringProvider connectionStringProvider,
            [CanBeNull] AssemblyLoaderFactory defaultAssemblyLoaderFactory = null,
            [CanBeNull] Action<IMigrationRunnerBuilder> configureRunner = null)
        {
            var services = new ServiceCollection();
            var assemblyLoaderFactory = defaultAssemblyLoaderFactory ?? new AssemblyLoaderFactory();

            if (!runnerContext.NoConnection && connectionStringProvider == null)
            {
                runnerContext.NoConnection = true;
            }

            // Configure the migration runner
            services
                .AddLogging(lb => lb.AddProvider(new LegacyFluentMigratorLoggerProvider(runnerContext.Announcer)))
                .AddFluentMigratorCore()
                .ConfigureRunner(c => configureRunner?.Invoke(c))
                .Configure<SelectingProcessorAccessorOptions>(opt => opt.ProcessorId = runnerContext.Database)
                .AddSingleton(assemblyLoaderFactory)
                .Configure<TypeFilterOptions>(
                    opt =>
                    {
                        opt.Namespace = runnerContext.Namespace;
                        opt.NestedNamespaces = runnerContext.NestedNamespaces;
                    })
                .Configure<AssemblySourceOptions>(opt => opt.AssemblyNames = runnerContext.Targets)
                .Configure<RunnerOptions>(
                    opt => { opt.SetValuesFrom(runnerContext); })
                .Configure<ProcessorOptions>(opt => { opt.SetValuesFrom(runnerContext); })
                .Configure<AppConfigConnectionStringAccessorOptions>(
                    opt => opt.ConnectionStringConfigPath = runnerContext.ConnectionStringConfigPath);

            // Configure the processor
            if (runnerContext.NoConnection)
            {
                // Always return the connectionless processor
                services
                    .AddScoped<IProcessorAccessor, ConnectionlessProcessorAccessor>();
            }

            return services;
        }

        private class MigrationRunnerBuilder : IMigrationRunnerBuilder
        {
            public MigrationRunnerBuilder(IServiceCollection services)
            {
                Services = services;
                DanglingAssemblySourceItem = null;
            }

            /// <inheritdoc />
            public IServiceCollection Services { get; }

            /// <inheritdoc />
            public IAssemblySourceItem DanglingAssemblySourceItem { get; set; }
        }

        [UsedImplicitly]
        private class ConnectionlessProcessorAccessor : IProcessorAccessor
        {
            public ConnectionlessProcessorAccessor(IServiceProvider serviceProvider)
            {
                Processor = ActivatorUtilities.CreateInstance<ConnectionlessProcessor>(serviceProvider);
            }

            /// <inheritdoc />
            public IMigrationProcessor Processor { get; }
        }
    }
}
