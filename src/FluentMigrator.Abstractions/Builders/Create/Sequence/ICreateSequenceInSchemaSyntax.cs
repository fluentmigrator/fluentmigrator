namespace FluentMigrator.Builders.Create.Sequence
{
    public interface ICreateSequenceInSchemaSyntax : ICreateSequenceSyntax
    {
        ICreateSequenceSyntax InSchema(string schemaName);
    }
}