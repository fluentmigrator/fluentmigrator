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
using System.Diagnostics;
using System.Linq;

using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Infrastructure;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// The default implementation of the <see cref="IProfileLoader"/>
    /// </summary>
    public class ProfileLoader : IProfileLoader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileLoader"/> class.
        /// </summary>
        /// <param name="options">The runner options</param>
        /// <param name="source">The profile source</param>
        /// <param name="serviceProvider">The service provider</param>
        [Obsolete("Use the other constructor")]
        public ProfileLoader(
            [NotNull] IOptions<RunnerOptions> options,
            [NotNull] IProfileSource source,
            [NotNull] IServiceProvider serviceProvider)
        {
            Profiles = source.GetProfiles(options.Value.Profile).ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileLoader"/> class.
        /// </summary>
        /// <param name="options">The runner options</param>
        /// <param name="source">The profile source</param>
        public ProfileLoader(
            [NotNull] IOptions<RunnerOptions> options,
            [NotNull] IProfileSource source)
        {
            Profiles = source.GetProfiles(options.Value.Profile).ToList();
        }

        /// <summary>
        /// Gets all found profiles
        /// </summary>
        public IEnumerable<IMigration> Profiles { get; }

        /// <inheritdoc />
        public void ApplyProfiles(IMigrationRunner runner)
        {
            // Run Profile if applicable
            foreach (var profile in Profiles)
            {
                runner.Up(profile);
            }
        }
    }
}
