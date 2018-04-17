#region License
// Copyright (c) 2007-2018, Fluent Migrator Project
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

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Interface to load migrations tagged with a profile name
    /// </summary>
    public interface IProfileLoader
    {
        /// <summary>
        /// Apply all loaded profiles
        /// </summary>
        void ApplyProfiles();

        /// <summary>
        /// Find all profile name tagged migrations in the given assembly collection
        /// </summary>
        /// <param name="assemblies">The assemblies to load the profile tagged migrations from</param>
        /// <param name="profile">The profile name to search the migrations for</param>
        /// <returns>The found migrations</returns>
        [Obsolete]
        IEnumerable<IMigration> FindProfilesIn(IAssemblyCollection assemblies, string profile);
    }
}
