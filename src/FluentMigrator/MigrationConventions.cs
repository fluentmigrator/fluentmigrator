using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator
{
	public class MigrationConventions
	{
		public Func<CreateTableExpression, string> GetPrimaryKeyName { get; set; }
		public Func<CreateForeignKeyExpression, string> GetForeignKeyName { get; set; }
		public Func<Type, bool> TypeIsMigration { get; set; }
		public Func<Type, long> GetMigrationVersion { get; set; }

		public MigrationConventions()
		{
			GetPrimaryKeyName = DefaultMigrationConventions.GetPrimaryKeyName;
			GetForeignKeyName = DefaultMigrationConventions.GetForeignKeyName;
			TypeIsMigration = DefaultMigrationConventions.TypeIsMigration;
			GetMigrationVersion = DefaultMigrationConventions.GetMigrationVersion;
		}
	}
}
