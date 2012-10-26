namespace FluentMigrator.Runner.Generators
{
    public interface IQuoter
    {
        string Quote(string value);
        string UnQuote(string value);
        string QuoteValue(object value);
        bool IsQuoted(string value);
        string QuoteColumnName(string columnName);
        string QuoteTableName(string tableName);
        string QuoteIndexName(string indexName);
        string QuoteConstraintName(string contraintName);
        string QuoteSchemaName(string schemaName);
        string QuoteCommand(string command);
        string QuoteSequenceName(string sequenceName);
    }
}
