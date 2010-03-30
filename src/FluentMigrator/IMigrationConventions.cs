using System;
namespace FluentMigrator
{
	public interface IMigrationConventions
	{
		Func<Model.ForeignKeyDefinition, string> GetForeignKeyName { get; set; }
		Func<Model.IndexDefinition, string> GetIndexName { get; set; }
		Func<Type, Infrastructure.MigrationMetadata> GetMetadataForMigration { get; set; }
		Func<string, string> GetPrimaryKeyName { get; set; }
		Func<Type, bool> TypeIsMigration { get; set; }
		Func<Type, bool> TypeIsVersionTableMetaData { get; set; }
		Func<string> GetWorkingDirectory { get; set; }
	}
}
