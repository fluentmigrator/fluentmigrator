using System.Reflection;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Runner
{
    public interface IVersionLoader
    {
	    bool VersionSchemaExists { get; }
		bool VersionTableExists { get; }
        void DeleteVersion(long version);
        FluentMigrator.VersionTableInfo.IVersionTableMetaData GetVersionTableMetaData();
        void LoadVersionInfo();
        void RemoveVersionTable();
        IMigrationRunner Runner { get; set; }
        void UpdateVersionInfo(long version);
        FluentMigrator.Runner.Versioning.IVersionInfo VersionInfo { get; set; }
        FluentMigrator.VersionTableInfo.IVersionTableMetaData VersionTableMetaData { get; }
    }
}