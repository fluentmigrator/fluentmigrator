using System;
namespace FluentMigrator
{
    public interface IMigrationConventions
    {
        Func<FluentMigrator.Model.ForeignKeyDefinition, string> GetForeignKeyName { get; set; }
        Func<FluentMigrator.Model.IndexDefinition, string> GetIndexName { get; set; }
        Func<Type, FluentMigrator.Infrastructure.MigrationMetadata> GetMetadataForMigration { get; set; }
        Func<string, string> GetPrimaryKeyName { get; set; }
        Func<Type, bool> TypeIsMigration { get; set; }
    }
}
