using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	public class OracleGenerator : GeneratorBase
	{
		public const int AnsiStringCapacity = 2000;
		public const int AnsiTextCapacity = 2147483647;
		public const int UnicodeStringCapacity = 2000;
		public const int UnicodeTextCapacity = int.MaxValue;
		public const int BlobCapacity = 2147483647;
		public const int DecimalCapacity = 19;
		public const int XmlCapacity = 1073741823;

		protected override void SetupTypeMaps()
		{
			SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255)");
			SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size)", AnsiStringCapacity);
			SetTypeMap(DbType.AnsiString, "VARCHAR2(255)");
			SetTypeMap(DbType.AnsiString, "VARCHAR2($size)", AnsiStringCapacity);
			SetTypeMap(DbType.AnsiString, "CLOB", AnsiTextCapacity);
			SetTypeMap(DbType.Binary, "RAW(2000)");
			SetTypeMap(DbType.Binary, "RAW($size)", AnsiStringCapacity);
			SetTypeMap(DbType.Binary, "RAW(MAX)", AnsiTextCapacity);
			SetTypeMap(DbType.Binary, "BLOB", BlobCapacity);
			SetTypeMap(DbType.Boolean, "NUMBER(1,0)");
			SetTypeMap(DbType.Byte, "NUMBER(3,0)");
			SetTypeMap(DbType.Currency, "NUMBER(19,1)");
			SetTypeMap(DbType.Date, "DATE");
			SetTypeMap(DbType.DateTime, "TIMESTAMP(4)");
			SetTypeMap(DbType.Decimal, "NUMBER(19,5)");
			SetTypeMap(DbType.Decimal, "NUMBER($size,$precision)", DecimalCapacity);
			SetTypeMap(DbType.Double, "DOUBLE PRECISION");
			SetTypeMap(DbType.Guid, "RAW(16)");
			SetTypeMap(DbType.Int16, "NUMBER(5,0)");
			SetTypeMap(DbType.Int32, "NUMBER(10,0)");
			SetTypeMap(DbType.Int64, "NUMBER(20,0)");
			SetTypeMap(DbType.Single, "FLOAT(24)");
			SetTypeMap(DbType.StringFixedLength, "NCHAR(255)");
			SetTypeMap(DbType.StringFixedLength, "NCHAR($size)", UnicodeStringCapacity);
			SetTypeMap(DbType.String, "NVARCHAR2(255)");
			SetTypeMap(DbType.String, "NVARCHAR2($size)", UnicodeStringCapacity);
			SetTypeMap(DbType.String, "NCLOB", UnicodeTextCapacity);
			SetTypeMap(DbType.Time, "DATE");
			SetTypeMap(DbType.Xml, "XMLTYPE");
		}


		public override string GenerateDDLForColumn(ColumnDefinition column)
		{
			var sb = new StringBuilder();

			sb.Append(column.Name);
			sb.Append(" ");
			sb.Append(GetTypeMap(column.Type.Value, column.Size, column.Precision));

			//Oracle requires Default before Not null
			if (column.DefaultValue != null)
			{
				sb.Append(" DEFAULT ");
				sb.Append(GetConstantValue(column.DefaultValue));
			}

			if (!column.IsNullable)
			{
				sb.Append(" NOT NULL");
			}

			if (column.IsIdentity)
			{
				//todo: would like to throw a warning here
			}

			if (column.IsPrimaryKey)
			{
				sb.Append(" PRIMARY KEY");
			}

			return sb.ToString();
		}

		/// <summary>
		/// Returns empty string as the constraint for Primary Key. 
		/// Oracle will generate a coinstraint name if none is specified ie. SYS_C008004
		/// Oracle is limited to 30 chars and the constraints must be unique for the enire schema
		/// so there is no way to get an intelligent name using table and column names
		/// </summary>
		/// <param name="primaryKeyColumns"></param>
		/// <param name="tableName"></param>
		/// <returns></returns>
		protected override string GetPrimaryKeyConstraintName(IList<ColumnDefinition> primaryKeyColumns, string tableName)
		{
			return string.Empty;
		}

		public override string Generate(CreateSchemaExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(DeleteSchemaExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(CreateTableExpression expression)
		{
			return FormatExpression("CREATE TABLE {0} ({1})", expression.TableName, GetColumnDDL(expression));
		}

		public override string Generate(CreateColumnExpression expression)
		{
			return FormatExpression("ALTER TABLE {0} ADD {1}", expression.TableName, GenerateDDLForColumn(expression.Column));
		}

		public override string Generate(DeleteTableExpression expression)
		{
			return FormatExpression("DROP TABLE {0}", expression.TableName);
		}

		public override string Generate(DeleteColumnExpression expression)
		{

			return FormatExpression("ALTER TABLE {0} DROP COLUMN {1}", expression.TableName, expression.ColumnName);
		}

		public override string Generate(CreateForeignKeyExpression expression)
		{
			string primaryColumns = GetColumnList(expression.ForeignKey.PrimaryColumns);
			string foreignColumns = GetColumnList(expression.ForeignKey.ForeignColumns);

			string sql = "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4})";

			return String.Format(sql,
						  expression.ForeignKey.ForeignTable,
						  expression.ForeignKey.Name,
						  foreignColumns,
						  expression.ForeignKey.PrimaryTable,
						  primaryColumns
						  );
		}

		public override string Generate(DeleteForeignKeyExpression expression)
		{
			string sql = "ALTER TABLE {0} DROP CONSTRAINT {1}";
			return String.Format(sql, expression.ForeignKey.PrimaryTable, expression.ForeignKey.Name);
		}

		public override string Generate(CreateIndexExpression expression)
		{
			var result = new StringBuilder("CREATE");
			if (expression.Index.IsUnique)
				result.Append(" UNIQUE");

			//if (expression.Index.IsClustered)
			//    result.Append(" CLUSTERED");
			//else
			//    result.Append(" NONCLUSTERED");

			result.Append(" INDEX {0} ON {1} (");

			bool first = true;
			foreach (IndexColumnDefinition column in expression.Index.Columns)
			{
				if (first)
					first = false;
				else
					result.Append(",");

				result.Append(column.Name);
				if (column.Direction == Direction.Ascending)
				{
					result.Append(" ASC");
				}
				else
				{
					result.Append(" DESC");
				}
			}
			result.Append(")");

			return FormatExpression(result.ToString(), expression.Index.Name, expression.Index.TableName);
		}

		public override string Generate(DeleteIndexExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(RenameTableExpression expression)
		{
			return FormatExpression("ALTER TABLE {0} RENAME TO {1}", expression.OldName, expression.NewName);
		}

		public override string Generate(RenameColumnExpression expression)
		{
			return FormatExpression("ALTER TABLE {0} RENAME COLUMN {1} TO {2}", expression.TableName, expression.OldName, expression.NewName);
		}

		public override string Generate(InsertDataExpression expression)
		{
			var result = new StringBuilder();
			foreach (InsertionDataDefinition row in expression.Rows)
			{
				List<string> columnNames = new List<string>();
				List<object> columnData = new List<object>();
				foreach (KeyValuePair<string, object> item in row)
				{
					columnNames.Add(item.Key);
					columnData.Add(item.Value);
				}

				string columns = GetColumnList(columnNames);
				string data = GetDataList(columnData);
				result.Append(FormatExpression(" INTO {0} ({1}) VALUES ({2})", expression.TableName, columns, data));
			}
			return "INSERT ALL" + result.ToString() + " SELECT 1 FROM DUAL";
		}

		public string FormatExpression(string template, params object[] args)
		{
			return String.Format(template, args);
		}

		private string GetColumnList(IEnumerable<string> columns)
		{
			string result = "";
			foreach (string column in columns)
			{
				result += column + ",";
			}
			return result.TrimEnd(',');
		}

		private string GetDataList(List<object> data)
		{
			string result = "";
			foreach (object column in data)
			{
				result += GetConstantValue(column) + ",";
			}
			return result.TrimEnd(',');
		}
	}
}
