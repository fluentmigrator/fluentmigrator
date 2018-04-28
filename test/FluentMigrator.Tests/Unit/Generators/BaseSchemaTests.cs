using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators
{
    [Category("Generator")]
    [Category("Schema")]
    public abstract class BaseSchemaTests
    {
        public abstract void CanAlterSchema();
        public abstract void CanCreateSchema();
        public abstract void CanDropSchema();
    }
}
