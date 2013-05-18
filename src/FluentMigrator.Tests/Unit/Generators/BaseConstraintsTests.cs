namespace FluentMigrator.Tests.Unit.Generators
{
    public abstract class BaseConstraintsTests
    {
        public abstract void CanCreateForeignKeyWithCustomSchema();
        public abstract void CanCreateForeignKeyWithDefaultSchema();
        public abstract void CanCreateForeignKeyWithDifferentSchemas();
        public abstract void CanCreateMultiColumnForeignKeyWithCustomSchema();
        public abstract void CanCreateMultiColumnForeignKeyWithDefaultSchema();
        public abstract void CanCreateMultiColumnForeignKeyWithDifferentSchemas();
        public abstract void CanCreateMultiColumnPrimaryKeyConstraintWithCustomSchema();
        public abstract void CanCreateMultiColumnPrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanCreateMultiColumnUniqueConstraintWithCustomSchema();
        public abstract void CanCreateMultiColumnUniqueConstraintWithDefaultSchema();
        public abstract void CanCreateNamedForeignKeyWithCustomSchema();
        public abstract void CanCreateNamedForeignKeyWithDefaultSchema();
        public abstract void CanCreateNamedForeignKeyWithDifferentSchemas();
        public abstract void CanCreateNamedForeignKeyWithOnDeleteAndOnUpdateOptions();
        public abstract void CanCreateNamedForeignKeyWithOnDeleteOptions(System.Data.Rule rule, string output);
        public abstract void CanCreateNamedForeignKeyWithOnUpdateOptions(System.Data.Rule rule, string output);
        public abstract void CanCreateNamedMultiColumnForeignKeyWithCustomSchema();
        public abstract void CanCreateNamedMultiColumnForeignKeyWithDefaultSchema();
        public abstract void CanCreateNamedMultiColumnForeignKeyWithDifferentSchemas();
        public abstract void CanCreateNamedMultiColumnPrimaryKeyConstraintWithCustomSchema();
        public abstract void CanCreateNamedMultiColumnPrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanCreateNamedMultiColumnUniqueConstraintWithCustomSchema();
        public abstract void CanCreateNamedMultiColumnUniqueConstraintWithDefaultSchema();
        public abstract void CanCreateNamedPrimaryKeyConstraintWithCustomSchema();
        public abstract void CanCreateNamedPrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanCreateNamedUniqueConstraintWithCustomSchema();
        public abstract void CanCreateNamedUniqueConstraintWithDefaultSchema();
        public abstract void CanCreatePrimaryKeyConstraintWithCustomSchema();
        public abstract void CanCreatePrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanCreateUniqueConstraintWithCustomSchema();
        public abstract void CanCreateUniqueConstraintWithDefaultSchema();
        public abstract void CanDropForeignKeyWithCustomSchema();
        public abstract void CanDropForeignKeyWithDefaultSchema();
        public abstract void CanDropPrimaryKeyConstraintWithCustomSchema();
        public abstract void CanDropPrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanDropUniqueConstraintWithCustomSchema();
        public abstract void CanDropUniqueConstraintWithDefaultSchema();
    }
}