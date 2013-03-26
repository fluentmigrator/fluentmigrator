namespace FluentMigrator.Tests.Integration.Processors
{
    public abstract class BaseSchemaTests
    {
        public abstract void CallingSchemaExistsReturnsFalseIfSchemaDoesNotExist();
        public abstract void CallingSchemaExistsReturnsTrueIfSchemaExists();
    }
}