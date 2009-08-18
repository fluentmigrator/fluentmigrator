using System;
using System.Collections.Generic;

namespace FluentMigrator.Infrastructure
{
	public class MigrationMetadata
	{
		private readonly Dictionary<string, object> _traits = new Dictionary<string, object>();

		public Type Type { get; set; }
		public long Version { get; set; }

		public object Trait(string name)
		{
			return _traits.ContainsKey(name) ? _traits[name] : null;
		}

		public bool HasTrait(string name)
		{
			return _traits.ContainsKey(name);
		}

		public void AddTrait(string name, object value)
		{
			_traits.Add(name, value);
		}
	}
}