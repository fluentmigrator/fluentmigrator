namespace FluentMigrator
{
    /// <summary>
    /// Options for the <see cref="IMigrationProcessor"/>
    /// </summary>
    public interface IMigrationProcessorOptions
    {
        /// <summary>
        /// Gets a value indicating whether a preview-only mode is active
        /// </summary>
        bool PreviewOnly { get; }

        /// <summary>
        /// Gets the global timeout
        /// </summary>
        int? Timeout { get; }

        /// <summary>
        /// Gets the provider switches
        /// </summary>
        string ProviderSwitches { get; }
    }
}
