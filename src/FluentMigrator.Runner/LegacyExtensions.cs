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
using System.Linq;

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;

using JetBrains.Annotations;

namespace FluentMigrator.Runner
{
    [Obsolete]
    internal static class LegacyExtensions
    {
        [Obsolete]
        public static IVersionTableMetaData GetVersionTableMetaData(
            [CanBeNull] this IAssemblyCollection assemblies,
            [NotNull] IConventionSet conventionSet,
            [NotNull] IMigrationRunnerConventions runnerConventions,
            [NotNull] IRunnerContext runnerContext)
        {
            if (assemblies == null)
            {
                var result = new DefaultVersionTableMetaData();
                conventionSet.SchemaConvention?.Apply(result);
                return result;
            }

            var matchedType = assemblies.GetExportedTypes()
                .FilterByNamespace(runnerContext.Namespace, runnerContext.NestedNamespaces)
                .FirstOrDefault(t => runnerConventions.TypeIsVersionTableMetaData(t));

            if (matchedType == null)
            {
                var result = new DefaultVersionTableMetaData();
                conventionSet.SchemaConvention?.Apply(result);
                return result;
            }

            return (IVersionTableMetaData) Activator.CreateInstance(matchedType);
        }

        [Obsolete]
        public static IMigrationRunnerConventions GetMigrationRunnerConventions(
            [CanBeNull] this IAssemblyCollection assemblies)
        {
            if (assemblies == null)
                return new MigrationRunnerConventions();

            var matchedType = assemblies
                .GetExportedTypes()
                .FirstOrDefault(t => typeof(IMigrationRunnerConventions).IsAssignableFrom(t));

            if (matchedType != null)
            {
                return (IMigrationRunnerConventions) Activator.CreateInstance(matchedType);
            }

            return new MigrationRunnerConventions();
        }
    }
}
