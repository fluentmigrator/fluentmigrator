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

using System.Collections.Generic;

using FluentMigrator.Infrastructure;

using JetBrains.Annotations;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Provides functionality to load migration information from a source.
    /// </summary>
    public interface IMigrationInformationLoader
    {
        /// <summary>
        /// Loads and retrieves a sorted list of migrations.
        /// </summary>
        /// <returns>
        /// A <see cref="SortedList{TKey, TValue}"/> where the key is the migration version as a <see cref="long"/>,
        /// and the value is the corresponding <see cref="IMigrationInfo"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is responsible for discovering and organizing migrations in a sorted order based on their version.
        /// </remarks>
        [NotNull]
        SortedList<long, IMigrationInfo> LoadMigrations();
    }
}
