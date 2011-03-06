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
using System.Text;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.VersionTableInfo;
using FluentMigrator.Expressions;

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

		public static bool TypeIsProfile(Type type)
		{
			return typeof(IMigration).IsAssignableFrom(type) && type.HasAttribute<ProfileAttribute>();
		}

		public static bool TypeIsVersionTableMetaData(Type type)
		{
			return typeof(IVersionTableMetaData).IsAssignableFrom(type) && type.HasAttribute<VersionTableMetaDataAttribute>();
		}

		public static MigrationMetadata GetMetadataForMigration(Type type)
		{
			var migrationAttribute = type.GetOneAttribute<MigrationAttribute>();
			var metadata = new MigrationMetadata { Type = type, Version = migrationAttribute.Version };

			foreach (MigrationTraitAttribute traitAttribute in type.GetAllAttributes<MigrationTraitAttribute>())
				metadata.AddTrait(traitAttribute.Name, traitAttribute.Value);

			return metadata;
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

		public static string GetWorkingDirectory()
		{
			return Environment.CurrentDirectory;
		}
	}
}