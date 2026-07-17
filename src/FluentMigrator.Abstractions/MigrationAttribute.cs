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

using JetBrains.Annotations;

namespace FluentMigrator
{
    /// <summary>
    /// Attribute for a migration
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [MeansImplicitUse]
    public class MigrationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationAttribute"/> class.
        /// </summary>
        /// <param name="version">The migration version</param>
        /// <param name="description">The migration description</param>
        public MigrationAttribute(long version, string description)
            : this(version, TransactionBehavior.Default, description)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationAttribute"/> class.
        /// </summary>
        /// <param name="version">The migration version</param>
        /// <param name="transactionBehavior">The desired transaction behavior</param>
        /// <param name="description">The migration description</param>
        public MigrationAttribute(long version, TransactionBehavior transactionBehavior = TransactionBehavior.Default, string description = null)
        {
            Version = version;
            TransactionBehavior = transactionBehavior;
            Description = description;
        }

        /// <summary>
        /// Gets the migration version
        /// </summary>
        public long Version { get; }

        /// <summary>
        /// Gets the human-readable version
        /// </summary>
        public virtual string VersionAsString => this.Version.ToString();

        /// <summary>
        /// Gets the desired transaction behavior
        /// </summary>
        public TransactionBehavior TransactionBehavior { get; }

        /// <summary>
        /// Gets the description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the transaction is a breaking change
        /// </summary>
        public bool BreakingChange { get; set; }
    }
}
