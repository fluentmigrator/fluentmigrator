namespace FluentMigrator
{
    public interface IMigrationProcessorOptions
    {
        bool PreviewOnly { get; set; }
        int Timeout { get; set; }
    }
}