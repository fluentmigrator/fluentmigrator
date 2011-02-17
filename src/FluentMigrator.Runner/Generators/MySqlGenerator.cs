#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	public class MySqlGenerator : GeneratorBase
	{
		public MySqlGenerator() : base(new MySqlColumn(), new ConstantFormatterWithQuotedBackslashes())
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

		public override string Generate(AlterSchemaExpression expression)
		{
		  throw new NotImplementedException();
		}

		public override string Generate(CreateTableExpression expression)
		{
			return String.Format("CREATE TABLE `{0}` ({1}) ENGINE = INNODB", expression.TableName, Column.Generate(expression));
		}

		public override string Generate(AlterColumnExpression expression)
		{
			return String.Format("ALTER TABLE {0} MODIFY {1}", expression.TableName, Column.Generate(expression.Column));
		}

		public override string Generate(CreateColumnExpression expression)
		{

			return String.Format("ALTER TABLE `{0}` ADD {1}", expression.TableName, Column.Generate(expression.Column));
		}

		public override string Generate(DeleteTableExpression expression)
		{
			return String.Format("DROP TABLE `{0}`", expression.TableName);
		}

		public override string Generate(DeleteColumnExpression expression)
		{
			return String.Format("ALTER TABLE `{0}` DROP COLUMN {1}", expression.TableName, expression.ColumnName);
		}

		public override string Generate(CreateForeignKeyExpression expression)
		{
			string primaryColumns = GetColumnList(expression.ForeignKey.PrimaryColumns);
			string foreignColumns = GetColumnList(expression.ForeignKey.ForeignColumns);

			string sql = "ALTER TABLE `{0}` ADD CONSTRAINT `{1}` FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6}";

			return String.Format(sql,
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
			string sql = "ALTER TABLE `{0}` DROP FOREIGN KEY `{1}`";
			return String.Format(sql, expression.ForeignKey.ForeignTable, expression.ForeignKey.Name);
		}

		public override string Generate(CreateIndexExpression expression)
		{
			var result = new StringBuilder("CREATE");
			if (expression.Index.IsUnique)
				result.Append(" UNIQUE");

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
			return String.Format("DROP INDEX {0}", expression.Index.Name, expression.Index.TableName);
		}

		public override string Generate(RenameTableExpression expression)
		{
			return String.Format("RENAME TABLE `{0}` TO `{1}`", expression.OldName, expression.NewName);
		}

		public override string Generate(RenameColumnExpression expression)
		{
			// may need to add definition to end. blerg
			//return String.Format("ALTER TABLE `{0}` CHANGE COLUMN {1} {2}", expression.TableName, expression.OldName, expression.NewName);
			
			// NOTE: The above does not work, as the CHANGE COLUMN syntax in Mysql requires the column definition to be re-specified,
			// even if it has not changed; so marking this as not working for now
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
				result.Append(String.Format("INSERT INTO `{0}` ({1}) VALUES ({2});", expression.TableName, columns, data));
			}
			return result.ToString();
		}

        public override string Generate(UpdateDataExpression expression)
        {
            var result = new StringBuilder();

            var set = String.Empty;
            var i = 0;
            foreach (var item in expression.Set)
            {
                if (i != 0)
                {
                    set += ", ";
                }

                set += String.Format("`{0}` = {1}", item.Key, Constant.Format(item.Value));
                i++;
            }

            var where = String.Empty;
            i = 0;
            foreach (var item in expression.Where)
            {
                if (i != 0)
                {
                    where += " AND ";
                }

                where += String.Format("`{0}` {1} {2}", item.Key, item.Value == null ? "IS" : "=", Constant.Format(item.Value));
                i++;
            }

            result.Append(String.Format("UPDATE `{0}` SET `{1}` WHERE {2};", expression.TableName, set, where));

            return result.ToString();
        }

		public override string Generate(DeleteDataExpression expression)
		{
			var result = new StringBuilder();

			if (expression.IsAllRows)
			{
				result.Append(String.Format("DELETE FROM `{0}`;", expression.TableName));
			}
			else
			{
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

                        where += String.Format("`{0}` {1} {2}", item.Key, item.Value == null ? "IS" : "=", Constant.Format(item.Value));
						i++;
					}

					result.Append(String.Format("DELETE FROM {0} WHERE {1};", expression.TableName, where));
				}
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
				result += Constant.Format(column) + ",";
			}
			return result.TrimEnd(',');
		}

        protected string FormatCascade(string onWhat, Rule rule)
        {
            string action = "NO ACTION";
            switch (rule)
            {
                case Rule.None:
                    return "";
                case Rule.Cascade:
                    action = "CASCADE";
                    break;
                case Rule.SetNull:
                    action = "SET NULL";
                    break;
                case Rule.SetDefault:
                    action = "SET DEFAULT";
                    break;
            }

            return string.Format(" ON {0} {1}", onWhat, action);
        }
	}
}
