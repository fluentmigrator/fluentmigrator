#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Runner.VersionTableInfo;

namespace FluentMigrator.Runner.Infrastructure
{
    public class DefaultMigrationRunnerConventions : IMigrationRunnerConventions
    {
        private DefaultMigrationRunnerConventions()
        {
        }

        public static DefaultMigrationRunnerConventions Instance { get; } = new DefaultMigrationRunnerConventions();

        public Func<Type, bool> TypeIsMigration => TypeIsMigrationImpl;
        public Func<Type, bool> TypeIsProfile => TypeIsProfileImpl;
        public Func<Type, MigrationStage?> GetMaintenanceStage => GetMaintenanceStageImpl;
        public Func<Type, bool> TypeIsVersionTableMetaData => TypeIsVersionTableMetaDataImpl;
        public Func<Type, IMigrationInfo> GetMigrationInfo => GetMigrationInfoForImpl;
        public Func<Type, bool> TypeHasTags => TypeHasTagsImpl;
        public Func<Type, IEnumerable<string>, bool> TypeHasMatchingTags => TypeHasMatchingTagsImpl;

        private static bool TypeIsMigrationImpl(Type type)
        {
            return typeof(IMigration).IsAssignableFrom(type) && type.HasAttribute<MigrationAttribute>();
        }

        private static MigrationStage? GetMaintenanceStageImpl(Type type)
        {
            if (!typeof(IMigration).IsAssignableFrom(type))
                return null;

            var attribute = type.GetOneAttribute<MaintenanceAttribute>();
            return attribute?.Stage;
        }

        private static bool TypeIsProfileImpl(Type type)
        {
            return typeof(IMigration).IsAssignableFrom(type) && type.HasAttribute<ProfileAttribute>();
        }

        private static bool TypeIsVersionTableMetaDataImpl(Type type)
        {
            return typeof(IVersionTableMetaData).IsAssignableFrom(type) && type.HasAttribute<VersionTableMetaDataAttribute>();
        }

        private static IMigrationInfo GetMigrationInfoForImpl(Type migrationType)
        {
            IMigration CreateMigration()
            {
                return (IMigration) Activator.CreateInstance(migrationType);
            }

            var migrationAttribute = migrationType.GetOneAttribute<MigrationAttribute>();
            var migrationInfo = new MigrationInfo(migrationAttribute.Version, migrationAttribute.Description, migrationAttribute.TransactionBehavior, CreateMigration);

            foreach (MigrationTraitAttribute traitAttribute in migrationType.GetAllAttributes<MigrationTraitAttribute>())
                migrationInfo.AddTrait(traitAttribute.Name, traitAttribute.Value);

            return migrationInfo;
        }

        private static bool TypeHasTagsImpl(Type type)
        {
            return type.GetOneAttribute<TagsAttribute>(true) != null;
        }

        private static bool TypeHasMatchingTagsImpl(Type type, IEnumerable<string> tagsToMatch)
        {
            var tags = type.GetAllAttributes<TagsAttribute>(true);

            if (tags.Any() && !tagsToMatch.Any())
                return false;

            var tagNamesForAllBehavior = tags.Where(t => t.Behavior == TagBehavior.RequireAll).SelectMany(t => t.TagNames).ToArray();
            if (tagNamesForAllBehavior.Any() && tagsToMatch.All(t => tagNamesForAllBehavior.Any(t.Equals)))
            {
                return true;
            }

            var tagNamesForAnyBehavior = tags.Where(t => t.Behavior == TagBehavior.RequireAny).SelectMany(t => t.TagNames).ToArray();
            if (tagNamesForAnyBehavior.Any() && tagsToMatch.Any(t => tagNamesForAnyBehavior.Any(t.Equals)))
            {
                return true;
            }

            return false;
        }
    }
}
