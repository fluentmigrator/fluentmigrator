namespace FluentMigrator.Tests.Unit.Generators
{
    public abstract class BaseSchemaTests
    {
        public abstract void CanAlterSchema();
        public abstract void CanCreateSchema();
        public abstract void CanDropSchema();
    }
}
