using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Advanced searching and filtration of types collections.
    /// </summary>
    static class TypeFinder
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
        public static IEnumerable<Type> FilterByNamespace(this IEnumerable<Type> types, string @namespace, bool loadNestedNamespaces)
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
            else
                return types;
        }
    }
}
