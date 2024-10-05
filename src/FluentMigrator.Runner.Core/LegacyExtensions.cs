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

using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Runner.Processors;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods
    /// </summary>
    [Obsolete]
    internal static class LegacyExtensions
    {
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

        /// <summary>
        /// Gets a <see cref="ProcessorOptions"/> instance for a given <see cref="IMigrationProcessorOptions"/> implementation
        /// </summary>
        /// <param name="options">The instance to get the <see cref="ProcessorOptions"/> for</param>
        /// <param name="connectionString">The connection string</param>
        /// <returns>The found/created <see cref="ProcessorOptions"/></returns>
        internal static ProcessorOptions GetProcessorOptions(this IMigrationProcessorOptions options, string connectionString)
        {
            if (options == null)
                return null;

            return options as ProcessorOptions ?? new ProcessorOptions()
            {
                ConnectionString = connectionString,
                PreviewOnly = options.PreviewOnly,
                ProviderSwitches = options.ProviderSwitches,
                Timeout = options.Timeout == null ? null : (TimeSpan?) TimeSpan.FromSeconds(options.Timeout.Value),
            };
        }

        /// <summary>
        /// Find the version table meta data in the given assembly collection
        /// </summary>
        /// <param name="assemblies">The assembly collection</param>
        /// <param name="runnerConventions">The runner conventions used to identify a version table metadata type</param>
        /// <param name="runnerContext">The runner context defining the search boundaries for the custom version table metadata type</param>
        /// <returns>A custom or the default version table metadata instance</returns>
        public static Type GetVersionTableMetaDataType(
            [CanBeNull] this IEnumerable<Assembly> assemblies,
            [NotNull] IMigrationRunnerConventions runnerConventions,
            [NotNull] IRunnerContext runnerContext)
        {
            if (assemblies == null)
            {
                return typeof(DefaultVersionTableMetaData);
            }

            var exportedTypes = assemblies.GetExportedTypes();

            var matchedType = exportedTypes
                .FilterByNamespace(runnerContext.Namespace, runnerContext.NestedNamespaces)
                .FirstOrDefault(t => runnerConventions.TypeIsVersionTableMetaData(t));

            if (matchedType == null)
            {
                return typeof(DefaultVersionTableMetaData);
            }

            return matchedType;
        }

        /// <summary>
        /// Find the version table meta data in the given assembly collection
        /// </summary>
        /// <param name="assemblies">The assembly collection</param>
        /// <param name="serviceProvider">The service provider to get all the required services from</param>
        /// <returns>A custom or the default version table metadata instance</returns>
        public static Type GetVersionTableMetaDataType(
            [CanBeNull] this IEnumerable<Assembly> assemblies,
            [NotNull] IServiceProvider serviceProvider)
        {
            var runnerConventions = serviceProvider.GetRequiredService<IMigrationRunnerConventions>();
            var runnerContext = serviceProvider.GetRequiredService<IRunnerContext>();
            return assemblies.GetVersionTableMetaDataType(runnerConventions, runnerContext);
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
        /// Get the collection of exported types
        /// </summary>
        /// <param name="assemblies">The assemblies to get the exported types from</param>
        /// <returns>The exported types</returns>
        public static IReadOnlyCollection<Type> GetExportedTypes(this IEnumerable<Assembly> assemblies)
        {
            var result = new List<Type>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var exportedType in assembly.GetExportedTypes())
                    {
                        if (!exportedType.IsAbstract && exportedType.IsClass)
                        {
                            result.Add(exportedType);
                        }
                    }
                }
                catch
                {
                    // Ignore assemblies that couldn't be loaded
                }
            }

            return result;
        }
    }
}
