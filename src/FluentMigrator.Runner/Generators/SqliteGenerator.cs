#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2010, Nathan Brown
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
using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	public class SqliteGenerator : GeneratorBase
	{
		public SqliteGenerator() : base(new SqliteTypeMap())		{
		}

		protected override string GenerateDDLForColumn(ColumnDefinition column)
		{
			var sb = new StringBuilder();

			sb.Append(column.Name);
			sb.Append(" ");

			if (!column.IsIdentity)
			{
				if (column.Type.HasValue)
				{
					sb.Append(GetTypeMap(column.Type.Value, column.Size, column.Precision));
				}
				else
				{
					sb.Append(column.CustomType);
				}
			}
			else
			{
				sb.Append(GetTypeMap(DbType.Int32, column.Size, column.Precision));
			}

			if (!column.IsNullable)
			{
				sb.Append(" NOT NULL");
			}

			if (column.DefaultValue != null)
			{
				sb.Append(" DEFAULT ");
				sb.Append(Constant.Format(column.DefaultValue));
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
            throw new NotImplementedException();
        }

		public override string Generate(CreateTableExpression expression)
		{
			return string.Format("CREATE TABLE {0} ({1})", expression.TableName, GetColumnDDL(expression));
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
			return String.Format("ALTER TABLE [{0}] ADD COLUMN {1}", expression.TableName, GenerateDDLForColumn(expression.Column));
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

        public override string Generate(DeleteDataExpression expression)
        {
            var result = new StringBuilder();

            if (expression.IsAllRows)
            {
                result.Append(String.Format("DELETE FROM {0};", expression.TableName));
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

                        where += String.Format("[{0}] = {1}", item.Key, Constant.Format(item.Value));
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
			}
			result.Append(")");

			return String.Format(result.ToString(), expression.Index.Name, expression.Index.TableName);
		}

		public override string Generate(DeleteIndexExpression expression)
		{
			throw new NotImplementedException();
		}
	}
}
