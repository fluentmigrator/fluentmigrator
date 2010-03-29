using System.Collections.Generic;
using System.Reflection;
using FluentMigrator.Infrastructure;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Runner
{
	public interface IMigrationLoader
	{
		IEnumerable<MigrationMetadata> FindMigrationsIn(Assembly assembly, string @namespace);
		IVersionTableMetaData GetVersionTableMetaData (Assembly assembly);
	}
}