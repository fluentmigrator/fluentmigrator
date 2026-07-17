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
using System.Collections.Generic;

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// The default <see cref="IMigrationInfo"/> implementation for migrations with the <see cref="MigrationAttribute"/>
    /// </summary>
    public class MigrationInfo : IMigrationInfo
    {
        private readonly Dictionary<string, object> _traits = new Dictionary<string, object>();
        private readonly Lazy<IMigration> _lazyMigration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationInfo"/> class.
        /// </summary>
        /// <param name="version">The migration version</param>
        /// <param name="transactionBehavior">The desired transaction behavior</param>
        /// <param name="migration">The underlying migration</param>
        /// <param name="versionAsString"></param>
        public MigrationInfo(long version, TransactionBehavior transactionBehavior, IMigration migration, string versionAsString = null)
            : this(version, null, transactionBehavior, false, () => migration, versionAsString ?? version.ToString())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationInfo"/> class.
        /// </summary>
        /// <param name="version">The migration version</param>
        /// <param name="transactionBehavior">The desired transaction behavior</param>
        /// <param name="isBreakingChange">Indicates whether the migration is a breaking change</param>
        /// <param name="migration">The underlying migration</param>
        /// <param name="versionAsString">The human-readable version</param>
        public MigrationInfo(long version, TransactionBehavior transactionBehavior, bool isBreakingChange, IMigration migration, string versionAsString = null)
            : this(version, null, transactionBehavior, isBreakingChange, () => migration, versionAsString ?? version.ToString())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationInfo"/> class.
        /// </summary>
        /// <param name="version">The migration version</param>
        /// <param name="description">The migration description</param>
        /// <param name="transactionBehavior">The desired transaction behavior</param>
        /// <param name="isBreakingChange">Indicates whether the migration is a breaking change</param>
        /// <param name="migrationFunc">A function to get the <see cref="IMigration"/> instance</param>
        /// <param name="versionAsString">The human-readable version</param>
        public MigrationInfo(
            long version,
            string description,
            TransactionBehavior transactionBehavior,
            bool isBreakingChange,
            Func<IMigration> migrationFunc,
            string versionAsString)
        {
            if (migrationFunc == null) throw new ArgumentNullException(nameof(migrationFunc));

            Version = version;
            Description = description;
            TransactionBehavior = transactionBehavior;
            IsBreakingChange = isBreakingChange;
            _lazyMigration = new Lazy<IMigration>(migrationFunc);
            VersionAsString = versionAsString;
        }

        /// <inheritdoc />
        public long Version { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public TransactionBehavior TransactionBehavior { get; }

        /// <inheritdoc />
        public IMigration Migration => _lazyMigration.Value;

        /// <inheritdoc />
        public bool IsBreakingChange { get; }

        public string VersionAsString { get; }

        /// <inheritdoc />
        public object Trait(string name)
        {
            return _traits.ContainsKey(name) ? _traits[name] : null;
        }

        /// <inheritdoc />
        public bool HasTrait(string name)
        {
            return _traits.ContainsKey(name);
        }

        /// <inheritdoc />
        public string GetName()
        {
            return string.Format("{0}: {1}", VersionAsString, Migration.GetType().Name);
        }

        /// <summary>
        /// Manually adds a trait to the migration
        /// </summary>
        /// <param name="name">The trait name</param>
        /// <param name="value">The trait value</param>
        public void AddTrait(string name, object value)
        {
            _traits.Add(name, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("MigrationType: {0}, TransactionBehavior: {1}", Migration.GetType(),
                                 TransactionBehavior);
        }
    }
}
