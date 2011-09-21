namespace FluentMigrator.Tests.Unit.Generators
{
    public abstract class BaseTableAlterTests
    {
        //Alter Tests
        public abstract void CanAddColumn();
        public abstract void CanAddDecimalColumn();
        public abstract void CanRenameColumn();
        public abstract void CanRenameTable();
        public abstract void CanAlterColumn();
        public abstract void CanCreateForeignKey();
        public abstract void CanCreateMulitColumnForeignKey();
        public abstract void CanCreateAutoIncrementColumn();

        public abstract void CanAlterSchema();
    }
}