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
		public OracleGenerator() : base(new OracleTypeMap())
		{
		}

		protected override string GenerateDDLForColumn(ColumnDefinition column)
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

        public override string Generate(AlterColumnExpression expression)
        {
            return String.Format("ALTER TABLE {0} MODIFY {1}", expression.TableName, GenerateDDLForColumn(expression.Column));
        }

		public override string Generate(CreateTableExpression expression)
		{
			return String.Format("CREATE TABLE {0} ({1})", expression.TableName, GetColumnDDL(expression));
		}

		public override string Generate(CreateColumnExpression expression)
		{
			return String.Format("ALTER TABLE {0} ADD {1}", expression.TableName, GenerateDDLForColumn(expression.Column));
		}

		public override string Generate(DeleteTableExpression expression)
		{
			return String.Format("DROP TABLE {0}", expression.TableName);
		}

		public override string Generate(DeleteColumnExpression expression)
		{

			return String.Format("ALTER TABLE {0} DROP COLUMN {1}", expression.TableName, expression.ColumnName);
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

			return String.Format(result.ToString(), expression.Index.Name, expression.Index.TableName);
		}

		public override string Generate(DeleteIndexExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(RenameTableExpression expression)
		{
			return String.Format("ALTER TABLE {0} RENAME TO {1}", expression.OldName, expression.NewName);
		}

		public override string Generate(RenameColumnExpression expression)
		{
			return String.Format("ALTER TABLE {0} RENAME COLUMN {1} TO {2}", expression.TableName, expression.OldName, expression.NewName);
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
				result.Append(String.Format(" INTO {0} ({1}) VALUES ({2})", expression.TableName, columns, data));
			}
			return "INSERT ALL" + result.ToString() + " SELECT 1 FROM DUAL";
		}

        public override string Generate(DeleteDataExpression expression)
        {
            var result = new StringBuilder();
            foreach (var row in expression.Rows)
            {
                var where = String.Empty;
                var i = 0;

                foreach (var item in row)
                {
                    if (i != 0)
                    {
                        where += " AND ";
                    }

                    where += String.Format("[{0}] = {1}", item.Key, GetConstantValue(item.Value));
                    i++;
                }

                result.Append(String.Format("DELETE FROM {0} WHERE {1};", expression.TableName, where));
            }
            return result.ToString();
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            throw new NotImplementedException();
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
