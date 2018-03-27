namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleQuoter : OracleQuoterQuotedIdentifier
    {
        public override string QuoteColumnName(string columnName)
        {
            return columnName;
        }

        public override string QuoteConstraintName(string constraintName)
        {
            return constraintName;
        }

        public override string QuoteIndexName(string indexName)
        {
            return indexName;
        }

        public override string QuoteSchemaName(string schemaName)
        {
            return schemaName;
        }

        public override string QuoteTableName(string tableName)
        {
            return tableName;
        }

        public override string QuoteSequenceName(string sequenceName)
        {
            return sequenceName;
        }
    }
}