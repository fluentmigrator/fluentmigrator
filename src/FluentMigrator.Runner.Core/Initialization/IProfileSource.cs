#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Gets all types
    /// </summary>
    public interface IProfileSource
    {
        /// <summary>
        /// Retrieves a collection of migrations associated with the specified profile.
        /// </summary>
        /// <param name="profile">The name of the profile to filter migrations. Can be <c>null</c> to retrieve all migrations.</param>
        /// <returns>A collection of migrations that match the specified profile.</returns>
        [NotNull, ItemNotNull]
        IEnumerable<IMigration> GetProfiles([CanBeNull] string profile);
    }
}
