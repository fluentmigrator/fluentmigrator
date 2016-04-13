namespace FluentMigrator.Builders.Create.Index
{
    /// <summary>
    /// Special options for unique indexes combined with default create index options.
    /// </summary>
    public interface ICreateIndexUniqueOptionsSyntax
    {
        /// <summary>
        /// Multiple rows with null values in index column(s) will be allowed.
        /// </summary>
        ICreateIndexOptionsSyntax WithNullsNotDistinct();
        /// <summary>
        /// Only one row with null values in index column(s) will be allowed.
        /// </summary>
        ICreateIndexOptionsSyntax WithNullsDistinct();
        /// <summary>
        /// Specify more options for the index.
        /// </summary>
        ICreateIndexOptionsSyntax WithOptions();
    }
}