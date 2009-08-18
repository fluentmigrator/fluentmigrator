using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Dialects
{
	public abstract class DialectBase : IDialect
	{
		protected const string SizePlaceholder = "$size";
		protected const string PrecisionPlaceholder = "$precision";

		private readonly Dictionary<DbType, SortedList<int, string>> _templates = new Dictionary<DbType, SortedList<int, string>>();

		public void SetTypeMap(DbType type, string template)
		{
			EnsureHasList(type);
			_templates[type][0] = template;
		}

		public void SetTypeMap(DbType type, string template, int maxSize)
		{
			EnsureHasList(type);
			_templates[type][maxSize] = template;
		}

		public string GetTypeMap(DbType type, int size, int precision)
		{
			if (!_templates.ContainsKey(type))
				throw new NotSupportedException(String.Format("Unsupported DbType '{0}'", type));

			if (size == 0)
				return ReplacePlaceholders(_templates[type][0], size, precision);

			foreach (KeyValuePair<int, string> entry in _templates[type])
			{
				int capacity = entry.Key;
				string template = entry.Value;

				if (capacity <= size)
					return ReplacePlaceholders(template, size, precision);
			}

			throw new NotSupportedException(String.Format("Unsupported DbType '{0}'", type));
		}

		public abstract string GenerateDDLForColumn(ColumnDefinition column);

		protected string ReplacePlaceholders(string value, int size, int precision)
		{
			return value.Replace(SizePlaceholder, size.ToString())
				.Replace(PrecisionPlaceholder, precision.ToString());
		}

		private void EnsureHasList(DbType type)
		{
			if (!_templates.ContainsKey(type))
				_templates.Add(type, new SortedList<int, string>());
		}
	}
}