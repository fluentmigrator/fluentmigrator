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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Base class that handles execution of scripts stored as embedded resource
    /// </summary>
    public abstract class ExecuteEmbeddedSqlScriptExpressionBase : ExecuteSqlScriptExpressionBase
    {
        /// <summary>
        /// Gets the fully qualified resource name and assembly
        /// </summary>
        /// <param name="resourceNames">The resource names where the <paramref name="sqlScriptNames"/> should be found</param>
        /// <param name="sqlScriptNames">The names of the SQL script resources to be found</param>
        /// <returns>the fully qualified resource name and assembly</returns>
        protected static (string name, Assembly assembly) GetQualifiedResourcePath(
            [NotNull] IReadOnlyCollection<(string name, Assembly assembly)> resourceNames,
            [NotNull, ItemNotNull] params string[] sqlScriptNames)
        {
            foreach (var sqlScriptName in sqlScriptNames)
            {
                var foundResources = FindResourceName(resourceNames, sqlScriptName).ToList();

                if (foundResources.Count > 1)
                {
                    var foundAssemblyNames = foundResources.Select(x => x.assembly.FullName).Distinct();
                    var foundResourceNames = foundResources.Select(x => x.name);
                    throw new InvalidOperationException(
$@"Could not find a unique resource named {sqlScriptName} in assemblies {string.Join(", ", foundAssemblyNames)}.
Possible candidates are:

{string.Join(Environment.NewLine + "\t", foundResourceNames)}
");
                }

                if (foundResources.Count == 1)
                {
                    return foundResources[index: 0];
                }
            }

            var assemblyNames = resourceNames.Select(x => x.assembly.FullName).Distinct();
            throw new InvalidOperationException(
                $"Could not find a resource with one of the following names {string.Join(",", sqlScriptNames)} in assemblies {string.Join(", ", assemblyNames)}");
        }

        /// <summary>
        /// Finds resources with the given name
        /// </summary>
        /// <param name="resourceNames">The resource names where the <paramref name="sqlScriptName"/> should be found</param>
        /// <param name="sqlScriptName">The name of the SQL script resource to be found</param>
        /// <returns>The found resources</returns>
        [NotNull]
        private static IReadOnlyList<(string name, Assembly assembly)> FindResourceName(
            [NotNull] IEnumerable<(string name, Assembly assembly)> resourceNames,
            [NotNull] string sqlScriptName)
        {
            //resource full name is in format `namespace.resourceName`
            var sqlScriptParts = Enumerable.Reverse(sqlScriptName.Split('.')).ToArray();

            bool IsNameMatch((string name, Assembly assembly) x) => Enumerable.Reverse(x.name.Split('.'))
                .Take(sqlScriptParts.Length)
                .SequenceEqual(sqlScriptParts, StringComparer.InvariantCultureIgnoreCase);

            return resourceNames.Where(IsNameMatch).ToList();
        }
    }
}
