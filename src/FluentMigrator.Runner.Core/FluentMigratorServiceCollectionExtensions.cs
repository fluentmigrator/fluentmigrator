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
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Infrastructure;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
#if NETFRAMEWORK
using FluentMigrator.Runner.Initialization.NetFramework;
#endif
using FluentMigrator.Runner.Logging;
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
        /// <remarks>
        /// This overload scans assemblies for migrations at runtime using reflection and is not
        /// compatible with trimmed or NativeAOT applications. For AOT-compatible usage, use
        /// <see cref="AddFluentMigratorSlim"/> and register types explicitly
        /// with <c>.WithTypes(typeof(MyMigration), ...)</c>.
        /// </remarks>
        [NotNull]
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses AppDomain to load assemblies, which may not be preserved in trimmed applications. Use AddFluentMigratorSlim().WithTypes(new Type[] { ... }) instead.")] 
#endif
        public static IServiceCollection AddFluentMigratorCore(
            [NotNull] this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

#pragma warning disable IL2026 // Assembly scanning; callers are warned by [RequiresUnreferencedCode] above
            services
                // The default assembly loader factory
                .TryAddSingleton<AssemblyLoaderFactory>();

            services
                // Defines the assemblies that are used to find migrations, profiles, maintenance code, etc...
                .TryAddSingleton<IAssemblySource, AssemblySource>();

            services
                // Provides the types out of the assemblies
                .TryAddSingleton<ITypeSource, AssemblyTypeSource>();

            services
                // Assembly loader engines
                .AddSingleton<IAssemblyLoadEngine, AssemblyNameLoadEngine>()
                .AddSingleton<IAssemblyLoadEngine, AssemblyFileLoadEngine>();
#pragma warning restore IL2026

            return AddFluentMigratorSlim(services);
        }

        /// <summary>
        /// Adds migration runner services to the specified <see cref="IServiceCollection"/> without registering
        /// the assembly-scanning infrastructure. This is the recommended entry point for trimmed and NativeAOT
        /// applications.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The updated service collection</returns>
        /// <remarks>
        /// After calling this method, register migration types explicitly using
        /// <c>ConfigureRunner(rb => rb.WithTypes(typeof(MyMigration), ...))</c>.
        /// Assembly scanning (<c>ScanIn</c>) is not available when using this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// services.AddFluentMigratorSlim()
        ///     .ConfigureRunner(rb => rb
        ///         .AddSQLite()
        ///         .WithGlobalConnectionString("Data Source=:memory:")
        ///         .WithTypes(typeof(Migration001_CreateUsers), typeof(Migration002_AddEmail)));
        /// </code>
        /// </example>
        [NotNull]
        public static IServiceCollection AddFluentMigratorSlim(
            [NotNull] this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services
                // Add support for options
                .AddOptions()

                // Add logging support
                .AddLogging()
                .AddScoped<ILogger>(provider => NullLogger.Instance);

            services
                .TryAddSingleton<IAssemblySource, EmptyAssemblySource>();

            services
                .TryAddSingleton<ITypeSource>(new ArrayTypeSource(Array.Empty<Type>()));

#pragma warning disable IL2026
            services
                // Configure the loader for migrations that should be executed during maintenance steps
                .TryAddSingleton<IMaintenanceLoader>(sp => ActivatorUtilities.CreateInstance<MaintenanceLoader>(sp));
#pragma warning restore IL2026

            services
                // Add the default embedded resource provider
                .AddSingleton<IEmbeddedResourceProvider>(sp => new DefaultEmbeddedResourceProvider(sp.GetRequiredService<IAssemblySource>().Assemblies));

                // Configure the runner
#pragma warning disable IL2026
            services
                .TryAddSingleton<IMigrationRunnerConventionsAccessor, AssemblySourceMigrationRunnerConventionsAccessor>();
#pragma warning restore IL2026

            services
                .TryAddSingleton(sp => sp.GetRequiredService<IMigrationRunnerConventionsAccessor>().MigrationRunnerConventions);

            services
                .TryAddSingleton(sp =>
                {
                    var conventions = sp.GetRequiredService<IMigrationRunnerConventions>();
                    if (conventions is IMigrationRunnerTagConventions tagConventions)
                    {
                        return tagConventions;
                    }

                    return DefaultMigrationRunnerConventions.Instance as IMigrationRunnerTagConventions;
                });

            services
                // The IStopWatch implementation used to show query timing
                .TryAddSingleton<IStopWatch, StopWatch>();

#pragma warning disable 618
#pragma warning disable IL2026
            services
                // Source for migrations
                .TryAddScoped<IMigrationSource>(sp =>
                    new MigrationSource(
                        sp.GetRequiredService<ITypeSource>(),
                        sp.GetRequiredService<IMigrationRunnerConventions>(),
                        sp,
                        sp.GetServices<IMigrationSourceItem>(),
                        sp.GetRequiredService<ILogger>()));

            services
                .TryAddScoped(
                    sp => sp.GetRequiredService<IMigrationSource>() as IFilteringMigrationSource
                     ?? new MigrationSource(
                        sp.GetRequiredService<ITypeSource>(),
                        sp.GetRequiredService<IMigrationRunnerConventions>(),
                        sp,
                        sp.GetServices<IMigrationSourceItem>(),
                        sp.GetRequiredService<ILogger>()));
#pragma warning restore IL2026
#pragma warning restore 618

            services
                // Source for profiles
                .TryAddScoped<IProfileSource, ProfileSource>();

            services
                // Configure the accessor for the convention set
                .TryAddScoped<IConventionSetAccessor, TypeSourceConventionSetAccessor>();

            services
                // The default set of conventions to be applied to migration expressions
                .TryAddScoped(sp =>
                    sp.GetRequiredService<IConventionSetAccessor>().GetConventionSet()
                    ?? ActivatorUtilities.CreateInstance<DefaultConventionSet>(sp)
                    );

            services
                // Provides well-known token variables (e.g. DefaultSchema) usable from
                // Execute.Sql/Script/EmbeddedScript. Additional providers may be registered
                // alongside this one.
                .TryAddEnumerable(ServiceDescriptor.Scoped<ISqlTokenProvider, DefaultSqlTokenProvider>());

            services
                // Gives Schema.Table(...).Exists() and the other schema queries the convention set's
                // default schema, as the runner gives it to expressions (#1055)
                .TryAddScoped<IDefaultSchemaNameResolver, ConventionSetDefaultSchemaNameResolver>();

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
                        var connectionFactory = sp.GetRequiredService<IMigrationConnectionFactory>();

                        if (options.Value.NoConnection || !connectionFactory.HasConnection)
                        {
                            return ActivatorUtilities.CreateInstance<ConnectionlessVersionLoader>(sp);
                        }

                        // The constructor checks for, and creates, the version table. Hold the application
                        // lock so that instances starting together don't all try to create it (#1949), even
                        // when IVersionLoader is resolved directly rather than through the runner.
                        using (sp.GetRequiredService<IApplicationLockLease>().Enter())
                        {
                            return ActivatorUtilities.CreateInstance<VersionLoader>(sp);
                        }
                    });

            services
                // The scope's share of the application lock; a no-op unless an IApplicationLocker is registered
                .TryAddScoped<IApplicationLockLease>(
                    sp =>
                    {
                        var options = sp.GetRequiredService<IOptions<RunnerOptions>>();
                        var processorOptions = sp.GetRequiredService<IOptionsSnapshot<ProcessorOptions>>();
                        var connectionFactory = sp.GetRequiredService<IMigrationConnectionFactory>();

                        // In preview mode, with or without a connection, nothing is written, so no lock is taken.
                        var locker = options.Value.NoConnection || processorOptions.Value.PreviewOnly || !connectionFactory.HasConnection
                            ? null
                            : sp.GetService<IApplicationLocker>();

                        return new ApplicationLockLease(
                            locker,
                            sp.GetRequiredService<IOptions<ApplicationLockerOptions>>().Value.AcquireTimeout,
                            sp.GetService<ILogger<ApplicationLockLease>>());
                    });

            services
                // Configure the runner. Wrap it in the application lock only when a locker is configured, so
                // applications without one still receive the MigrationRunner instance itself.
                //
                // MigrationRunner is deliberately not registered as a service of its own: resolving it
                // would hand out a runner that bypasses the lock.
                .TryAddScoped<IMigrationRunner>(
                    sp =>
                    {
                        // Resolve the lease before the runner. The runner creates the processor, and a scope
                        // disposes its services in reverse order of creation, so the processor rolls back any
                        // transaction left open by an undisposed BeginScope() before the lease releases the lock.
                        var lease = sp.GetRequiredService<IApplicationLockLease>();
                        var runner = ActivatorUtilities.CreateInstance<MigrationRunner>(sp);

                        return lease.IsEnabled
                            ? new ApplicationLockingMigrationRunner(runner, lease)
                            : runner;
                    });

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

            services
                // Connection Factory
                .TryAddScoped<IMigrationConnectionFactory, ConnectionStringMigrationConnectionFactory>();

            return services;
        }

        /// <summary>
        /// Registers the application-wide lock that keeps concurrent application instances from migrating
        /// the same database at the same time.
        /// </summary>
        /// <typeparam name="TApplicationLocker">The <see cref="IApplicationLocker"/> implementation, registered scoped.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <param name="configure">Optionally configures <see cref="ApplicationLockerOptions"/>, such as the acquisition timeout.</param>
        /// <returns>The updated service collection.</returns>
        /// <remarks>
        /// Once registered, every runner operation and every migration scope holds the lock, and so does
        /// version-table creation. See <see cref="IApplicationLocker"/> for the contract an implementation must meet.
        /// </remarks>
        /// <example>
        /// <code>
        /// services
        ///     .AddFluentMigratorCore()
        ///     .ConfigureApplicationLocker&lt;SqlServerApplicationLocker&gt;(o => o.AcquireTimeout = TimeSpan.FromMinutes(5));
        /// </code>
        /// </example>
        public static IServiceCollection ConfigureApplicationLocker<
