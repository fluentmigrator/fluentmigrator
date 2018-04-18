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
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.VersionTableInfo;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up the migration runner services in an <see cref="IServiceCollection"/>.
    /// </summary>
    [CLSCompliant(false)]
    public static class FluentMigratorServiceCollectionExtensions
    {
        /// <summary>
        /// Adds migration runner services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TMigrationProcessorFactory">The type of the migration processor factory</typeparam>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="connectionString">The connection string used to connect to the database</param>
        /// <param name="configure">The <see cref="IMigrationRunnerBuilder"/> configuration delegate.</param>
        /// <returns>The updated service collection</returns>
        [NotNull]
        public static IServiceCollection AddFluentMigrator<TMigrationProcessorFactory>(
            [NotNull] this IServiceCollection services,
            [NotNull] string connectionString,
            [NotNull] Action<IMigrationRunnerBuilder> configure)
            where TMigrationProcessorFactory : class, IMigrationProcessorFactory
        {
            return services.AddFluentMigrator(typeof(TMigrationProcessorFactory), connectionString, configure);
        }

        /// <summary>
        /// Adds migration runner services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="migrationProcessorFactoryType">The type of the migration processor factory</param>
        /// <param name="connectionString">The connection string used to connect to the database</param>
        /// <param name="configure">The <see cref="IMigrationRunnerBuilder"/> configuration delegate.</param>
        /// <returns>The updated service collection</returns>
        [NotNull]
        public static IServiceCollection AddFluentMigrator(
            [NotNull] this IServiceCollection services,
            [NotNull] Type migrationProcessorFactoryType,
            [NotNull] string connectionString,
            [NotNull] Action<IMigrationRunnerBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            services
                .AddFluentMigrator(connectionString)

                // Initialize the DB-specific processor
                .AddSingleton(typeof(IMigrationProcessorFactory), migrationProcessorFactoryType)
                .AddScoped(sp =>
                {
                    var processorFactory = sp.GetRequiredService<IMigrationProcessorFactory>();
                    var announcer = sp.GetRequiredService<IAnnouncer>();
                    var options = sp.GetRequiredService<IMigrationProcessorOptions>();
                    return processorFactory.Create(connectionString, announcer, options);
                });

            var builder = new MigrationRunnerBuilder(services);
            configure.Invoke(builder);

            return services;
        }

        /// <summary>
        /// Adds migration runner (except the DB processor specific) services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The updated service collection</returns>
        [NotNull]
        internal static IServiceCollection AddFluentMigrator(
            [NotNull] this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services
                // Create the announcer to output the migration messages
                .AddSingleton<IAnnouncer, NullAnnouncer>()

                // Processor specific options (usually none are needed)
                .AddSingleton<IMigrationProcessorOptions, ProcessorOptions>()

                // The default assembly loader factory
                .AddSingleton<AssemblyLoaderFactory>()

                // Add the default embedded resource provider
                .AddScoped<IEmbeddedResourceProvider, DefaultEmbeddedResourceProvider>()

                // Configure the default version table metadata
                .AddSingleton<IVersionTableMetaData, DefaultVersionTableMetaData>()

                // Configure the loader for migrations that should be executed during maintenance steps
                .AddScoped<IMaintenanceLoader, MaintenanceLoader>()

                // Configure the migration information loader
                .AddScoped<IMigrationInformationLoader, DefaultMigrationInformationLoader>()

                // Configure the runner context
                .AddScoped<IRunnerContext, RunnerContext>()

                // Configure the runner conventions
                .AddScoped<IMigrationRunnerConventions, MigrationRunnerConventions>()

                // The default set of conventions to be applied to migration expressions
                .AddScoped<IConventionSet, DefaultConventionSet>()

                // Configure the runner
                .AddScoped<IMigrationRunner, MigrationRunner>();

            return services;
        }

        private class MigrationRunnerBuilder : IMigrationRunnerBuilder
        {
            public MigrationRunnerBuilder(IServiceCollection services)
            {
                Services = services;
            }

            /// <inheritdoc />
            public IServiceCollection Services { get; }
        }
    }
}
