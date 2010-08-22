using FluentMigrator.Runner.Versioning;

namespace FluentMigrator.Runner
{
	public interface IVersionLoader
	{
		VersionInfo VersionInfo { get; }
		void RemoveVersionTable();
	}
}