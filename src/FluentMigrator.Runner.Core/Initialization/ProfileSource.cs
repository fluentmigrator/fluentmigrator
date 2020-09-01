#region License
// Copyright (c) 2018, FluentMigrator Project
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
using System.Reflection;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// The default implementation of <see cref="IProfileSource"/>
    /// </summary>
    public class ProfileSource : IProfileSource
    {
        [NotNull]
        private readonly IFilteringMigrationSource _source;

        [NotNull]
        private readonly IMigrationRunnerConventions _conventions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileSource"/> class.
        /// </summary>
        /// <param name="source">The assembly source</param>
        /// <param name="conventions">The migration runner conventions</param>
        public ProfileSource(
            [NotNull] IFilteringMigrationSource source,
            [NotNull] IMigrationRunnerConventions conventions)
        {
            _source = source;
            _conventions = conventions;
        }

        /// <inheritdoc />
        public IEnumerable<IMigration> GetProfiles(string profile) =>
            _source.GetMigrations(t => IsSelectedProfile(t, profile));

        private bool IsSelectedProfile(Type type, string profile)
        {
            if (!_conventions.TypeIsProfile(type))
                return false;
            var profileAttribute = type.GetCustomAttribute<ProfileAttribute>();
            return !string.IsNullOrEmpty(profile) && string.Equals(profileAttribute.ProfileName, profile);
        }
    }
}
