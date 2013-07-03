using FluentMigrator.Runner.Versioning;
using System.Collections.Generic;
namespace FluentMigrator.Runner
{
    public interface IVersionLoader
    {
        bool AlreadyCreatedVersionSchema { get; }
        bool AlreadyCreatedVersionTable { get; }
        void DeleteVersion(long version);
        FluentMigrator.VersionTableInfo.IVersionTableMetaData GetVersionTableMetaData();
        void LoadVersionInfo(IVersionInfo versionInfo, IEnumerable<long> migratedVersions);
        void RemoveVersionTable();
        IMigrationRunner Runner { get; set; }
        void UpdateVersionInfo(long version);
        FluentMigrator.Runner.Versioning.IVersionInfo VersionInfo { get; set; }
        FluentMigrator.VersionTableInfo.IVersionTableMetaData VersionTableMetaData { get; }
    }
}