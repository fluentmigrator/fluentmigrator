namespace FluentMigrator.Tests.Unit.Generators
{
    public abstract class BaseIndexTests
    {
        public abstract void CanCreateIndexWithCustomSchema();
        public abstract void CanCreateIndexWithDefaultSchema();
        public abstract void CanCreateMultiColumnIndexWithCustomSchema();
        public abstract void CanCreateMultiColumnIndexWithDefaultSchema();
        public abstract void CanCreateMultiColumnUniqueIndexWithCustomSchema();
        public abstract void CanCreateMultiColumnUniqueIndexWithDefaultSchema();
        public abstract void CanCreateUniqueIndexWithCustomSchema();
        public abstract void CanCreateUniqueIndexWithDefaultSchema();
        public abstract void CanDropIndexWithCustomSchema();
        public abstract void CanDropIndexWithDefaultSchema();
    }
}
