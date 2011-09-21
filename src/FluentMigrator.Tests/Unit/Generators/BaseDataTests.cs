namespace FluentMigrator.Tests.Unit.Generators
{
    public abstract class BaseDataTests
    {
        public abstract void CanInsertData();
        public abstract void CanDeleteData();
        public abstract void CanDeleteDataAllRows();
        public abstract void CanDeleteDataMultipleRows();
        public abstract void CanInsertGuidData();

        public abstract void CanUpdateData();
    }
}