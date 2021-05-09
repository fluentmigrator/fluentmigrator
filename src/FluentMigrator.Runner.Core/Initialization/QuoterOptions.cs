namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Conventions of how C# values are converted to SQL
    /// </summary>
    public sealed class QuoterOptions
    {
        /// <summary>
        /// Gets or sets the value indicating how enum values are handled.<br/>
        /// If <see langword="true"/> enums are converted to a string, if <see langword="false"/> enums are converted to the underlying numeric type.<br/>
        /// Default value: <see langword="true"/>.
        /// </summary>
        public bool EnumAsString { get; set; } = true;
    }
}
