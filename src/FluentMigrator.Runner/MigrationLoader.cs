using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentMigrator.Runner
{
	public class MigrationLoader : IMigrationLoader
	{
		public MigrationConventions Conventions { get; private set; }

		public MigrationLoader(MigrationConventions conventions)
		{
			Conventions = conventions;
		}

		public IEnumerable<MigrationDefinition> FindMigrationsIn(Assembly assembly)
		{
			foreach (Type type in assembly.GetExportedTypes())
			{
				if (Conventions.TypeIsMigration(type))
				{
					long version = Conventions.GetMigrationVersion(type);
					yield return new MigrationDefinition { Type = type, Version = version };
				}
			}
		}
	}
}