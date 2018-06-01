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
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Validation;

using JetBrains.Annotations;

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

                // The default set of conventions to be applied to migration expressions
                .AddSingleton<IConventionSet, DefaultConventionSet>()

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
                .AddTransient<IMigrationContext, MigrationContext>();

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
