namespace FluentMigrator.Builders.Create.Index
{
    /// <summary>
    /// Syntax for continue setting more column options or add another column to the index.
    /// </summary>
    public interface ICreateIndexMoreColumnOptionsSyntax : ICreateIndexOnColumnSyntax
    {
        /// <summary>
        /// Column should have unique values, but multiple rows with null values should be accepted.
        /// </summary>
        ICreateIndexOnColumnSyntax UniqueWithNullsNotDistinct();
        /// <summary>
        /// Column should have unique values. Only one row with null value should be accepted (default for most known database engines).
        /// </summary>
        ICreateIndexOnColumnSyntax UniqueWithNullsDistinct();
    }
}
