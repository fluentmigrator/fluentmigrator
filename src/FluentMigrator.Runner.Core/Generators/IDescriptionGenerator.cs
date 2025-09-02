using System.Collections.Generic;
using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Generators
{
    /// <summary>
    /// Generate SQL statements to set descriptions for tables and columns
    /// </summary>
    public interface IDescriptionGenerator
    {
        /// <summary>
        /// Generates a collection of SQL statements to add descriptions for a table and its columns.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="FluentMigrator.Expressions.CreateTableExpression"/> containing the table and column definitions
        /// for which the description statements are to be generated.
        /// </param>
        /// <returns>
        /// A collection of SQL statements as strings, where each statement represents a description for the table
        /// or its columns.
        /// </returns>
        IEnumerable<string> GenerateDescriptionStatements(CreateTableExpression expression);
        /// <summary>
        /// Generates a SQL statement to set a description for a table.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="FluentMigrator.Expressions.AlterTableExpression"/> containing the table details 
        /// and the description to be applied.
        /// </param>
        /// <returns>
        /// A SQL statement that sets the description for the specified table.
        /// </returns>
        string GenerateDescriptionStatement(AlterTableExpression expression);
        /// <summary>
        /// Generates a SQL statement to set a description for a specific column in a table.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="CreateColumnExpression"/> containing the details of the column for which the description is to be generated.
        /// </param>
        /// <returns>
        /// A SQL statement that sets the description for the specified column, or an empty string if no description is provided.
        /// </returns>
        string GenerateDescriptionStatement(CreateColumnExpression expression);
        /// <summary>
        /// Generates a SQL statement to describe or annotate a column in a database schema.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="FluentMigrator.Expressions.AlterColumnExpression"/> representing the column to be described.
        /// </param>
        /// <returns>
        /// A SQL string that defines the description for the specified column.
        /// </returns>
        string GenerateDescriptionStatement(AlterColumnExpression expression);
    }
}
