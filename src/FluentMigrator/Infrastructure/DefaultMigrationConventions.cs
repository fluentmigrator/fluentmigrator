#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.Text;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Infrastructure
{
    public static class DefaultMigrationConventions
    {
        public static string GetPrimaryKeyName(string tableName)
        {
            return "PK_" + tableName;
        }

        public static string GetForeignKeyName(ForeignKeyDefinition foreignKey)
        {
            var sb = new StringBuilder();

            sb.Append("FK_");
            sb.Append(foreignKey.ForeignTable);

            foreach (string foreignColumn in foreignKey.ForeignColumns)
            {
                sb.Append("_");
                sb.Append(foreignColumn);
            }

            sb.Append("_");
            sb.Append(foreignKey.PrimaryTable);

            foreach (string primaryColumn in foreignKey.PrimaryColumns)
            {
                sb.Append("_");
                sb.Append(primaryColumn);
            }

            return sb.ToString();
        }

        public static string GetIndexName(IndexDefinition index)
        {
            var sb = new StringBuilder();

            sb.Append("IX_");
            sb.Append(index.TableName);

            foreach (IndexColumnDefinition column in index.Columns)
            {
                sb.Append("_");
                sb.Append(column.Name);
            }

            return sb.ToString();
        }

        public static bool TypeIsMigration(Type type)
        {
            return typeof(IMigration).IsAssignableFrom(type) && type.HasAttribute<MigrationAttribute>();
        }

        public static MigrationStage? GetMaintenanceStage(Type type) 
        {
            if (!typeof(IMigration).IsAssignableFrom(type))
                return null;
            
            var attribute = type.GetOneAttribute<MaintenanceAttribute>();
            return attribute != null ? attribute.Stage : (MigrationStage?)null;
        }

        public static bool TypeIsProfile(Type type)
        {
            return typeof(IMigration).IsAssignableFrom(type) && type.HasAttribute<ProfileAttribute>();
        }

        public static bool TypeIsVersionTableMetaData(Type type)
        {
            return typeof(IVersionTableMetaData).IsAssignableFrom(type) && type.HasAttribute<VersionTableMetaDataAttribute>();
        }

        public static IMigrationInfo GetMigrationInfoFor(Type migrationType)
        {
            var migrationAttribute = migrationType.GetOneAttribute<MigrationAttribute>();
            Func<IMigration> migrationFunc = () => (IMigration)migrationType.Assembly.CreateInstance(migrationType.FullName);
            var migrationInfo = new MigrationInfo(migrationAttribute.Version, migrationAttribute.Description, migrationAttribute.TransactionBehavior, migrationFunc);

            foreach (MigrationTraitAttribute traitAttribute in migrationType.GetAllAttributes<MigrationTraitAttribute>())
                migrationInfo.AddTrait(traitAttribute.Name, traitAttribute.Value);

            return migrationInfo;
        }

        public static string GetWorkingDirectory()
        {
            return Environment.CurrentDirectory;
        }

        public static string GetConstraintName(ConstraintDefinition expression)
        {
            StringBuilder sb = new StringBuilder();
            if (expression.IsPrimaryKeyConstraint)
            {
                sb.Append("PK_");
            }
            else
            {
                sb.Append("UC_");
            }

            sb.Append(expression.TableName);
            foreach (var column in expression.Columns)
            {
                sb.Append("_" + column);
            }
            return sb.ToString();
        }

        public static bool TypeHasTags(Type type)
        {
            return type.GetOneAttribute<TagsAttribute>(true) != null;
        }

        public static bool TypeHasMatchingTags(Type type, IEnumerable<string> tagsToMatch)
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

        public static string GetAutoScriptUpName(Type type, string databaseType)
        {
            if (TypeIsMigration(type))
            {
                var version = type.GetOneAttribute<MigrationAttribute>().Version;
                return string.Format("Scripts.Up.{0}_{1}_{2}.sql"
                        , version
                        , type.Name
                        , databaseType);
            }
            return string.Empty;
        }

        public static string GetAutoScriptDownName(Type type, string databaseType)
        {
            if (TypeIsMigration(type))
            {
                var version = type.GetOneAttribute<MigrationAttribute>().Version;
                return string.Format("Scripts.Down.{0}_{1}_{2}.sql"
                        , version
                        , type.Name
                        , databaseType);
            }
            return string.Empty;
        }
    }
}
