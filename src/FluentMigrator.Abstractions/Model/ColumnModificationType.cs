namespace FluentMigrator.Model
{
    /// <summary>
    /// Indicates whether a column should be created or altered
    /// </summary>
    public enum ColumnModificationType
    {
        /// <summary>
        /// The column in question should be created
        /// </summary>
        Create,

        /// <summary>
        /// The column in question should be altered
        /// </summary>
        Alter
    }
}
