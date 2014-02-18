using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.SchemaGen.Model;

namespace FluentMigrator.SchemaGen.Extensions
{
    public class ModelDiff<T>
        where T : ICodeComparable
    {
        private readonly string[] oldKeys;
        private readonly string[] newKeys;

        private IDictionary<string, T> oldObjs;
        private IDictionary<string, T> newObjs;

        public ModelDiff(IEnumerable<T> oldObjs, IEnumerable<T> newObjs)
        {
            if (oldObjs == null) throw new ArgumentNullException("oldObjs");
            if (newObjs == null) throw new ArgumentNullException("newObjs");

            this.oldObjs = oldObjs.ToDictionary(obj => obj.FQName);
            this.newObjs = newObjs.ToDictionary(obj => obj.FQName);

            oldKeys = this.oldObjs.Keys.OrderBy(k => k).ToArray();
            newKeys = this.newObjs.Keys.OrderBy(k => k).ToArray();
        }

        public T GetOldObject(string key)
        {
            return oldObjs[key];
        }

        public T GetNewObject(string key)
        {
            return newObjs[key];
        }

        /// <summary>
        /// Get old objects that are removed or updated.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetRemovedOrUpdated()
        {
            return from key in oldKeys
                   where !newObjs.ContainsKey(key) // Removed
                        ||
                        !oldObjs[key].CreateCode.Equals(newObjs[key].CreateCode)  // Updated
                        || newObjs[key].TypeChanged
                   select oldObjs[key];
        }

        /// <summary>
        /// Get new objects that are added or updated.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetAddedOrUpdated()
        {
            return from key in newKeys
                   where !oldObjs.ContainsKey(key) // Added
                        || !oldObjs[key].CreateCode.Equals(newObjs[key].CreateCode)  // Updated
                        || newObjs[key].TypeChanged
                   select newObjs[key];
        }

        public IEnumerable<T> GetRemoved()
        {
            return from key in oldKeys.Except(newKeys) select oldObjs[key];
        }

        public IEnumerable<T> GetUpdatedNew()
        {
            return from key in oldKeys.Intersect(newKeys)
                   where !oldObjs[key].CreateCode.Equals(newObjs[key].CreateCode)
                        || newObjs[key].TypeChanged
                   select newObjs[key];
        }

        public IEnumerable<T> GetUpdatedOld()
        {
            return from key in oldKeys.Intersect(newKeys)
                   where !oldObjs[key].CreateCode.Equals(newObjs[key].CreateCode)
                        || newObjs[key].TypeChanged
                   select oldObjs[key];
        }

        public IEnumerable<T> GetAdded()
        {
            return from key in newKeys.Except(oldKeys) select newObjs[key];
        }

        public IEnumerable<KeyValuePair<string, string>> GetRenamed()
        {
            return from kvp1 in oldObjs
                   from kvp2 in newObjs
                   where kvp1.Key != kvp2.Key && kvp1.Value.DefinitionCode.Equals(kvp2.Value.DefinitionCode)
                   select new KeyValuePair<string, string>(kvp1.Key, kvp2.Key);
        }
    }
}