using System.Reflection;
using FluentMigrator.Runner.Versioning;

namespace FluentMigrator.Runner
{
	public interface IMigrationVersionRunner
	{
		Assembly MigrationAssembly { get; }
		VersionInfo VersionInfo { get; }
		void MigrateUp();
		void MigrateUp(long version);
		void Rollback(int steps);
		void RollbackToVersion( long version );
		void MigrateDown(long version);
		void RemoveVersionTable();
	}
}