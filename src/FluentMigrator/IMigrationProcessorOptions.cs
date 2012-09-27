namespace FluentMigrator
{
    public interface IMigrationProcessorOptions
    {
        bool PreviewOnly { get; }
        int Timeout { get; }
        bool UseTrans { get; set; }
    }
}