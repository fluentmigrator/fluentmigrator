using System;
using System.Collections.Generic;
using System.Reflection;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner
{
	public class MigrationLoader : IMigrationLoader
	{
		public IMigrationConventions Conventions { get; private set; }

		public MigrationLoader(IMigrationConventions conventions)
		{
			Conventions = conventions;
		}

		public IEnumerable<MigrationMetadata> FindMigrationsIn(Assembly assembly)
		{
			foreach (Type type in assembly.GetExportedTypes())
			{
				if (Conventions.TypeIsMigration(type))
					yield return Conventions.GetMetadataForMigration(type);
			}
		}
	}
}