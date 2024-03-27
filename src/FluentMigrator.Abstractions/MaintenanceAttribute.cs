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

namespace FluentMigrator
{
    /// <summary>
    /// Annotates migrations that should always be executed at a specified stage.
    /// </summary>
    /// <remarks>
    /// Migration annotated with <see cref="MaintenanceAttribute" /> will be always executed
    /// when migrating the database to the latest version. The execution stage in which it would
    /// be executed is defined by <see cref="Stage" />. The transaction behavior can also be defined
    /// with the <see cref="TransactionBehavior"/>, which if not specified defaults to the default
    /// transaction behavior.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MaintenanceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaintenanceAttribute"/> class
        /// </summary>
        /// <param name="stage">The migration stage when the migration should be applied</param>
        public MaintenanceAttribute(MigrationStage stage) : this(stage, TransactionBehavior.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaintenanceAttribute"/> class.
        /// </summary>
        /// <param name="stage">The migration stage when the migration should be applied</param>
        /// <param name="transactionBehavior">The desired transaction behavior</param>
        public MaintenanceAttribute(MigrationStage stage, TransactionBehavior transactionBehavior)
        {
            Stage = stage;
            TransactionBehavior = transactionBehavior;
        }

        /// <summary>
        /// Gets the migration stage when the migration should be applied
        /// </summary>
        public MigrationStage Stage { get; }

        /// <summary>
        /// Gets the desired transaction behavior
        /// </summary>
        public TransactionBehavior TransactionBehavior { get; }
    }
}
