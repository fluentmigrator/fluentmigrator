namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleQuoter : OracleQuoterQuotedIdentifier
    {
        public override string Quote(string name)
        {
            return UnQuote(name);
        }

        public override string QuoteConstraintName(string constraintName, string schemaName)
        {
            return base.QuoteConstraintName(UnQuote(constraintName), UnQuote(schemaName));
        }

        public override string QuoteIndexName(string indexName, string schemaName)
        {
            return base.QuoteIndexName(UnQuote(indexName), UnQuote(schemaName));
        }

        public override string QuoteTableName(string tableName, string schemaName)
        {
            return base.QuoteTableName(UnQuote(tableName), UnQuote(schemaName));
        }

        public override string QuoteSequenceName(string sequenceName, string schemaName)
        {
            return base.QuoteTableName(UnQuote(sequenceName), UnQuote(schemaName));
        }
    }
}
