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

using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Infrastructure;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.VersionTableInfo;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for the <see cref="IMigrationRunnerBuilder"/> interface
    /// </summary>
    [CLSCompliant(false)]
    public static class MigrationRunnerBuilderExtensions
    {
        /// <summary>
        /// Sets the announcer
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="announcer">The announcer to use</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithAnnouncer(this IMigrationRunnerBuilder builder, IAnnouncer announcer)
        {
            builder.Services.AddSingleton(_ => announcer);
            return builder;
        }

        /// <summary>
        /// Sets the migration processor options
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="options">The migration processor options</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithProcessorOptions(this IMigrationRunnerBuilder builder,
            IMigrationProcessorOptions options)
        {
            builder.Services.AddSingleton(_ => options);
            return builder;
        }

        /// <summary>
        /// Sets the version table meta data
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="versionTableMetaData">The version table meta data</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder ConfigureVersionTable(
            this IMigrationRunnerBuilder builder,
            IVersionTableMetaData versionTableMetaData)
        {
            builder.Services.AddSingleton(_ => versionTableMetaData);
            return builder;
        }

        /// <summary>
        /// Sets the runner context
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="context">The runner context</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithRunnerContext(
            this IMigrationRunnerBuilder builder,
            IRunnerContext context)
        {
            builder.Services.AddScoped(_ => context);
            return builder;
        }

        /// <summary>
        /// Sets the migration runner conventions
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="conventions">The migration runner conventions</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithRunnerConventions(
            this IMigrationRunnerBuilder builder,
            IMigrationRunnerConventions conventions)
        {
            builder.Services.AddScoped(_ => conventions);
            return builder;
        }

        /// <summary>
        /// Adds the migrations
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="targets">The target assemblies</param>
        /// <param name="namespace">The required namespace</param>
        /// <param name="withNestedNamespaces">Indicates whether migrations in the nested namespaces should be used too</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder AddMigrations(
            this IMigrationRunnerBuilder builder,
            IEnumerable<string> targets,
            [CanBeNull] string @namespace = null,
            bool withNestedNamespaces = false)
        {
            using (var sp = builder.Services.BuildServiceProvider())
            {
                var loaderFactory = sp.GetService<AssemblyLoaderFactory>() ?? new AssemblyLoaderFactory();
                var assemblies = loaderFactory.GetTargetAssemblies(targets);
                return builder.AddMigrations(assemblies, @namespace, withNestedNamespaces);
            }
        }

        /// <summary>
        /// Adds the migrations
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="assemblies">The target assemblies</param>
        /// <param name="namespace">The required namespace</param>
        /// <param name="withNestedNamespaces">Indicates whether migrations in the nested namespaces should be used too</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder AddMigrations(
            this IMigrationRunnerBuilder builder,
            IEnumerable<Assembly> assemblies,
            [CanBeNull] string @namespace = null,
            bool withNestedNamespaces = false)
        {
            var migrations = GetExportedTypes(assemblies)
                .FilterByNamespace(@namespace, withNestedNamespaces)
                .Where(t => DefaultMigrationRunnerConventions.Instance.TypeIsMigration(t))
                .ToList();
            if (migrations.Count == 0)
            {
                throw new MissingMigrationsException($"No migrations found in the namespace {@namespace}");
            }

            foreach (var migration in migrations)
            {
                builder.Services.AddScoped(typeof(IMigration), migration);
            }

            return builder;
        }

        private static Type[] GetExportedTypes(IEnumerable<Assembly> assemblies)
        {
            var result = new List<Type>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    result.AddRange(assembly.GetExportedTypes());
                }
                catch
                {
                    // Ignore assemblies that couldn't be loaded
                }
            }

            return result.ToArray();
        }
    }
}
