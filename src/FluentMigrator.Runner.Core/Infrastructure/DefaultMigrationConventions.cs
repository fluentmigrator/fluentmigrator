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
using System.Linq;
using System.Reflection;

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.VersionTableInfo;

namespace FluentMigrator.Runner.Infrastructure
{
    /// <summary>
    /// Provides default conventions for migration runner.
    /// </summary>
    public class DefaultMigrationRunnerConventions : IMigrationRunnerConventions
    {
        private DefaultMigrationRunnerConventions()
        {
        }

        /// <inheritdoc />
        public static DefaultMigrationRunnerConventions Instance { get; } = new DefaultMigrationRunnerConventions();

        /// <inheritdoc />
        public Func<Type, bool> TypeIsMigration => TypeIsMigrationImpl;
        /// <inheritdoc />
        public Func<Type, bool> TypeIsProfile => TypeIsProfileImpl;
        /// <inheritdoc />
        public Func<Type, MigrationStage?> GetMaintenanceStage => GetMaintenanceStageImpl;
        /// <inheritdoc />
        public Func<Type, bool> TypeIsVersionTableMetaData => TypeIsVersionTableMetaDataImpl;

        /// <inheritdoc />
        [Obsolete]
        public Func<Type, IMigrationInfo> GetMigrationInfo => GetMigrationInfoForImpl;

        /// <inheritdoc />
        public Func<IMigration, IMigrationInfo> GetMigrationInfoForMigration => GetMigrationInfoForMigrationImpl;

        /// <inheritdoc />
        public Func<Type, bool> TypeHasTags => TypeHasTagsImpl;
        /// <inheritdoc />
        public Func<Type, IEnumerable<string>, bool> TypeHasMatchingTags => TypeHasMatchingTagsImpl;

        private static bool TypeIsMigrationImpl(Type type)
        {
            return typeof(IMigration).IsAssignableFrom(type) && type.GetCustomAttributes<MigrationAttribute>().Any();
        }

        private static MigrationStage? GetMaintenanceStageImpl(Type type)
        {
            if (!typeof(IMigration).IsAssignableFrom(type))
                return null;

            var attribute = type.GetCustomAttribute<MaintenanceAttribute>();
            return attribute?.Stage;
        }

        private static bool TypeIsProfileImpl(Type type)
        {
            return typeof(IMigration).IsAssignableFrom(type) && type.GetCustomAttributes<ProfileAttribute>().Any();
        }

        private static bool TypeIsVersionTableMetaDataImpl(Type type)
        {
            return typeof(IVersionTableMetaData).IsAssignableFrom(type) && type.GetCustomAttributes<VersionTableMetaDataAttribute>().Any();
        }

        private static IMigrationInfo GetMigrationInfoForMigrationImpl(IMigration migration)
        {
            var migrationType = migration.GetType();
            var migrationAttribute = migrationType.GetCustomAttribute<MigrationAttribute>();
            var migrationInfo = new MigrationInfo(migrationAttribute.Version, migrationAttribute.Description, migrationAttribute.TransactionBehavior, migrationAttribute.BreakingChange, () => migration, migrationAttribute.VersionAsString);

            foreach (var traitAttribute in migrationType.GetCustomAttributes<MigrationTraitAttribute>(true))
                migrationInfo.AddTrait(traitAttribute.Name, traitAttribute.Value);

            return migrationInfo;
        }

        private IMigrationInfo GetMigrationInfoForImpl(Type migrationType)
        {
            var migration = (IMigration) Activator.CreateInstance(migrationType);
            return GetMigrationInfoForMigration(migration);
        }

        private static bool TypeHasTagsImpl(Type type)
        {
            return GetInheritedCustomAttributes<TagsAttribute>(type).Any();
        }

        private static IEnumerable<T> GetInheritedCustomAttributes<T>(Type type)
        {
            var attributeType = typeof(T);

            return type
                .GetCustomAttributes(attributeType, true)
                .Union(
                    type.GetInterfaces()
                        .SelectMany(interfaceType => interfaceType.GetCustomAttributes(attributeType, true)))
                .Distinct()
                .Cast<T>();
        }

        private static bool TypeHasMatchingTagsImpl(Type type, IEnumerable<string> tagsToMatch)
        {
            var tags = GetInheritedCustomAttributes<TagsAttribute>(type).ToList();
            var matchTagsList = tagsToMatch.ToList();

            if (tags.Count != 0 && matchTagsList.Count == 0)
                return false;

            var tagNamesForAllBehavior = tags.Where(t => t.Behavior == TagBehavior.RequireAll).SelectMany(t => t.TagNames).ToArray();
            if (tagNamesForAllBehavior.Any() && matchTagsList.All(t => tagNamesForAllBehavior.Any(t.Equals)))
            {
                return true;
            }

            var tagNamesForAnyBehavior = tags.Where(t => t.Behavior == TagBehavior.RequireAny).SelectMany(t => t.TagNames).ToArray();
            if (tagNamesForAnyBehavior.Any() && matchTagsList.Any(t => tagNamesForAnyBehavior.Any(t.Equals)))
            {
                return true;
            }

            return false;
        }
    }
}
