using System;
using System.Collections.Generic;
using System.Data;

namespace FluentMigrator.Runner.Dialects
{
	public class TypeMap : ITypeMap
	{
		private const string SizePlaceholder = "$size";
		private const string PrecisionPlaceholder = "$precision";
		private const string ScalePlaceholder = "$scale";

		private readonly Dictionary<DbType, SortedList<int, string>> _templates = new Dictionary<DbType, SortedList<int, string>>();

		public void Set(DbType type, string template)
		{
			EnsureHasList(type);
			_templates[type][0] = template;
		}

		public void Set(DbType type, int maxSize, string template)
		{
			EnsureHasList(type);
			_templates[type][maxSize] = template;
		}

		public string Get(DbType type, int size, int precision, int scale)
		{
			if (!_templates.ContainsKey(type))
				throw new NotSupportedException(String.Format("Unsupported DbType '{0}'", type));

			if (size == 0)
				return ReplacePlaceholders(_templates[type][0], size, precision, scale);

			foreach (KeyValuePair<int, string> entry in _templates[type])
			{
				int capacity = entry.Key;
				string template = entry.Value;

				if (capacity <= size)
					return ReplacePlaceholders(template, size, precision, scale);
			}

			throw new NotSupportedException(String.Format("Unsupported DbType '{0}'", type));
		}

		private string ReplacePlaceholders(string value, int size, int precision, int scale)
		{
			return value.Replace(SizePlaceholder, size.ToString())
				.Replace(PrecisionPlaceholder, precision.ToString())
				.Replace(ScalePlaceholder, scale.ToString());
		}

		private void EnsureHasList(DbType type)
		{
			if (!_templates.ContainsKey(type))
				_templates.Add(type, new SortedList<int, string>());
		}
	}
}