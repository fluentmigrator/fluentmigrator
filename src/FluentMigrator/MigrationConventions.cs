using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator
{
	public class MigrationConventions
	{
		public Func<CreateTableExpression, string> GetPrimaryKeyName { get; set; }
		public Func<ForeignKeyDefinition, string> GetForeignKeyName { get; set; }
		public Func<Type, bool> TypeIsMigration { get; set; }
		public Func<Type, MigrationMetadata> GetMetadataForMigration { get; set; }

		public MigrationConventions()
		{
			GetPrimaryKeyName = DefaultMigrationConventions.GetPrimaryKeyName;
			GetForeignKeyName = DefaultMigrationConventions.GetForeignKeyName;
			TypeIsMigration = DefaultMigrationConventions.TypeIsMigration;
			GetMetadataForMigration = DefaultMigrationConventions.GetMetadataForMigration;
		}
	}
}
