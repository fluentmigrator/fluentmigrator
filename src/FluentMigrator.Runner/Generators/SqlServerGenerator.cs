using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	public class SqlServerGenerator : GeneratorBase
	{
		public const int AnsiStringCapacity = 8000;
		public const int AnsiTextCapacity = 2147483647;
		public const int UnicodeStringCapacity = 4000;
		public const int UnicodeTextCapacity = 1073741823;
		public const int ImageCapacity = 2147483647;
		public const int DecimalCapacity = 19;
		public const int XmlCapacity = 1073741823;

		public SqlServerGenerator()
		{
			SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255)");
			SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size)", AnsiStringCapacity);
			SetTypeMap(DbType.AnsiString, "VARCHAR(255)");
			SetTypeMap(DbType.AnsiString, "VARCHAR($size)", AnsiStringCapacity);
			SetTypeMap(DbType.AnsiString, "TEXT", AnsiTextCapacity);
			SetTypeMap(DbType.Binary, "VARBINARY(8000)");
			SetTypeMap(DbType.Binary, "VARBINARY($size)", AnsiStringCapacity);
			SetTypeMap(DbType.Binary, "IMAGE", ImageCapacity);
			SetTypeMap(DbType.Boolean, "BIT");
			SetTypeMap(DbType.Byte, "TINYINT");
			SetTypeMap(DbType.Currency, "MONEY");
			SetTypeMap(DbType.Date, "DATETIME");
			SetTypeMap(DbType.DateTime, "DATETIME");
			SetTypeMap(DbType.Decimal, "DECIMAL(19,5)");
			SetTypeMap(DbType.Decimal, "DECIMAL(19,$size)", DecimalCapacity);
			SetTypeMap(DbType.Double, "DOUBLE PRECISION");
			SetTypeMap(DbType.Guid, "UNIQUEIDENTIFIER");
			SetTypeMap(DbType.Int16, "SMALLINT");
			SetTypeMap(DbType.Int32, "INT");
			SetTypeMap(DbType.Int64, "BIGINT");
			SetTypeMap(DbType.Single, "REAL");
			SetTypeMap(DbType.StringFixedLength, "NCHAR(255)");
			SetTypeMap(DbType.StringFixedLength, "NCHAR($size)", UnicodeStringCapacity);
			SetTypeMap(DbType.String, "NVARCHAR(255)");
			SetTypeMap(DbType.String, "NVARCHAR($size)", UnicodeStringCapacity);
			SetTypeMap(DbType.String, "NTEXT", UnicodeTextCapacity);
			SetTypeMap(DbType.Time, "DATETIME");
			SetTypeMap(DbType.Xml, "XML", XmlCapacity);
		}

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

		private string GetColumnDDL(IList<ColumnDefinition> columns)
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

		public override string Generate(CreateTableExpression expression)
		{
			return FormatExpression("CREATE TABLE [{0}] ({1})", expression.TableName, GetColumnDDL(expression.Columns));
		}

		public override string Generate(CreateColumnExpression expression)
		{
		    return FormatExpression("ALTER TABLE [{0}] ADD {1}", expression.TableName, GenerateDDLForColumn(expression.Column));
		}

		public override string Generate(DeleteTableExpression expression)
		{
			return FormatExpression("DROP TABLE [{0}]", expression.TableName);
		}

		public override string Generate(DeleteColumnExpression expression)
		{

		    return FormatExpression("ALTER TABLE [{0}] DROP COLUMN {1}", expression.TableName, expression.ColumnName);            
		}

		public override string Generate(CreateForeignKeyExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(DeleteForeignKeyExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(CreateIndexExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(DeleteIndexExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(RenameTableExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(RenameColumnExpression expression)
		{
			throw new NotImplementedException();
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

		public string FormatExpression(string template, params object[] args)
		{
			return String.Format(template, args);
		}
	}
}