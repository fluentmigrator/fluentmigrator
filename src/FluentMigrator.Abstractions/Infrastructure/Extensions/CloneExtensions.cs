#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

namespace FluentMigrator.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for cloning
    /// </summary>
    public static class CloneExtensions
    {
        /// <summary>
        /// Clones all <paramref name="items"/>
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="items">The items to clone</param>
        /// <returns>The cloned items</returns>
        public static IEnumerable<T> CloneAll<T>(this IEnumerable<T> items)
            where T : ICloneable
        {
            foreach (T item in items)
                yield return (T)item.Clone();
        }

        /// <summary>
        /// Clones all key/value pairs
        /// </summary>
        /// <param name="dict">The key value pairs to clone</param>
        /// <returns>The cloned key/value pairs</returns>
        public static Dictionary<string, object> Clone(this IEnumerable<KeyValuePair<string, object>> dict)
        {
            return dict
                .Select(x => Tuple.Create(x.Key, (x.Value as ICloneable)?.Clone() ?? x.Value))
                .ToDictionary(x => x.Item1, x => x.Item2);
        }

        /// <summary>
        /// Clones the values of <paramref name="dict"/> and stores them into <paramref name="target"/>
        /// </summary>
        /// <param name="dict">The key/value pairs to clone</param>
        /// <param name="target">The target dictionary to store the cloned key/value pairs</param>
        public static void CloneTo(this IEnumerable<KeyValuePair<string, object>> dict, IDictionary<string, object> target)
        {
            var clonedItems = dict
                .Select(x => Tuple.Create(x.Key, (x.Value as ICloneable)?.Clone() ?? x.Value));
            foreach (var clonedItem in clonedItems)
            {
                target[clonedItem.Item1] = clonedItem.Item2;
            }
        }
    }
}
