namespace FluentMigrator.Builders
{
    public interface IInSchemaSyntax
    {
        IInSchemaSyntax InSchema(string schemaName);

        IInSchemaSyntax CheckIfExists(bool enabled = true);
    }
}