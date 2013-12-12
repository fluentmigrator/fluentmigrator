#region License

// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.Reflection;
using FluentMigrator.Infrastructure.Extensions;

namespace FluentMigrator.Infrastructure
{
    public class MigrationInfo : IMigrationInfo
    {
        private readonly Dictionary<string, object> _traits = new Dictionary<string, object>();

        public MigrationInfo(long version, TransactionBehavior transactionBehavior, IMigration migration)
        {
            if (migration == null) throw new ArgumentNullException("migration");

            Version = version;
            TransactionBehavior = transactionBehavior;
            Migration = migration;
        }

        public long Version { get; private set; }
        public TransactionBehavior TransactionBehavior { get; private set; }
        public IMigration Migration { get; private set; }

        public object Trait(string name)
        {
            return _traits.ContainsKey(name) ? _traits[name] : null;
        }

        public bool HasTrait(string name)
        {
            return _traits.ContainsKey(name);
        }

        public string GetName()
        {
            var type = Migration.GetType();
            if (type.HasAttribute<MigrationDescriptionAttribute>())
            {
                var da = type.GetOneAttribute<MigrationDescriptionAttribute>();
                var resourceType = da.ResourceType;
                var name = string.IsNullOrEmpty(da.Name)
                    ? string.Format("V{0}", Version)
                    : da.Name;
                var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                //TODO: throw exception when property is null or invalid return type??
                if (property != null && property.PropertyType == typeof(string))
                {
                    return string.Format("{0}: {1} - {2}", Version, type.Name, property.GetValue(null, null));
                }
            }
            return string.Format("{0}: {1}", Version, type.Name);
        }

        public void AddTrait(string name, object value)
        {
            _traits.Add(name, value);
        }

        public override string ToString()
        {
            return string.Format("MigrationType: {0}, TransactionBehavior: {1}", Migration.GetType(),
                                 TransactionBehavior);
        }
    }
}