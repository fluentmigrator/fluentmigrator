using System;

namespace FluentMigrator.VersionTableInfo
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class VersionTableMetaDataAttribute : Attribute
	{
		public VersionTableMetaDataAttribute()
		{
		}
	}
}