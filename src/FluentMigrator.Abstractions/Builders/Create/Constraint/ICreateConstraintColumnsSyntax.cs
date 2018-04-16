namespace FluentMigrator.Builders.Create.Constraint
{
    /// <summary>
    /// Define the columns of a constraint
    /// </summary>
    public interface ICreateConstraintColumnsSyntax
    {
        /// <summary>
        /// The column for the constraint
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns>Other constraint options</returns>
        ICreateConstraintOptionsSyntax Column(string columnName);

        /// <summary>
        /// The columns for the constraint
        /// </summary>
        /// <param name="columnNames">The column names</param>
        /// <returns>Other constraint options</returns>
        ICreateConstraintOptionsSyntax Columns(params string[] columnNames);
    }
}
