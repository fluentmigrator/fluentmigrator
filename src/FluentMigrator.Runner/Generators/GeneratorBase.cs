using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	public abstract class GeneratorBase : IMigrationGenerator
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

                if (size <= capacity)
					return ReplacePlaceholders(template, size, precision);
			}

			throw new NotSupportedException(String.Format("Unsupported DbType '{0}'", type));
		}

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

		public abstract string Generate(CreateTableExpression expression);
		public abstract string Generate(CreateColumnExpression expression);
		public abstract string Generate(DeleteTableExpression expression);
		public abstract string Generate(DeleteColumnExpression expression);
		public abstract string Generate(CreateForeignKeyExpression expression);
		public abstract string Generate(DeleteForeignKeyExpression expression);
		public abstract string Generate(CreateIndexExpression expression);
		public abstract string Generate(DeleteIndexExpression expression);
		public abstract string Generate(RenameTableExpression expression);
		public abstract string Generate(RenameColumnExpression expression);		
	}
}