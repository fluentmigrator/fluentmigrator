#region License

// Copyright (c) 2007-2024, Fluent Migrator Project
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

namespace FluentMigrator.Runner.Extensions
{
    /// <summary>
    /// Provides extension methods for working with <see cref="System.Type"/> objects.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Determines whether the specified <see cref="Type"/> is assignable to the type specified by the generic parameter <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to check against.</typeparam>
        /// <param name="type">The <see cref="Type"/> to evaluate.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Type"/> is assignable to <typeparamref name="T"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
        public static bool Is<T>(this Type type)
        {
            return typeof (T).IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Type"/> is a concrete class.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to evaluate.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Type"/> is a class and not abstract; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
        public static bool IsConcrete(this Type type)
        {
            return type.IsClass && !type.IsAbstract;
        }
    }
}
