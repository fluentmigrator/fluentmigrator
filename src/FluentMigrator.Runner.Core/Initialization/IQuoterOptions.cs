namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Conventions of how C# values are converted to SQL
    /// </summary>
    public interface IQuoterOptions
    {
        /// <remarks>
        /// If <see langword="true"/> enums are converted to a string, if <see langword="false"/> enums are converted to the underlying numeric type.<br/>
        /// Default value: <see langword="true"/>.
        /// </remarks>
        bool EnumAsString { get; }
    }
}
