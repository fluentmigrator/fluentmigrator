namespace FluentMigrator.Tests.Unit.Generators
{
    public abstract class BaseDataTests
    {
        public abstract void CanDeleteDataForAllRowsWithCustomSchema();
        public abstract void CanDeleteDataForAllRowsWithDefaultSchema();
        public abstract void CanDeleteDataForMultipleRowsWithCustomSchema();
        public abstract void CanDeleteDataForMultipleRowsWithDefaultSchema();
        public abstract void CanDeleteDataWithCustomSchema();
        public abstract void CanDeleteDataWithDefaultSchema();
        public abstract void CanInsertDataWithCustomSchema();
        public abstract void CanInsertDataWithDefaultSchema();
        public abstract void CanInsertGuidDataWithCustomSchema();
        public abstract void CanInsertGuidDataWithDefaultSchema();
        public abstract void CanUpdateDataForAllDataWithCustomSchema();
        public abstract void CanUpdateDataForAllDataWithDefaultSchema();
        public abstract void CanUpdateDataWithCustomSchema();
        public abstract void CanUpdateDataWithDefaultSchema();
    }
}
