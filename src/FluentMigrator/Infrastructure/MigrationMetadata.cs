using System;

namespace FluentMigrator.Infrastructure
{
	public class MigrationMetadata
	{
		public Type Type { get; set; }
		public long Version { get; set; }
	}
}