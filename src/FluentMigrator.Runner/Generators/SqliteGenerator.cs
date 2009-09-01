using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Builders.Insert;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	public class SqliteGenerator : GeneratorBase
	{
		public const int AnsiStringCapacity = 8000;
		public const int AnsiTextCapacity = 2147483647;
		public const int UnicodeStringCapacity = 4000;
		public const int UnicodeTextCapacity = 1073741823;
		public const int ImageCapacity = 2147483647;
		public const int DecimalCapacity = 19;
		public const int XmlCapacity = 1073741823;

		protected override void SetupTypeMaps()
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
			SetTypeMap(DbType.Int32, "INTEGER");
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

		public override string GenerateDDLForColumn(ColumnDefinition column)
		{
			var sb = new StringBuilder();

			sb.Append(column.Name);
			sb.Append(" ");
			sb.Append(GetTypeMap(column.Type.Value, column.Size, column.Precision));

			if (!column.IsNullable)
			{
				sb.Append(" NOT NULL");
			}

			if (column.DefaultValue != null)
			{
				sb.Append(" DEFAULT ");
				sb.Append(GetConstantValue(column.DefaultValue));
			}

			if (column.IsIdentity)
			{
				sb.Append(" IDENTITY");
			}

			if (column.IsPrimaryKey)
			{
				sb.Append(" PRIMARY KEY");
			}

			//Assume that if its IDENTITY and PRIMARY KEY, the it should be an AUTOINCREMENT column
			sb.Replace(" IDENTITY PRIMARY KEY", " PRIMARY KEY AUTOINCREMENT");

			return sb.ToString();
		}

		public override string Generate(CreateTableExpression expression)
		{
			return string.Format("CREATE TABLE {0} ({1})", expression.TableName, GetColumnDDL(expression.Columns));
		}

		public override string Generate(RenameTableExpression expression)
		{
			return string.Format("ALTER TABLE {0} RENAME TO {1}", expression.OldName, expression.NewName);
		}

		public override string Generate(DeleteTableExpression expression)
		{
			return string.Format("DROP TABLE {0}", expression.TableName);
		}

		public override string Generate(CreateColumnExpression expression)
		{
			//return string.Format("ALTER TABLE {0} ADD COLUMN {1}", expression.TableName, expression.Column.Name);
			return FormatExpression("ALTER TABLE [{0}] ADD COLUMN {1}", expression.TableName, GenerateDDLForColumn(expression.Column));
		}

		public override string Generate(RenameColumnExpression expression)
		{
			throw new System.NotImplementedException();
		}

	    public override string Generate(InsertDataExpression expression)
	    {
            var result = new StringBuilder();
            foreach (InsertionData row in expression.Rows)
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
                result.Append(FormatExpression("INSERT INTO [{0}] ({1}) VALUES ({2});", expression.TableName, columns, data));
            }
            return result.ToString();
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

	    public override string Generate(DeleteColumnExpression expression)
		{
			return string.Format("ALTER TABLE {0} DROP COLUMN {1}", expression.TableName, expression.ColumnName);
		}

		public override string Generate(CreateForeignKeyExpression expression)
		{
            // Ignore foreign keys for SQLite
		    return "";
		}

		public override string Generate(DeleteForeignKeyExpression expression)
		{
		    return "";
		}

		public override string Generate(CreateIndexExpression expression)
		{
			var result = new StringBuilder("CREATE");
			if (expression.Index.IsUnique)
				result.Append(" UNIQUE");

			result.Append(" INDEX IF NOT EXISTS {0} ON {1} (");

			bool first = true;
			foreach (IndexColumnDefinition column in expression.Index.Columns)
			{
				if (first)
					first = false;
				else
					result.Append(",");

				result.Append(column.Name);

				// Not yet implemented correctly
				//				if (column.Direction == Direction.Ascending)
				//					result.Append(" ASC");
				//				else
				//					result.Append(" DESC");
			}
			result.Append(")");

			return FormatExpression(result.ToString(), expression.Index.Name, expression.Index.TableName);
		}

		public override string Generate(DeleteIndexExpression expression)
		{
			throw new NotImplementedException();
		}
	}
}
