using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	public class JetGenerator : GeneratorBase
	{
		public JetGenerator() : base(new JetColumn(), new ConstantFormatter())
		{
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
			return String.Format("CREATE TABLE [{0}] ({1})", expression.TableName, Column.Generate(expression));
		}

		public override string Generate(AlterColumnExpression expression)
		{
			return String.Format("ALTER TABLE [{0}] ALTER COLUMN {1}", expression.TableName, Column.Generate(expression.Column));
		}

		public override string Generate(CreateColumnExpression expression)
		{
			return String.Format("ALTER TABLE [{0}] ADD COLUMN {1}", expression.TableName, Column.Generate(expression.Column));
		}

		public override string Generate(DeleteTableExpression expression)
		{
			return String.Format("DROP TABLE [{0}]", expression.TableName);
		}

		public override string Generate(DeleteColumnExpression expression)
		{
			return String.Format("ALTER TABLE [{0}] DROP COLUMN {1}", expression.TableName, expression.ColumnName);
		}

		public override string Generate(CreateForeignKeyExpression expression)
		{
			var primaryColumns = GetColumnList(expression.ForeignKey.PrimaryColumns);
			var foreignColumns = GetColumnList(expression.ForeignKey.ForeignColumns);

			const string sql = "ALTER TABLE [{0}] ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES [{3}] ({4}){5}{6}";

			return string.Format(
				sql,
				expression.ForeignKey.ForeignTable,
				expression.ForeignKey.Name,
				foreignColumns,
				expression.ForeignKey.PrimaryTable,
				primaryColumns,
				FormatCascade("DELETE", expression.ForeignKey.OnDelete),
				FormatCascade("UPDATE", expression.ForeignKey.OnUpdate)
				);
		}

		public override string Generate(DeleteForeignKeyExpression expression)
		{
			string sql = "ALTER TABLE [{0}] DROP CONSTRAINT {1}";
			return String.Format(sql, expression.ForeignKey.ForeignTable, expression.ForeignKey.Name);
		}

		public override string Generate(CreateIndexExpression expression)
		{
			var result = new StringBuilder("CREATE");
			if (expression.Index.IsUnique)
				result.Append(" UNIQUE");

			result.Append(" INDEX {0} ON [{1}] (");

			var first = true;
			foreach (var column in expression.Index.Columns)
			{
				if (first)
					first = false;
				else
					result.Append(",");

				result.Append(column.Name);
				result.Append(column.Direction == Direction.Ascending ? " ASC" : " DESC");
			}
			result.Append(")");

			return String.Format(result.ToString(), expression.Index.Name, expression.Index.TableName);
		}

		public override string Generate(DeleteIndexExpression expression)
		{
			return String.Format("DROP INDEX {0} ON [{1}]", expression.Index.Name, expression.Index.TableName);
		}

		public override string Generate(RenameTableExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(RenameColumnExpression expression)
		{
			throw new NotImplementedException();
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
				result.Append(String.Format("INSERT INTO [{0}] ({1}) VALUES ({2});", expression.TableName, columns, data));
			}
			return result.ToString();
		}

		public override string Generate(AlterDefaultConstraintExpression expression)
		{
			throw new NotImplementedException();
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

					where += String.Format("[{0}] {1} {2}", item.Key, item.Value == null ? "IS" : "=", Constant.Format(item.Value));
					i++;
				}

				result.Append(String.Format("DELETE FROM [{0}] WHERE {1};", expression.TableName, where));
			}
			return result.ToString();
		}

		public override string Generate(UpdateDataExpression expression)
		{
			var set = new StringBuilder();
			int i = 0;

			foreach (var item in expression.Set)
			{
				if (i != 0)
				{
					set.Append(", ");
				}
				set.AppendFormat("[{0}] = {1}", item.Key, Constant.Format(item.Value));
				i++;
			}

			var where = new StringBuilder();
			i = 0;

			foreach (var item in expression.Where)
			{
				if (i != 0)
				{
					where.Append(" AND ");
				}
				where.AppendFormat("[{0}] {1} {2}", item.Key, item.Value == null ? "IS" : "=", Constant.Format(item.Value));
				i++;
			}

			return String.Format("UPDATE [{0}] SET {1} WHERE {2}", expression.TableName, set, where);
		}

		private string GetColumnList(IEnumerable<string> columns)
		{
			string result = string.Empty;
			foreach (string column in columns)
			{
				result += "[" + column + "],";
			}
			return result.TrimEnd(',');
		}

		private string GetDataList(List<object> data)
		{
			var result = string.Empty;
			foreach (var column in data)
			{
				result += Constant.Format(column) + ",";
			}
			return result.TrimEnd(',');
		}

		private string FormatCascade(string onWhat, Rule rule)
		{
			string action;
			switch (rule)
			{
				case Rule.None:
					return string.Empty;
				case Rule.Cascade:
					action = "CASCADE";
					break;
				case Rule.SetNull:
					action = "SET NULL";
					break;
				case Rule.SetDefault:
					// not supported
					throw new NotImplementedException();
				default:
					throw new ArgumentException("Unknown Rule: " + rule);
			}

			return String.Format(" ON {0} {1}", onWhat, action);
		}
	}
}
