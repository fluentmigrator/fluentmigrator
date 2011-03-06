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
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Expressions;

namespace FluentMigrator
{
	public class MigrationConventions : IMigrationConventions
	{
		public Func<string, string> GetPrimaryKeyName { get; set; }
		public Func<ForeignKeyDefinition, string> GetForeignKeyName { get; set; }
		public Func<IndexDefinition, string> GetIndexName { get; set; }
		public Func<Type, bool> TypeIsMigration { get; set; }
		public Func<Type, bool> TypeIsProfile { get; set; }
		public Func<Type, bool> TypeIsVersionTableMetaData {get;set;}
		public Func<Type, MigrationMetadata> GetMetadataForMigration { get; set; }
		public Func<string> GetWorkingDirectory { get; set; }

        public Func<Model.ConstraintDefinition, string> GetConstraintName { get; set; }


		public MigrationConventions()
		{
			GetPrimaryKeyName = DefaultMigrationConventions.GetPrimaryKeyName;
			GetForeignKeyName = DefaultMigrationConventions.GetForeignKeyName;
			GetIndexName = DefaultMigrationConventions.GetIndexName;
			TypeIsMigration = DefaultMigrationConventions.TypeIsMigration;
			TypeIsProfile = DefaultMigrationConventions.TypeIsProfile;
			TypeIsVersionTableMetaData = DefaultMigrationConventions.TypeIsVersionTableMetaData;
			GetMetadataForMigration = DefaultMigrationConventions.GetMetadataForMigration;
			GetWorkingDirectory = DefaultMigrationConventions.GetWorkingDirectory;
            GetConstraintName = DefaultMigrationConventions.GetConstraintName;
              
		}
	}
}
