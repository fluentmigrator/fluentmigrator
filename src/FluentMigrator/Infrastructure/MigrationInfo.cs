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
using FluentMigrator.Helpers;

namespace FluentMigrator.Infrastructure
{
    public class MigrationInfo : IMigrationInfo
    {
        private readonly Dictionary<string, object> _traits = new Dictionary<string, object>();
        private LazyLoader<IMigration> _lazyMigration;

        public MigrationInfo(long version, TransactionBehavior transactionBehavior, IMigration migration)
            : this(version, null, transactionBehavior, () => migration)
        {
        }

        public MigrationInfo(long version, string description, TransactionBehavior transactionBehavior, Func<IMigration> migrationFunc)
        {
            if (migrationFunc == null) throw new ArgumentNullException("migrationFunc");

            Version = version;
            Description = description;
            TransactionBehavior = transactionBehavior;
            _lazyMigration = new LazyLoader<IMigration>(migrationFunc);
        }

        public long Version { get; private set; }
        public string Description { get; private set; }
        public TransactionBehavior TransactionBehavior { get; private set; }
        public IMigration Migration
        {
            get
            {
                return _lazyMigration.Value;
            }
        }

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
            return string.Format("{0}: {1}", Version, Migration.GetType().Name);
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