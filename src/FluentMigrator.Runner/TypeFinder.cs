using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Helper class simplifying searching for types in assemblies.
    /// </summary>
    class TypeFinder
    {
        /// <summary>
        /// Enumerates all types in the specified assembly located in the specifying namespace and optionally in nested namespaces
        /// </summary>
        /// <param name="assembly">Assembly to search for types in.</param>
        /// <param name="namespace">Namespace to search types in. Set to null or empty string to search in all namespaces.</param>
        /// <param name="loadNestedNamespaces">Set to true to search for types located in nested namespaces of <paramref name="namespace"/>.
        /// This parameter is ignored if <paramref name="namespace"/> is null or empty string.
        /// </param>
        /// <returns>Enumerable list of all types found.</returns>
        public static IEnumerable<Type> FindTypes(Assembly assembly, string @namespace, bool loadNestedNamespaces)
        {
            IEnumerable<Type> types = assembly.GetExportedTypes();

            if (!string.IsNullOrEmpty(@namespace))
            {
                Func<Type, bool> shouldInclude = t => t.Namespace == @namespace;
                if (loadNestedNamespaces)
                {
                    string matchNested = @namespace + ".";
                    shouldInclude = t => t.Namespace == @namespace || t.Namespace.StartsWith(matchNested);
                }

                return types.Where(shouldInclude);
            }
            else
                return types;
        }
    }
}
