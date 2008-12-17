using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentMigrator.Runner
{
	public interface IMigrationLoader
	{
		IEnumerable<MigrationDefinition> FindMigrationsIn(Assembly assembly);
	}
}