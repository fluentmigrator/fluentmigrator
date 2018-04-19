#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.VersionTableInfo;

using JetBrains.Annotations;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods
    /// </summary>
    [Obsolete]
    internal static class LegacyExtensions
    {
        /// <summary>
        /// Find the version table meta data in the given assembly collection
        /// </summary>
        /// <param name="assemblies">The assembly collection</param>
        /// <param name="conventionSet">The convention set whose schema convention should be applied to the default version table metadata</param>
        /// <param name="runnerConventions">The runner conventions used to identify a version table metadata type</param>
        /// <param name="runnerContext">The runner context defining the search boundaries for the custom version table metadata type</param>
        /// <returns>A custom or the default version table metadata instance</returns>
        [Obsolete]
        public static IVersionTableMetaData GetVersionTableMetaData(
            [CanBeNull] this IAssemblyCollection assemblies,
            [NotNull] IConventionSet conventionSet,
            [NotNull] IMigrationRunnerConventions runnerConventions,
            [NotNull] IRunnerContext runnerContext)
        {
            if (assemblies == null)
            {
                var result = new DefaultVersionTableMetaData();
                conventionSet.SchemaConvention?.Apply(result);
                return result;
            }

            var matchedType = assemblies.GetExportedTypes()
                .FilterByNamespace(runnerContext.Namespace, runnerContext.NestedNamespaces)
                .FirstOrDefault(t => runnerConventions.TypeIsVersionTableMetaData(t));

            if (matchedType == null)
            {
                var result = new DefaultVersionTableMetaData();
                conventionSet.SchemaConvention?.Apply(result);
                return result;
            }

            return (IVersionTableMetaData) Activator.CreateInstance(matchedType);
        }

        /// <summary>
        /// Search for custom migration runner conventions
        /// </summary>
        /// <param name="assemblies">The assemblies to search for</param>
        /// <returns>The custom or the default migration runner conventions</returns>
        [Obsolete]
        public static IMigrationRunnerConventions GetMigrationRunnerConventions(
            [CanBeNull] this IAssemblyCollection assemblies)
        {
            if (assemblies == null)
                return new MigrationRunnerConventions();

            var matchedType = assemblies
                .GetExportedTypes()
                .FirstOrDefault(t => typeof(IMigrationRunnerConventions).IsAssignableFrom(t));

            if (matchedType != null)
            {
                return (IMigrationRunnerConventions) Activator.CreateInstance(matchedType);
            }

            return new MigrationRunnerConventions();
        }

        /// <summary>
        /// Loads the connection string using the connection string provider for the given assembly
        /// </summary>
        /// <param name="assemblies">The assembly to load the connection string from</param>
        /// <param name="connectionStringProvider">The connection string provider</param>
        /// <param name="runnerContext">The runner context</param>
        /// <returns>The found connection string</returns>
        public static string LoadConnectionString(
            this IReadOnlyCollection<Assembly> assemblies,
            IConnectionStringProvider connectionStringProvider,
            IRunnerContext runnerContext)
        {
            var singleAssembly = assemblies.Count == 1 ? assemblies.Single() : null;
            var singleAssemblyLocation = singleAssembly != null ? singleAssembly.Location : string.Empty;

            var connectionString = connectionStringProvider.GetConnectionString(
                runnerContext.Announcer,
                runnerContext.Connection,
                runnerContext.ConnectionStringConfigPath,
                singleAssemblyLocation,
                runnerContext.Database);

            return connectionString;
        }

        /// <summary>
        /// Get all assemblies for the given assembly names
        /// </summary>
        /// <param name="loaderFactory">The factory to create an <see cref="IAssemblyLoader"/> for a given assembly (file) name</param>
        /// <param name="assemblyNames">The assembly (file) names</param>
        /// <returns>The collection of assemblies that could be loaded</returns>
        public static IEnumerable<Assembly> GetTargetAssemblies(
            this AssemblyLoaderFactory loaderFactory,
            IEnumerable<string> assemblyNames)
        {
            var assemblies = new HashSet<Assembly>();

            foreach (var target in assemblyNames)
            {
                var assembly = loaderFactory.GetAssemblyLoader(target).Load();
                if (assemblies.Add(assembly))
                {
                    yield return assembly;
                }
            }
        }

        /// <summary>
        /// Returns <c>true</c> when the type is probably a FluentMigrator-owned class
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns><c>true</c> when the type is probably a FluentMigrator-owned class</returns>
        internal static bool IsFluentMigratorRunnerType(this Type type)
        {
            return type.Namespace != null && type.Namespace.StartsWith("FluentMigrator.Runner.", StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the name for a given migration generator instance
        /// </summary>
        /// <param name="generator">The migration generator instance to get its name for</param>
        /// <returns>The name of the migration generator</returns>
        [NotNull]
        public static string GetName([NotNull] this IMigrationGenerator generator)
        {
            return generator.GetType().Name.Replace("Generator", string.Empty);
        }
    }
}
