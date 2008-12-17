using System;

namespace FluentMigrator.Runner
{
	public class MigrationDefinition
	{
		public Type Type { get; set; }
		public long Version { get; set; }
	}
}