using System.Reflection;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Runner
{
	public interface IVersionLoader
	{
		//VersionInfo VersionInfo { get; }
	    bool VersionSchemaExists { get; }
		bool VersionTableExists { get; }
		void RemoveVersionTable();
		IVersionTableMetaData GetVersionTableMetaData();
	}
}