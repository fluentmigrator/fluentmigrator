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
	public class SqlServer2000Generator : GeneratorBase
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
			SetTypeMap(DbType.Binary, "VARBINARY(MAX)", int.MaxValue);
			SetTypeMap(DbType.Binary, "IMAGE", ImageCapacity);
			SetTypeMap(DbType.Boolean, "BIT");
			SetTypeMap(DbType.Byte, "TINYINT");
			SetTypeMap(DbType.Currency, "MONEY");
			SetTypeMap(DbType.Date, "DATETIME");
			SetTypeMap(DbType.DateTime, "DATETIME");
			SetTypeMap(DbType.Decimal, "DECIMAL(19,5)");
			SetTypeMap(DbType.Decimal, "DECIMAL($size,$precision)", DecimalCapacity);
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
			SetTypeMap(DbType.String, "NVARCHAR(MAX)", int.MaxValue);
			SetTypeMap(DbType.String, "NTEXT", UnicodeTextCapacity);
			SetTypeMap(DbType.Time, "DATETIME");
			SetTypeMap(DbType.Xml, "XML", XmlCapacity);
		}

		public override string Generate(CreateSchemaExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(DeleteSchemaExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(RenameTableExpression expression)
		{
			return FormatExpression("sp_rename {0}[{1}], [{2}]", FormatSchema(expression.SchemaName), expression.OldName, expression.NewName);
		}

		public override string Generate(RenameColumnExpression expression)
		{
			return FormatExpression("sp_rename '{0}[{1}].[{2}]', [{3}]", FormatSchema(expression.SchemaName, false), expression.TableName, expression.OldName, expression.NewName);
		}

		public override string Generate(CreateTableExpression expression)
		{
			return FormatExpression("CREATE TABLE {0}[{1}] ({2})", FormatSchema(expression.SchemaName), expression.TableName, GetColumnDDL(expression));
		}

		public override string Generate(DeleteTableExpression expression)
		{
			return FormatExpression("DROP TABLE {0}[{1}]", FormatSchema(expression.SchemaName), expression.TableName);
		}

		public override string Generate(CreateForeignKeyExpression expression)
		{
			var primaryColumns = GetColumnList(expression.ForeignKey.PrimaryColumns);
			var foreignColumns = GetColumnList(expression.ForeignKey.ForeignColumns);

			const string sql = "ALTER TABLE {0}[{1}] ADD CONSTRAINT {2} FOREIGN KEY ({3}) REFERENCES [{4}].[{5}] ({6})";

			return string.Format(sql,
								 FormatSchema(expression.ForeignKey.ForeignTableSchema),
								 expression.ForeignKey.ForeignTable,
								 expression.ForeignKey.Name,
								 foreignColumns,
								 FormatSchema(expression.ForeignKey.PrimaryTableSchema),
								 expression.ForeignKey.PrimaryTable,
								 primaryColumns
				);
		}

		public override string Generate(DeleteForeignKeyExpression expression)
		{
			const string sql = "ALTER TABLE {0}[{1}] DROP CONSTRAINT {2}";
			return string.Format(sql, FormatSchema(expression.ForeignKey.PrimaryTableSchema), expression.ForeignKey.PrimaryTable, expression.ForeignKey.Name);
		}

		public override string Generate(CreateColumnExpression expression)
		{
			return FormatExpression("ALTER TABLE {0}[{1}] ADD {2}", FormatSchema(expression.SchemaName), expression.TableName, GenerateDDLForColumn(expression.Column));
		}

		public override string Generate(DeleteColumnExpression expression)
		{
			// before we drop a column, we have to drop any default value constraints in SQL Server
			const string sql = @"
			DECLARE @default sysname, @sql nvarchar(max);

			-- get name of default constraint
			SELECT @default = name
			FROM sys.default_constraints 
			WHERE parent_object_id = object_id('{1}{2}')
			AND type = 'D'
			AND parent_column_id = (
				SELECT column_id 
				FROM sys.columns 
				WHERE object_id = object_id('{1}{2}')
				AND name = '{3}'
			);

			-- create alter table command as string and run it
			SET @sql = N'ALTER TABLE {0}[{2}] DROP CONSTRAINT ' + @default;
			EXEC sp_executesql @sql;

			-- now we can finally drop column
			ALTER TABLE {0}[{2}] DROP COLUMN [{3}];";

			return FormatExpression(sql, FormatSchema(expression.SchemaName), FormatSchema(expression.SchemaName, false), expression.TableName, expression.ColumnName);
		}

		public override string Generate(CreateIndexExpression expression)
		{
			var result = new StringBuilder("CREATE");
			if (expression.Index.IsUnique)
				result.Append(" UNIQUE");

			result.Append(expression.Index.IsClustered ? " CLUSTERED" : " NONCLUSTERED");

			result.Append(" INDEX {0} ON {1}[{2}] (");

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

			return FormatExpression(result.ToString(), expression.Index.Name, FormatSchema(expression.Index.SchemaName), expression.Index.TableName);
		}

		public override string Generate(DeleteIndexExpression expression)
		{
			return FormatExpression("DROP INDEX {0}[{1}] ON [{2}]", FormatSchema(expression.Index.SchemaName), expression.Index.Name, expression.Index.TableName);
		}

		public override string Generate(InsertDataExpression expression)
		{
			var result = new StringBuilder();
			foreach (var row in expression.Rows)
			{
				var columnNames = new List<string>();
				var columnData = new List<object>();
				foreach (var item in row)
				{
					columnNames.Add(item.Key);
					columnData.Add(item.Value);
				}

				var columns = GetColumnList(columnNames);
				var data = GetDataList(columnData);
				result.Append(FormatExpression("INSERT INTO {0}[{1}] ({2}) VALUES ({3});", FormatSchema(expression.SchemaName), expression.TableName, columns, data));
			}
			return result.ToString();
		}

		protected string FormatSchema(string schemaName)
		{
			return FormatSchema(schemaName, true);
		}

		protected virtual string FormatSchema(string schemaName, bool escapeSchemaName)
		{
			// schemas were not supported until SQL Server 2005
			return string.Empty;
		}

		protected new string FormatExpression(string template, params object[] args)
		{
			return string.Format(template, args);
		}

		protected string GetColumnList(IEnumerable<string> columns)
		{
			var result = "";
			foreach (var column in columns)
			{
				result += "[" + column + "],";
			}
			return result.TrimEnd(',');
		}

		protected string GetDataList(List<object> data)
		{
			var result = "";
			foreach (var column in data)
			{
				result += GetConstantValue(column) + ",";
			}
			return result.TrimEnd(',');
		}
	}
}
