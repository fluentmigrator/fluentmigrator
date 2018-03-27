namespace FluentMigrator.Tests.Unit.Generators
{
    public abstract class BaseTableTests
    {
        public abstract void CanCreateTableWithCustomColumnTypeWithCustomSchema();
        public abstract void CanCreateTableWithCustomColumnTypeWithDefaultSchema();
        public abstract void CanCreateTableWithCustomSchema();
        public abstract void CanCreateTableWithDefaultSchema();
        public abstract void CanCreateTableWithDefaultValueExplicitlySetToNullWithCustomSchema();
        public abstract void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema();
        public abstract void CanCreateTableWithDefaultValueWithCustomSchema();
        public abstract void CanCreateTableWithDefaultValueWithDefaultSchema();
        public abstract void CanCreateTableWithIdentityWithCustomSchema();
        public abstract void CanCreateTableWithIdentityWithDefaultSchema();
        public abstract void CanCreateTableWithMultiColumnPrimaryKeyWithCustomSchema();
        public abstract void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema();
        public abstract void CanCreateTableWithNamedMultiColumnPrimaryKeyWithCustomSchema();
        public abstract void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema();
        public abstract void CanCreateTableWithNamedPrimaryKeyWithCustomSchema();
        public abstract void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema();
        public abstract void CanCreateTableWithNullableFieldWithCustomSchema();
        public abstract void CanCreateTableWithNullableFieldWithDefaultSchema();
        public abstract void CanCreateTableWithPrimaryKeyWithCustomSchema();
        public abstract void CanCreateTableWithPrimaryKeyWithDefaultSchema();
        public abstract void CanDropTableWithCustomSchema();
        public abstract void CanDropTableWithDefaultSchema();
        public abstract void CanRenameTableWithCustomSchema();
        public abstract void CanRenameTableWithDefaultSchema();
    }
}