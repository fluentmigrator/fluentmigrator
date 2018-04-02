using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner
{
    public interface IVersionLoader
    {
        bool AlreadyCreatedVersionSchema { get; }
        bool AlreadyCreatedVersionTable { get; }
        void DeleteVersion(long version);
        VersionTableInfo.IVersionTableMetaData GetVersionTableMetaData();
        void LoadVersionInfo();
        void RemoveVersionTable();
        IMigrationRunner Runner { get; set; }
        void UpdateVersionInfo(long version);
        void UpdateVersionInfo(long version, string description);
        Versioning.IVersionInfo VersionInfo { get; set; }
        VersionTableInfo.IVersionTableMetaData VersionTableMetaData { get; }
    }
}
