namespace FluentMigrator.Tests.Integration.Processors
{
    public abstract class BaseSchemaExtensionsTests
    {
        public abstract void CallingColumnExistsCanAcceptSchemaNameWithSingleQuote();
        public abstract void CallingConstraintExistsCanAcceptSchemaNameWithSingleQuote();
        public abstract void CallingIndexExistsCanAcceptSchemaNameWithSingleQuote();
        public abstract void CallingSchemaExistsCanAcceptSchemaNameWithSingleQuote();
        public abstract void CallingTableExistsCanAcceptSchemaNameWithSingleQuote();
    }
}