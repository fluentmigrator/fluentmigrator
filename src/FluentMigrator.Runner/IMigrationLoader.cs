using System;
using System.Collections.Generic;
using System.Reflection;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner
{
	public interface IMigrationLoader
	{
		IEnumerable<MigrationMetadata> FindMigrationsIn(Assembly assembly);
	}
}