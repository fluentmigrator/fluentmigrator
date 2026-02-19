#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when unapplied migrations have version numbers 
    /// less than the greatest version number of applied migrations.
    /// </summary>
    /// <remarks>
    /// This exception is typically used to indicate an invalid version order in migration sequences.
    /// It provides details about the invalid migrations and their associated version numbers.
    /// </remarks>
    /// <seealso cref="RunnerException"/>
    public class VersionOrderInvalidException : RunnerException
    {
        private IReadOnlyCollection<KeyValuePair<long, IMigrationInfo>> _invalidMigrations;

        /// <summary>
        /// Gets or sets the collection of invalid migrations that caused the exception.
        /// </summary>
        /// <value>
        /// A collection of key-value pairs where the key represents the version number of the migration,
        /// and the value contains the associated migration information.
        /// </value>
        /// <remarks>
        /// Invalid migrations are those with version numbers less than the greatest version number
        /// of already applied migrations.
        /// </remarks>
        public IEnumerable<KeyValuePair<long, IMigrationInfo>> InvalidMigrations
        {
            get => _invalidMigrations;
            set => _invalidMigrations = value.ToList();
        }

        /// <summary>
        /// Gets a collection of version numbers associated with unapplied migrations that 
        /// have version numbers less than the greatest version number of applied migrations.
        /// </summary>
        /// <value>
        /// A collection of version numbers representing the invalid migrations.
        /// </value>
        /// <remarks>
        /// This property provides a simplified view of the invalid migrations by exposing only their version numbers.
        /// It is derived from the <see cref="InvalidMigrations"/> property.
        /// </remarks>
        public IEnumerable<long> InvalidVersions => _invalidMigrations.Select(x => x.Key);

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionOrderInvalidException"/> class with the specified invalid migrations.
        /// </summary>
        /// <param name="invalidMigrations">
        /// A collection of key-value pairs where the key represents the version number and the value represents the migration information
        /// for migrations that are considered invalid due to their version order.
        /// </param>
        /// <remarks>
        /// This constructor is used to create an exception that provides details about unapplied migrations with version numbers
        /// less than the greatest version number of applied migrations.
        /// </remarks>
        public VersionOrderInvalidException(IEnumerable<KeyValuePair<long, IMigrationInfo>> invalidMigrations)
        {
            _invalidMigrations = invalidMigrations.ToList();
        }

        /// <inheritdoc />
        public override string Message
        {
            get
            {
                var result = "Unapplied migrations have version numbers that are less than the greatest version number of applied migrations:";

                foreach (var pair in InvalidMigrations)
                {
                    result += $"{Environment.NewLine}{pair.Key} - {pair.Value.Migration.GetType().Name}";
                }

                return result;
            }
        }
    }
}
