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

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// This adapter wraps a migration into a MigrationInfo instance, used to keep <see cref="IMigration"/> backwards compatible with new <see cref="IMigrationInfo"/>.
    /// </summary>
    public class NonAttributedMigrationToMigrationInfoAdapter : IMigrationInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NonAttributedMigrationToMigrationInfoAdapter"/> class.
        /// </summary>
        /// <param name="migration">The underlying migration</param>
        public NonAttributedMigrationToMigrationInfoAdapter(IMigration migration)
            : this(migration, TransactionBehavior.Default)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="NonAttributedMigrationToMigrationInfoAdapter"/> class.
        /// </summary>
        /// <param name="migration">The underlying migration</param>
        /// <param name="transactionBehavior">The desired transaction behavior</param>
        public NonAttributedMigrationToMigrationInfoAdapter(IMigration migration, TransactionBehavior transactionBehavior)
        {
            Migration = migration ?? throw new ArgumentNullException(nameof(migration));
            TransactionBehavior = transactionBehavior;
        }

        /// <inheritdoc />
        public string Description { get; } = string.Empty;

        /// <inheritdoc />
        public long Version => -1;

        /// <inheritdoc />
        public TransactionBehavior TransactionBehavior { get; private set;}

        /// <inheritdoc />
        public IMigration Migration { get; private set; }

        /// <inheritdoc />
        public bool IsBreakingChange => false;

        /// <inheritdoc />
        public object Trait(string name)
        {
            return null;
        }

        /// <inheritdoc />
        public bool HasTrait(string name)
        {
            return false;
        }

        /// <inheritdoc />
        public string GetName()
        {
            return string.Format("{0}", Migration.GetType().Name);
        }
    }
}
