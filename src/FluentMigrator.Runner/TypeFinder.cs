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

using JetBrains.Annotations;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Advanced searching and filtration of types collections.
    /// </summary>
    internal static class TypeFinder
    {
        /// <summary>
        /// Searches for types located in the specifying namespace and optionally in its nested namespaces.
        /// </summary>
        /// <param name="types">Source types collection to search in.</param>
        /// <param name="namespace">Namespace to search types in. Set to null or empty string to search in all namespaces.</param>
        /// <param name="loadNestedNamespaces">Set to true to search for types located in nested namespaces of <paramref name="namespace"/>.
        /// This parameter is ignored if <paramref name="namespace"/> is null or empty string.
        /// </param>
        /// <returns>Collection of types matching specified criteria.</returns>
        public static IEnumerable<Type> FilterByNamespace([NotNull, ItemNotNull] this IEnumerable<Type> types, [CanBeNull] string @namespace, bool loadNestedNamespaces)
        {
            if (!string.IsNullOrEmpty(@namespace))
            {
                Func<Type, bool> shouldInclude = t => t.Namespace == @namespace;
                if (loadNestedNamespaces)
                {
                    string matchNested = @namespace + ".";
                    shouldInclude = t => t.Namespace != null && (t.Namespace == @namespace || t.Namespace.StartsWith(matchNested));
                }

                return types.Where(shouldInclude);
            }

            return types;
        }

        /// <summary>
        /// Test if the type is in the given namespace
        /// </summary>
        /// <remarks>
        /// All types will be accepted when no namespace is given.
        /// </remarks>
        /// <param name="type">The type to test</param>
        /// <param name="namespace">The namespace</param>
        /// <param name="loadNestedNamespaces">Indicates whether nested namespaces should be accepted</param>
        /// <returns><c>true</c> when the type is in the given namespace</returns>
        public static bool IsInNamespace(
            [NotNull] this Type type,
            [CanBeNull] string @namespace,
            bool loadNestedNamespaces)
        {
            if (string.IsNullOrEmpty(@namespace))
                return true;

            if (type.Namespace == null)
                return false;

            if (type.Namespace == @namespace)
                return true;

            if (!loadNestedNamespaces)
                return false;

            var matchNested = @namespace + ".";
            return type.Namespace.StartsWith(matchNested, StringComparison.Ordinal);
        }
    }
}
