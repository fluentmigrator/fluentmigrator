namespace FluentMigrator.Tests.Integration.Processors
{
    public abstract class BaseTableTests
    {
        public abstract void CallingTableExistsCanAcceptTableNameWithSingleQuote();
        public abstract void CallingTableExistsReturnsFalseIfTableDoesNotExist();
        public abstract void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema();
        public abstract void CallingTableExistsReturnsTrueIfTableExists();
        public abstract void CallingTableExistsReturnsTrueIfTableExistsWithSchema();
    }
}