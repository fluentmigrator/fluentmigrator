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

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// This adapter wraps a migration into a MigrationInfo instance, used to keep IMigration backwards compatible with new IMigrationInfo.
    /// </summary>
    public class NonAttributedMigrationToMigrationInfoAdapter : IMigrationInfo
    {
        public NonAttributedMigrationToMigrationInfoAdapter(IMigration migration) : this(migration, TransactionBehavior.Default)
        {}

        public NonAttributedMigrationToMigrationInfoAdapter(IMigration migration, TransactionBehavior transactionBehavior)
        {
             if (migration == null) throw new ArgumentNullException("migration");
            Migration = migration;
            TransactionBehavior = transactionBehavior;
        }

        public string Description { get; private set; }

        public long Version
        {
            get { return -1; }
        }

        public TransactionBehavior TransactionBehavior { get; private set;}

        public IMigration Migration { get; private set; }

        public object Trait(string name)
        {
            return null;
        }

        public bool HasTrait(string name)
        {
            return false;
        }

        public string GetName()
        {
            return string.Format("{0}", Migration.GetType().Name);
        }
    }
}