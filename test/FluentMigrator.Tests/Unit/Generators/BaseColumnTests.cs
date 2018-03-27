using System.Data;

namespace FluentMigrator.Tests.Unit.Generators
{
    public abstract class BaseColumnTests
    {
        public abstract void CanAlterColumnWithCustomSchema();
        public abstract void CanAlterColumnWithDefaultSchema();
        public abstract void CanCreateAutoIncrementColumnWithCustomSchema();
        public abstract void CanCreateAutoIncrementColumnWithDefaultSchema();
        public abstract void CanCreateColumnWithCustomSchema();
        public abstract void CanCreateColumnWithDefaultSchema();
        public abstract void CanCreateDecimalColumnWithCustomSchema();
        public abstract void CanCreateDecimalColumnWithDefaultSchema();
        public abstract void CanDropColumnWithCustomSchema();
        public abstract void CanDropColumnWithDefaultSchema();
        public abstract void CanDropMultipleColumnsWithCustomSchema();
        public abstract void CanDropMultipleColumnsWithDefaultSchema();
        public abstract void CanRenameColumnWithCustomSchema();
        public abstract void CanRenameColumnWithDefaultSchema();
    }
}