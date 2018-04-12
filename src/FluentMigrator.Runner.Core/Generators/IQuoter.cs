namespace FluentMigrator.Runner.Generators
{
    /// <summary>
    /// The interface to be implemented for handling quotes
    /// </summary>
    public interface IQuoter
    {
        /// <summary>
        /// Returns a quoted string that has been correctly escaped
        /// </summary>
        string Quote(string value);

        /// <summary>
        /// Provides an unquoted, unescaped string
        /// </summary>
        string UnQuote(string value);

        /// <summary>
        /// Quotes a value to be embedded into an SQL script/statement
        /// </summary>
        /// <param name="value">The value to be quoted</param>
        /// <returns>The quoted value</returns>
        string QuoteValue(object value);

        /// <summary>
        /// Returns true is the value starts and ends with a close quote
        /// </summary>
        bool IsQuoted(string value);

        /// <summary>
        /// Quotes a column name
        /// </summary>
        string QuoteColumnName(string columnName);

        /// <summary>
        /// Quotes a Table name
        /// </summary>
        string QuoteTableName(string tableName, string schemaName = null);

        /// <summary>
        /// Quote an index name
        /// </summary>
        string QuoteIndexName(string indexName, string schemaName = null);

        /// <summary>
        /// Quotes a constraint name
        /// </summary>
        string QuoteConstraintName(string contraintName, string schemaName = null);

        /// <summary>
        /// Quotes a Sequence name
        /// </summary>
        string QuoteSequenceName(string sequenceName, string schemaName = null);

        /// <summary>
        /// Quotes a schema name
        /// </summary>
        /// <param name="schemaName">The schema name to quote</param>
        /// <returns>The quoted schema name</returns>
        string QuoteSchemaName(string schemaName);
    }
}
