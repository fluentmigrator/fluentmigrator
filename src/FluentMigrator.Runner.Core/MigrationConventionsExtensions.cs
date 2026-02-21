#region License
// Copyright (c) 2019, Fluent Migrator Project
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

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IMigrationRunnerConventions"/>.
    /// </summary>
    public static class MigrationConventionsExtensions
    {
        /// <summary>
        /// Returns a value indicating whether the type matches the given tag list.
        /// </summary>
        /// <param name="conventions">The conventions to use.</param>
        /// <param name="type">The type to validate.</param>
        /// <param name="tagsList">The list of tags to check against.</param>
        /// <param name="includeUntagged">Allow untagged entries.</param>
        /// <returns><see langword="true"/> when the requested tags match the tags attached to the type.</returns>
        public static bool HasRequestedTags(
            this IMigrationRunnerConventions conventions,
#if NET
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            Type type,
            string[] tagsList,
            bool includeUntagged)
        {
            if (tagsList.Length > 0)
            {
                if (includeUntagged)
                {
                    return conventions.TypeHasMatchingTags(type, tagsList)
                     || !conventions.TypeHasTags(type);
                }

                return conventions.TypeHasMatchingTags(type, tagsList);
            }

            return !conventions.TypeHasTags(type);
        }
    }
}
