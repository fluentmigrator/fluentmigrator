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

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// The default implementation of the <see cref="IProfileLoader"/>
    /// </summary>
    public class ProfileLoader : IProfileLoader
    {
        [Obsolete]
        private readonly IMigrationRunner _runner;

        private readonly IMigrationRunnerConventions _conventions;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileLoader"/> class.
        /// </summary>
        /// <param name="runnerContext">The migration runner context</param>
        /// <param name="conventions">The migration runner conventions</param>
        /// <param name="migrations">The migrations</param>
        /// <param name="serviceProvider">The service provider</param>
        public ProfileLoader(IRunnerContext runnerContext, IMigrationRunnerConventions conventions, IEnumerable<IMigration> migrations, IServiceProvider serviceProvider)
        {
            _conventions = conventions;
            _serviceProvider = serviceProvider;
            Profiles = FindProfilesIn(migrations, runnerContext.Profile).ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileLoader"/> class.
        /// </summary>
        /// <param name="runnerContext">The migration runner context</param>
        /// <param name="runner">The migration runner</param>
        /// <param name="conventions">The migration runner conventions</param>
        [Obsolete]
        public ProfileLoader(IRunnerContext runnerContext, IMigrationRunner runner, IMigrationRunnerConventions conventions)
        {
            _runner = runner;
            _conventions = conventions;
            Profiles = FindProfilesIn(runner.MigrationAssemblies, runnerContext.Profile).ToList();
        }

        /// <inheritdoc />
        [Obsolete]
        public IEnumerable<IMigration> FindProfilesIn(IAssemblyCollection assemblies, string profile)
        {
            if (string.IsNullOrEmpty(profile))
                yield break;

            IEnumerable<Type> matchedTypes = assemblies.GetExportedTypes()
                .Where(t => _conventions.TypeIsProfile(t) && t.GetOneAttribute<ProfileAttribute>().ProfileName.ToLower() == profile.ToLower())
                .OrderBy(x => x.Name);

            foreach (var type in matchedTypes)
            {
                if (type.FullName == null)
                    continue;
                yield return type.Assembly.CreateInstance(type.FullName) as IMigration;
            }
        }

        /// <summary>
        /// Gets all found profiles
        /// </summary>
        public IEnumerable<IMigration> Profiles { get; }

        /// <inheritdoc />
        [Obsolete]
        public bool SupportsParameterlessApplyProfile => _runner != null;

        /// <inheritdoc />
        [Obsolete]
        public void ApplyProfiles()
        {
            if (_runner == null)
            {
                Debug.Assert(_serviceProvider != null, "_serviceProvider != null");
                var runner = _serviceProvider.GetRequiredService<IMigrationRunner>();
                ApplyProfiles(runner);
            }
            else
            {
                ApplyProfiles(_runner);
            }
        }

        /// <inheritdoc />
        public void ApplyProfiles(IMigrationRunner runner)
        {
            // Run Profile if applicable
            foreach (var profile in Profiles)
            {
                runner.Up(profile);
            }
        }

        private IEnumerable<IMigration> FindProfilesIn(IEnumerable<IMigration> migrations, string profile)
        {
            if (string.IsNullOrEmpty(profile))
                return Enumerable.Empty<IMigration>();

            var matchedMigrations =
                from migration in migrations
                let type = migration.GetType()
                where _conventions.TypeIsProfile(type) && string.Equals(type.GetOneAttribute<ProfileAttribute>().ProfileName, profile, StringComparison.OrdinalIgnoreCase)
                orderby type.Name
                select migration;

            return matchedMigrations;
        }
    }
}
