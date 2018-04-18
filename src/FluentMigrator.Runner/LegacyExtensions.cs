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
