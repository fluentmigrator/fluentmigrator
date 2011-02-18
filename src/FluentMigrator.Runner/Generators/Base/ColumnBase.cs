

namespace FluentMigrator.Runner.Generators.Base
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;

	internal abstract class ColumnBase : IColumn
	{
		private readonly ITypeMap _typeMap;
		private readonly IConstantFormatter _constantFormatter;
		protected IList<Func<ColumnDefinition, string>> ClauseOrder { get; set; }
        protected virtual bool CanSeperatePrimaryKeyAndIdentity { get { return true; } }

		public ColumnBase(ITypeMap typeMap, IConstantFormatter constantFormatter)
		{			_typeMap = typeMap;
			_constantFormatter = constantFormatter;
			ClauseOrder = new List<Func<ColumnDefinition, string>> { FormatName, FormatType, FormatNullable, FormatDefaultValue, FormatIdentity, FormatPrimaryKey };
		}

		protected string GetTypeMap(DbType value, int size, int precision)
		{
			return _typeMap.GetTypeMap(value, size, precision);
		}

		protected IConstantFormatter Constant
		{
			get { return _constantFormatter; }
		}

		protected virtual string FormatName(ColumnDefinition column)
		{
			return column.Name;
		}

		protected virtual string FormatType(ColumnDefinition column)
		{
			if (!column.Type.HasValue)
				return column.CustomType;

			return GetTypeMap(column.Type.Value, column.Size, column.Precision);
		}

		protected virtual string FormatNullable(ColumnDefinition column)
		{
			return !column.IsNullable ? "NOT NULL" : string.Empty;
		}

		protected virtual string FormatDefaultValue(ColumnDefinition column)
		{
			if (column.DefaultValue is ColumnDefinition.UndefinedDefaultValue)
				return string.Empty;

			// see if this is for a system method
			if (column.DefaultValue is SystemMethods)
			{
				string method = FormatSystemMethods((SystemMethods)column.DefaultValue);
				if (string.IsNullOrEmpty(method))
					return string.Empty;

				return "DEFAULT " + method;
			}

			return "DEFAULT " + Constant.Format(column.DefaultValue);
		}

		protected abstract string FormatIdentity(ColumnDefinition column);
		protected abstract string FormatPrimaryKey(ColumnDefinition column);
		protected abstract string FormatSystemMethods(SystemMethods systemMethod);

		public virtual string Generate( ColumnDefinition column )
		{
			var clauses = new List<string>();

			foreach ( var action in ClauseOrder )
			{
				string clause = action( column );
				if ( !string.IsNullOrEmpty( clause ) )
					clauses.Add( clause );
			}

			return string.Join( " ", clauses.ToArray() );
		}

		public string Generate(CreateTableExpression expression)
		{

			IList<ColumnDefinition> columns = expression.Columns;
			string result = "";
			int total = columns.Count - 1;

			//if more than one column is a primary key or the primary key is given a name, then it needs to be added separately
            IList<ColumnDefinition> primaryKeyColumns = GetPrimaryKeyColumns(columns);
			bool addPrimaryKeySeparately = false;
			if (primaryKeyColumns.Count > 1 || (primaryKeyColumns.Count == 1 && !string.IsNullOrEmpty(primaryKeyColumns[0].PrimaryKeyName)))
			{
                if(CanSeperatePrimaryKeyAndIdentity || primaryKeyColumns.All(x => !x.IsIdentity))
                {
                    addPrimaryKeySeparately = true;
                    foreach (ColumnDefinition column in primaryKeyColumns)
                    {
                        column.IsPrimaryKey = false;
                    }
                }
			}

			for (int i = 0; i < columns.Count; i++)
			{
				result += Generate(columns[i]);

				if (i != total)
					result += ", ";
			}

			if (addPrimaryKeySeparately)
				result = AddPrimaryKeyConstraint(expression.TableName, primaryKeyColumns, result);

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

		private string AddPrimaryKeyConstraint(string tableName, IList<ColumnDefinition> primaryKeyColumns, string result)
		{
			string keyColumns = "";
			foreach (ColumnDefinition column in primaryKeyColumns)
			{
				keyColumns += column.Name + ",";
			}
			keyColumns = keyColumns.TrimEnd(',');
			result += String.Format(", {0} PRIMARY KEY ({1})", GetPrimaryKeyConstraintName(primaryKeyColumns, tableName), keyColumns);

			return result;
		}

		/// <summary>
		/// Gets the name of the primary key constraint. Some Generators may need to override if the constraint name is limited
		/// </summary>
		/// <returns></returns>
		protected virtual string GetPrimaryKeyConstraintName(IList<ColumnDefinition> primaryKeyColumns, string tableName)
		{
			string keyName = string.Empty;
			string assignedName = string.Empty;
			foreach (ColumnDefinition column in primaryKeyColumns)
			{
				keyName += column.Name + "_";

				if (!string.IsNullOrEmpty(column.PrimaryKeyName))
				{
					assignedName = column.PrimaryKeyName;
					break;
				}
			}

			keyName += "PK";

			if (!string.IsNullOrEmpty(assignedName))
			{
				keyName = assignedName;
			}

			return string.Format("CONSTRAINT {0}", keyName);
		}
	}
}
