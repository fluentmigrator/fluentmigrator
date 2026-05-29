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
using System.Collections.Generic;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Defines methods for determining whether a migration type has tags
    /// and whether those tags match a given set of tag names.
    /// </summary>
    /// <remarks>
    /// This interface replaces the <see cref="IMigrationRunnerConventions.TypeHasTags"/>
    /// and <see cref="IMigrationRunnerConventions.TypeHasMatchingTags"/> delegate properties
    /// with proper methods, allowing parameter-level attributes (such as
    /// <c>DynamicallyAccessedMembersAttribute</c>) to be preserved for AOT/trimming scenarios.
    /// </remarks>
    public interface IMigrationRunnerTagConventions
    {
        /// <summary>
        /// Determines whether the specified type is associated with tags.
        /// </summary>
        /// <param name="type">The type to check for tag attributes.</param>
        /// <returns><c>true</c> if the type has tags; otherwise, <c>false</c>.</returns>
        bool TypeHasTags(
#if NET
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            Type type);

        /// <summary>
        /// Determines whether a given type has tags that match a specified set of tags.
        /// </summary>
        /// <param name="type">The type to check for matching tag attributes.</param>
        /// <param name="tagsToMatch">The tags to match against.</param>
        /// <returns><c>true</c> if the type has matching tags; otherwise, <c>false</c>.</returns>
        bool TypeHasMatchingTags(
#if NET
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            Type type,
            IEnumerable<string> tagsToMatch);
    }
}
