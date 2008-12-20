using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	public abstract class GeneratorBase : IMigrationGenerator
	{
		protected const string SizePlaceholder = "$size";
		protected const string PrecisionPlaceholder = "$precision";

		private readonly Dictionary<DbType, SortedList<int, string>> _templates = new Dictionary<DbType, SortedList<int, string>>();

		public GeneratorBase()
		{
			SetupTypeMaps();
		}

		protected abstract void SetupTypeMaps();

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

		public virtual string GenerateDDLForColumn(ColumnDefinition column)
		{
			var sb = new StringBuilder();
			
			sb.Append(column.Name);
			sb.Append(" ");
			sb.Append(GetTypeMap(column.Type.Value, column.Size, column.Precision));

			if (column.IsPrimaryKey)
			{
				sb.Append(" PRIMARY KEY CLUSTERED");
			}

			return sb.ToString();
		}

		protected string GetColumnDDL(IList<ColumnDefinition> columns)
		{
			string result = "";
			int total = columns.Count - 1;

			//if more than one column is a primary key, then it needs to be added separately            
			IList<ColumnDefinition> primaryKeyColumns = GetPrimaryKeyColumns(columns);
			if (primaryKeyColumns.Count > 1)
			{
				foreach (ColumnDefinition column in primaryKeyColumns)
				{
					column.IsPrimaryKey = false;
				}
			}

			for (int i = 0; i < columns.Count; i++)
			{
				result += GenerateDDLForColumn(columns[i]);

				if (i != total)
					result += ", ";
			}

			result = AddPrimaryKeyConstraint(primaryKeyColumns, result);

			return result;
		}

		private IList<ColumnDefinition> GetPrimaryKeyColumns(IList<ColumnDefinition> columns)
		{
			IList<ColumnDefinition> primaryKeyColumns = new List<ColumnDefinition>();
			foreach (ColumnDefinition column in columns)
			{
				if (column.IsPrimaryKey)
				{
					primaryKeyColumns.Add(column);
				}
			}
			return primaryKeyColumns;
		}

		private string AddPrimaryKeyConstraint(IList<ColumnDefinition> primaryKeyColumns, string result)
		{
			if (primaryKeyColumns.Count > 1)
			{
				string keyName = "";
				string keyColumns = "";
				foreach (ColumnDefinition column in primaryKeyColumns)
				{
					keyName += column.Name + "_";
					keyColumns += column.Name + ",";
				}
				keyName += "PK";
				keyColumns = keyColumns.TrimEnd(',');
				result += String.Format(", CONSTRAINT {0} PRIMARY KEY ({1})", keyName, keyColumns);
			}
			return result;
		}
	}
}