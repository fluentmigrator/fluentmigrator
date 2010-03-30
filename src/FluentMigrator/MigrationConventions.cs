using System;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator
{
	public class MigrationConventions : IMigrationConventions
	{
		public Func<string, string> GetPrimaryKeyName { get; set; }
		public Func<ForeignKeyDefinition, string> GetForeignKeyName { get; set; }
		public Func<IndexDefinition, string> GetIndexName { get; set; }
		public Func<Type, bool> TypeIsMigration { get; set; }
		public Func<Type, bool> TypeIsVersionTableMetaData {get;set;}
		public Func<Type, MigrationMetadata> GetMetadataForMigration { get; set; }
		public Func<string> GetWorkingDirectory { get; set; }

		public MigrationConventions()
		{
			GetPrimaryKeyName = DefaultMigrationConventions.GetPrimaryKeyName;
			GetForeignKeyName = DefaultMigrationConventions.GetForeignKeyName;
			GetIndexName = DefaultMigrationConventions.GetIndexName;
			TypeIsMigration = DefaultMigrationConventions.TypeIsMigration;
			TypeIsVersionTableMetaData = DefaultMigrationConventions.TypeIsVersionTableMetaData;
			GetMetadataForMigration = DefaultMigrationConventions.GetMetadataForMigration;
			GetWorkingDirectory = DefaultMigrationConventions.GetWorkingDirectory;
		}
	}
}