#if NET
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            TApplicationLocker>(
            [NotNull] this IServiceCollection services,
            [CanBeNull] Action<ApplicationLockerOptions> configure = null)
            where TApplicationLocker : class, IApplicationLocker
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.RemoveAll<IApplicationLocker>();
            services.AddScoped<IApplicationLocker, TApplicationLocker>();
            return ConfigureApplicationLockerOptions(services, configure);
        }

        /// <summary>
        /// Registers the application-wide lock that keeps concurrent application instances from migrating
        /// the same database at the same time.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <param name="factory">Creates the <see cref="IApplicationLocker"/>, once per dependency-injection scope.</param>
        /// <param name="configure">Optionally configures <see cref="ApplicationLockerOptions"/>, such as the acquisition timeout.</param>
        /// <returns>The updated service collection.</returns>
        /// <remarks>
        /// Once registered, every runner operation and every migration scope holds the lock, and so does
        /// version-table creation. See <see cref="IApplicationLocker"/> for the contract an implementation must meet.
        /// </remarks>
        public static IServiceCollection ConfigureApplicationLocker(
            [NotNull] this IServiceCollection services,
            [NotNull] Func<IServiceProvider, IApplicationLocker> factory,
            [CanBeNull] Action<ApplicationLockerOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            services.RemoveAll<IApplicationLocker>();
            services.AddScoped(factory);
            return ConfigureApplicationLockerOptions(services, configure);
        }

        private static IServiceCollection ConfigureApplicationLockerOptions(
            IServiceCollection services,
            Action<ApplicationLockerOptions> configure)
        {
            var optionsBuilder = services.AddOptions<ApplicationLockerOptions>();
            if (configure != null)
            {
                optionsBuilder.Configure(configure);
            }

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
