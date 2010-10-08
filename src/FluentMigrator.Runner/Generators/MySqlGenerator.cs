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
using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	public class MySqlGenerator : GeneratorBase
	{
		public const int AnsiTinyStringCapacity = 127;
		public const int StringCapacity = 255;
		public const int TextCapacity = 65535;
		public const int MediumTextCapacity = 16777215;
		public const int DecimalCapacity = 19;

		protected override void SetupTypeMaps()
		{
			SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255)");
			SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size)", StringCapacity);
			SetTypeMap(DbType.AnsiStringFixedLength, "TEXT", TextCapacity);
			SetTypeMap(DbType.AnsiStringFixedLength, "MEDIUMTEXT", MediumTextCapacity);
			SetTypeMap(DbType.AnsiString, "VARCHAR(255)");
			SetTypeMap(DbType.AnsiString, "VARCHAR($size)", StringCapacity);
			SetTypeMap(DbType.AnsiString, "TEXT", TextCapacity);
			SetTypeMap(DbType.AnsiString, "MEDIUMTEXT", MediumTextCapacity);
			SetTypeMap(DbType.Binary, "LONGBLOB");
			SetTypeMap(DbType.Binary, "TINYBLOB", AnsiTinyStringCapacity);
			SetTypeMap(DbType.Binary, "BLOB", TextCapacity);
			SetTypeMap(DbType.Binary, "MEDIUMBLOB", MediumTextCapacity);
			SetTypeMap(DbType.Boolean, "TINYINT(1)");
			SetTypeMap(DbType.Byte, "TINYINT UNSIGNED");
			SetTypeMap(DbType.Currency, "MONEY");
			SetTypeMap(DbType.Date, "DATE");
			SetTypeMap(DbType.DateTime, "DATETIME");
			SetTypeMap(DbType.Decimal, "DECIMAL(19,5)");
			SetTypeMap(DbType.Decimal, "DECIMAL($size,$precision)", DecimalCapacity);
			SetTypeMap(DbType.Double, "DOUBLE");
			SetTypeMap(DbType.Guid, "VARCHAR(40)");
			SetTypeMap(DbType.Int16, "SMALLINT");
			SetTypeMap(DbType.Int32, "INTEGER");
			SetTypeMap(DbType.Int64, "BIGINT");
			SetTypeMap(DbType.Single, "FLOAT");
			SetTypeMap(DbType.StringFixedLength, "CHAR(255)");
			SetTypeMap(DbType.StringFixedLength, "CHAR($size)", StringCapacity);
			SetTypeMap(DbType.StringFixedLength, "TEXT", TextCapacity);
			SetTypeMap(DbType.StringFixedLength, "MEDIUMTEXT", MediumTextCapacity);
			SetTypeMap(DbType.String, "VARCHAR(255)");
			SetTypeMap(DbType.String, "VARCHAR($size)", StringCapacity);
			SetTypeMap(DbType.String, "TEXT", TextCapacity);
			SetTypeMap(DbType.String, "MEDIUMTEXT", MediumTextCapacity);
			SetTypeMap(DbType.Time, "DATETIME");
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
			return FormatExpression("CREATE TABLE `{0}` ({1}) ENGINE = INNODB", expression.TableName, GetColumnDDL(expression));
		}

        public override string Generate(AlterColumnExpression expression)
        {
            return FormatExpression("ALTER TABLE {0} MODIFY {1}", expression.TableName, GenerateDDLForColumn(expression.Column));
        }

		public override string Generate(CreateColumnExpression expression)
		{

			return FormatExpression("ALTER TABLE `{0}` ADD {1}", expression.TableName, GenerateDDLForColumn(expression.Column));
		}

		public override string Generate(DeleteTableExpression expression)
		{
			return FormatExpression("DROP TABLE `{0}`", expression.TableName);
		}

		public override string Generate(DeleteColumnExpression expression)
		{
			return FormatExpression("ALTER TABLE `{0}` DROP COLUMN {1}", expression.TableName, expression.ColumnName);
		}

		public override string Generate(CreateForeignKeyExpression expression)
		{
			string primaryColumns = GetColumnList(expression.ForeignKey.PrimaryColumns);
			string foreignColumns = GetColumnList(expression.ForeignKey.ForeignColumns);

			string sql = "ALTER TABLE `{0}` ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4})";

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
			string sql = "ALTER TABLE `{0}` DROP FOREIGN KEY `{1}`";
			return String.Format(sql, expression.ForeignKey.PrimaryTable, expression.ForeignKey.Name);
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

			return FormatExpression(result.ToString(), expression.Index.Name, expression.Index.TableName);
		}

		public override string Generate(DeleteIndexExpression expression)
		{
			return FormatExpression("DROP INDEX {0}", expression.Index.Name, expression.Index.TableName);
		}

		public override string Generate(RenameTableExpression expression)
		{
			return FormatExpression("RENAME TABLE `{0}` TO `{1}`", expression.OldName, expression.NewName);
		}

		public override string Generate(RenameColumnExpression expression)
		{
			// may need to add definition to end. blerg
			//return FormatExpression("ALTER TABLE `{0}` CHANGE COLUMN {1} {2}", expression.TableName, expression.OldName, expression.NewName);
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
				result.Append(FormatExpression("INSERT INTO `{0}` ({1}) VALUES ({2});", expression.TableName, columns, data));
			}
			return result.ToString();
		}

        public override string Generate(DeleteDataExpression expression)
        {
            var result = new StringBuilder();

            if (expression.IsAllRows)
            {
                result.Append(FormatExpression("DELETE FROM {0};", expression.TableName));
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

                        where += String.Format("[{0}] = {1}", item.Key, GetConstantValue(item.Value));
                        i++;
                    }

                    result.Append(FormatExpression("DELETE FROM {0} WHERE {1};", expression.TableName, where));
                }
            }

            return result.ToString();
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            throw new NotImplementedException();
        }

		public override string GenerateDDLForColumn(ColumnDefinition column)
		{
			var sb = new StringBuilder();

			sb.Append(column.Name);
			sb.Append(" ");

			if (column.Type.HasValue)
			{
				sb.Append(GetTypeMap(column.Type.Value, column.Size, column.Precision));
			}
			else
			{
				sb.Append(column.CustomType);
			}

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
				sb.Append(" AUTO_INCREMENT");
			}

			if (column.IsPrimaryKey)
			{
				sb.Append(string.Format(", PRIMARY KEY (`{0}`)", column.Name));
			}

			return sb.ToString();
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
