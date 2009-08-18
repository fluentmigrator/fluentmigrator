using System;

namespace FluentMigrator
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class MigrationAttribute : Attribute
	{
		public long Version { get; private set; }

		public MigrationAttribute(long version)
		{
			Version = version;
		}
	}
}
