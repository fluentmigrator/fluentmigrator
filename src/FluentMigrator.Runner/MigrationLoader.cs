using System;
using System.Collections.Generic;
using System.Reflection;
using FluentMigrator.Infrastructure;
using System.Linq;

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

		public IEnumerable<MigrationMetadata> FindMigrationsIn(Assembly assembly, string @namespace)
		{
			foreach (Type type in assembly.GetExportedTypes().Where(x => x.Namespace == @namespace))
			{
				if (Conventions.TypeIsMigration(type))
					yield return Conventions.GetMetadataForMigration(type);
			}
		}
	}
}