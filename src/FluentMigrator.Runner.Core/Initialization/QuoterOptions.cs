namespace FluentMigrator.Runner.Initialization
{
    /// <inheritdoc cref="IQuoterOptions"/>
    public sealed class QuoterOptions : IQuoterOptions
    {
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the value indicating how enum values are handled.<br/>
        /// </summary>
        public bool EnumAsString { get; set; } = true;
    }
}
