
namespace FluentMigrator.Runner.Helpers
{
    /// <summary>
    /// Provides helper methods for formatting SQL strings.
    /// </summary>
    /// <remarks>
    /// This class contains utility methods to assist with SQL string formatting,
    /// such as escaping special characters to ensure safe SQL execution.
    /// </remarks>
    public class FormatHelper
    {
        /// <summary>
        /// Escapes single quotes in a SQL string to prevent SQL injection or syntax errors.
        /// </summary>
        /// <param name="sql">The SQL string to be escaped.</param>
        /// <returns>A new string with single quotes escaped by doubling them.</returns>
        /// <remarks>
        /// This method is commonly used to sanitize SQL strings by replacing single quotes with two single quotes,
        /// ensuring that the resulting string is safe for use in SQL queries.
        /// </remarks>
        public static string FormatSqlEscape(string sql)
        {
            return sql.Replace("'", "''");
        }
    }
}
