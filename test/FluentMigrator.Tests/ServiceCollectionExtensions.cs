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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.VersionTableInfo;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection CreateServiceCollection()
        {
            return new ServiceCollection().Reset();
        }

        public static IServiceCollection CreateServices(this IMigrationProcessor processor)
        {
            return CreateServiceCollection()
                .WithProcessor(processor);
        }

        public static IServiceCollection Reset(this IServiceCollection services)
        {
            services.Clear();
            services
                // Create the announcer to output the migration messages
                .AddSingleton<IAnnouncer>(sp => new TextWriterAnnouncer(TestContext.Out) { ShowSql = true })

                // Processor specific options (usually none are needed)
                .AddScoped<IMigrationProcessorOptions, ProcessorOptions>()

                // Configure the default version table metadata
                .AddSingleton<IVersionTableMetaData, DefaultVersionTableMetaData>()

                // Configure the loader for migrations that should be executed during maintenance steps
                .AddScoped<IMaintenanceLoader, MaintenanceLoader>()

                // Configure the migration information loader
                .AddScoped<IMigrationInformationLoader, DefaultMigrationInformationLoader>()

                // Configure the runner context
                .AddScoped<IRunnerContext>(sp => new RunnerContext(sp.GetRequiredService<IAnnouncer>())
                {
                    AllowBreakingChange = true,
                })

                // Configure the runner conventions
                .AddScoped<IMigrationRunnerConventions, MigrationRunnerConventions>()

                // Configure the runner
                .AddScoped<IMigrationRunner, MigrationRunner>();

            return services;
        }

        public static IServiceCollection WithMigrations(
            [NotNull] this IServiceCollection services,
            [NotNull] Func<IServiceProvider, IEnumerable<IMigration>> migrations)
        {
            return services
                .AddScoped(migrations);
        }

        public static IServiceCollection WithAllTestMigrations(
            [NotNull] this IServiceCollection services)
        {
            return services
                .WithMigrationsIn(@namespace: null);
        }

        public static IServiceCollection WithMigrationsIn(
            [NotNull] this IServiceCollection services,
            [CanBeNull] string @namespace,
            bool recursive = false)
        {
            var allTypes = Assembly.GetExecutingAssembly().GetExportedTypes().ToList();
            var migrationTypes = allTypes
                .Where(t => typeof(IMigration).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract && t.IsClass);

            if (!string.IsNullOrEmpty(@namespace))
            {
                if (recursive)
                {
                    var namespaceWithDot = $"{@namespace}.";
                    migrationTypes = migrationTypes.Where(t => t.Namespace != null && (t.Namespace == @namespace || t.Namespace.StartsWith(namespaceWithDot)));
                }
                else
                {
                    migrationTypes = migrationTypes.Where(t => t.Namespace == @namespace);
                }
            }

            foreach (var migrationType in migrationTypes)
            {
                services = services
                    .AddScoped(typeof(IMigration), migrationType);
            }

            return services;
        }

        public static IServiceCollection WithProcessor(this IServiceCollection services, IMigrationProcessor processor)
        {
            return services
                .AddSingleton(processor)
                .AddSingleton<IQuerySchema>(_ => processor)
                .AddScoped<IMigrationContext>(sp =>
                {
                    var runnerContext = sp.GetRequiredService<IRunnerContext>();
                    return new MigrationContext(processor, runnerContext.ApplicationContext, runnerContext.Connection, sp);
                });
        }

        public static IServiceCollection WithRunnerContext(this IServiceCollection services, IRunnerContext runnerContext)
        {
            return services
                .AddScoped(sp => runnerContext);
        }

        public static IServiceCollection WithRunnerContext(this IServiceCollection services, Func<IServiceProvider, IRunnerContext> runnerContextFunc)
        {
            return services
                .AddScoped(runnerContextFunc);
        }

        public static IServiceCollection WithRunnerConventions(this IServiceCollection services, IMigrationRunnerConventions conventions)
        {
            return services
                .AddScoped(_ => conventions);
        }
    }
}
