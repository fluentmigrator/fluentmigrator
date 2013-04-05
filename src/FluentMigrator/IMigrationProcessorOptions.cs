namespace FluentMigrator
{
    public interface IMigrationProcessorOptions
    {
        bool PreviewOnly { get; }
        int Timeout { get; }
        string ProviderSwitches { get; }
    }
}