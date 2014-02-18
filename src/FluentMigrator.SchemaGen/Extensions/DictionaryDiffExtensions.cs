using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.SchemaGen.Extensions
{
    /// <summary>
    /// Dictionary extension methods to assist in finding differences
    /// </summary>
    public static class DictionaryDiffExtensions
    {
        /// <summary>
        /// Get values in <paramref name="dict1"/> that are not in <paramref name="dict2"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict1"></param>
        /// <param name="dict2"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetRemoved<T>(this IDictionary<string, T> dict1, IDictionary<string, T> dict2)
        {
            var keys1 = dict1.Keys.OrderBy(k => k);
            var keys2 = dict2.Keys.OrderBy(k => k);

            return from key in keys1.Except(keys2)
                   select dict1[key];
        }

        /// <summary>
        /// Get keys in <paramref name="dict1"/> that are not in <paramref name="dict2"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict1"></param>
        /// <param name="dict2"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetRemovedNames<T>(this IDictionary<string, T> dict1, IDictionary<string, T> dict2)
        {
            var keys1 = dict1.Keys.OrderBy(k => k);
            var keys2 = dict2.Keys.OrderBy(k => k);

            return from key in keys1.Except(keys2)
                   select key;
        }

        /// <summary>
        /// Get values not in <paramref name="dict1"/> that are in <paramref name="dict2"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict1"></param>
        /// <param name="dict2"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetAdded<T>(this IDictionary<string, T> dict1, IDictionary<string, T> dict2)
        {
            var keys1 = dict1.Keys.OrderBy(k => k);
            var keys2 = dict2.Keys.OrderBy(k => k);

            return from key in keys2.Except(keys1)
                   select dict2[key];
        }

        /// <summary>
        /// Get keys not in <paramref name="dict1"/> that are in <paramref name="dict2"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict1"></param>
        /// <param name="dict2"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAddedNames<T>(this IDictionary<string, T> dict1, IDictionary<string, T> dict2)
        {
            var keys1 = dict1.Keys.OrderBy(k => k);
            var keys2 = dict2.Keys.OrderBy(k => k);

            return from key in keys2.Except(keys1)
                   select key;
        }

        /// <summary>
        /// Get key indexed values in both <paramref name="dict1"/> and <paramref name="dict2"/> that are not equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict1"></param>
        /// <param name="dict2"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetUpdated<T>(this IDictionary<string, T> dict1, IDictionary<string, T> dict2)
            where T : IEquatable<T>
        {
            var keys1 = dict1.Keys.OrderBy(k => k);
            var keys2 = dict2.Keys.OrderBy(k => k);

            return from key in keys1.Intersect(keys2)
                   where !dict1[key].Equals(dict2[key])
                   select dict2[key];
        }

        /// <summary>
        /// Get keys of indexed values in both <paramref name="dict1"/> and <paramref name="dict2"/> that are not equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict1"></param>
        /// <param name="dict2"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetUpdatedNames<T>(this IDictionary<string, T> dict1, IDictionary<string, T> dict2)
            where T : IEquatable<T>
        {
            var keys1 = dict1.Keys.OrderBy(k => k);
            var keys2 = dict2.Keys.OrderBy(k => k);

            return from key in keys1.Intersect(keys2)
                   where !dict1[key].Equals(dict2[key])
                   select key;
        }

        ///// <summary>
        ///// Get key indexed values in both <paramref name="dict1"/> and <paramref name="dict2"/>.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="dict1"></param>
        ///// <param name="dict2"></param>
        ///// <returns></returns>
        //public static IEnumerable<T> GetRetained<T>(this IDictionary<string, T> dict1, IDictionary<string, T> dict2)
        //{
        //    var keys1 = dict1.Keys.OrderBy(k => k);
        //    var keys2 = dict2.Keys.OrderBy(k => k);

        //    return from key in keys1.Intersect(keys2) 
        //           select dict2[key];
        //}

        ///// <summary>
        ///// Get key indexed values in both <paramref name="dict1"/> and <paramref name="dict2"/>.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="dict1"></param>
        ///// <param name="dict2"></param>
        ///// <returns></returns>
        //public static IEnumerable<string> GetRetainedNames<T>(this IDictionary<string, T> dict1, IDictionary<string, T> dict2)
        //{
        //    var keys1 = dict1.Keys.OrderBy(k => k);
        //    var keys2 = dict2.Keys.OrderBy(k => k);

        //    return from key in keys1.Intersect(keys2)
        //           select key;
        //}

        /// <summary>
        /// Return a list of the names of objects having matching <paramref name="T"/> values.
        /// If values in <paramref name="dict1"/> (or <paramref name="dict2"/>) are non unique then it may report multiple renames for the same object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict1"></param>
        /// <param name="dict2"></param>
        /// <returns>Object name pairs of rename objects</returns>
        public static IEnumerable<KeyValuePair<string, string>> GetRenamed<T>(this IDictionary<string, T> dict1, IDictionary<string, T> dict2)
        {
            return from kvp1 in dict1
                from kvp2 in dict2
                where kvp1.Key != kvp2.Key && kvp1.Value.Equals(kvp2.Value)
                select new KeyValuePair<string, string>(kvp1.Key, kvp2.Key);
        }

    }
}
