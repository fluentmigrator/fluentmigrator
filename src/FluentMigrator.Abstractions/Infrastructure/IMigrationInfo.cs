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

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// Interface for migration information
    /// </summary>
    public interface IMigrationInfo
    {
        /// <summary>
        /// Gets the migration version
        /// </summary>
        long Version { get; }

        /// <summary>
        /// Gets the migration description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the desired transaction behavior
        /// </summary>
        TransactionBehavior TransactionBehavior { get; }

        /// <summary>
        /// Gets the migration
        /// </summary>
        IMigration Migration { get; }

        /// <summary>
        /// Gets a value indicating whether the migration is a breaking change
        /// </summary>
        bool IsBreakingChange { get; }

        /// <summary>
        /// Gets the trait object with the given name
        /// </summary>
        /// <param name="name">The trait name</param>
        /// <returns>The object associated with the given <paramref name="name"/></returns>
        object Trait(string name);

        /// <summary>
        /// Returns a value indicating whether a given trait was specified
        /// </summary>
        /// <param name="name">The trait name</param>
        /// <returns><c>true</c> when the trait was specified</returns>
        bool HasTrait(string name);

        /// <summary>
        /// Gets the migration name
        /// </summary>
        /// <returns></returns>
        string GetName();
    }
}
