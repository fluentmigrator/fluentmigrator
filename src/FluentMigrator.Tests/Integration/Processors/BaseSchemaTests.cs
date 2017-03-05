namespace FluentMigrator.Tests.Integration.Processors
{
    public abstract class BaseSchemaTests : IntegrationTestBase
    {
        public abstract void CallingSchemaExistsReturnsFalseIfSchemaDoesNotExist();
        public abstract void CallingSchemaExistsReturnsTrueIfSchemaExists();
    }
}