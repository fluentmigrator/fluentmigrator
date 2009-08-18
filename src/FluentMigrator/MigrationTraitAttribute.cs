using System;

namespace FluentMigrator
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class MigrationTraitAttribute : Attribute
	{
		public string Name { get; private set; }
		public object Value { get; private set; }

		public MigrationTraitAttribute(string name)
		{
			Name = name;
		}

		public MigrationTraitAttribute(string name, object value)
			: this(name)
		{
			Value = value;
		}
	}
}
