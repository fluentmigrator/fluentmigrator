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
        public SqlServer2000Generator()
            : base(new SqlServerColumn(new SqlServer2000TypeMap()), new ConstantFormatter())
        {
        }

        protected SqlServer2000Generator(IColumn column)
            : base(column, new ConstantFormatter())
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

        public override string Generate(RenameTableExpression expression)
        {
            return String.Format("sp_rename '{0}[{1}]', '{2}'", FormatSchema(expression.SchemaName), FormatSqlEscape(expression.OldName), FormatSqlEscape(expression.NewName));
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return String.Format("sp_rename '{0}[{1}].[{2}]', '{3}'", FormatSchema(expression.SchemaName, false), FormatSqlEscape(expression.TableName), FormatSqlEscape(expression.OldName), FormatSqlEscape(expression.NewName));
        }

        public override string Generate(AlterColumnExpression expression)
        {
            return String.Format("ALTER TABLE {0}[{1}] ALTER COLUMN {2}", FormatSchema(expression.SchemaName), expression.TableName, Column.Generate(expression.Column));
        }

        public override string Generate(CreateTableExpression expression)
        {
            return String.Format("CREATE TABLE {0}[{1}] ({2})", FormatSchema(expression.SchemaName), expression.TableName, Column.Generate(expression));
        }

        public override string Generate(DeleteTableExpression expression)
        {
            return String.Format("DROP TABLE {0}[{1}]", FormatSchema(expression.SchemaName), expression.TableName);
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            var primaryColumns = GetColumnList(expression.ForeignKey.PrimaryColumns);
            var foreignColumns = GetColumnList(expression.ForeignKey.ForeignColumns);

            const string sql = "ALTER TABLE {0}[{1}] ADD CONSTRAINT {2} FOREIGN KEY ({3}) REFERENCES {4}[{5}] ({6}){7}{8}";

            return string.Format(sql,
                                FormatSchema(expression.ForeignKey.ForeignTableSchema),
                                expression.ForeignKey.ForeignTable,
                                expression.ForeignKey.Name,
                                foreignColumns,
                                FormatSchema(expression.ForeignKey.PrimaryTableSchema),
                                expression.ForeignKey.PrimaryTable,
                                primaryColumns,
                                FormatCascade("DELETE", expression.ForeignKey.OnDelete),
                                FormatCascade("UPDATE", expression.ForeignKey.OnUpdate)
                );
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            const string sql = "ALTER TABLE {0}[{1}] DROP CONSTRAINT {2}";
            return string.Format(sql, FormatSchema(expression.ForeignKey.ForeignTableSchema), expression.ForeignKey.ForeignTable, expression.ForeignKey.Name);
        }

        public override string Generate(CreateColumnExpression expression)
        {
            return String.Format("ALTER TABLE {0}[{1}] ADD {2}", FormatSchema(expression.SchemaName), expression.TableName, Column.Generate(expression.Column));
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

            return String.Format(sql, FormatSchema(expression.SchemaName), FormatSchema(expression.SchemaName, false), expression.TableName, expression.ColumnName);
        }

        public override string Generate(CreateIndexExpression expression)
        {
            var result = new StringBuilder("CREATE");
            if (expression.Index.IsUnique)
                result.Append(" UNIQUE");

            result.Append(expression.Index.IsClustered ? " CLUSTERED" : " NONCLUSTERED");

            result.Append(" INDEX [{0}] ON {1}[{2}] (");

            var first = true;
            foreach (var column in expression.Index.Columns)
            {
                if (first)
                    first = false;
                else
                    result.Append(",");

                result.Append("[" + column.Name + "]");
                result.Append(column.Direction == Direction.Ascending ? " ASC" : " DESC");
            }
            result.Append(")");

            return String.Format(result.ToString(), expression.Index.Name, FormatSchema(expression.Index.SchemaName), expression.Index.TableName);
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            return String.Format("DROP INDEX {0}[{1}] ON [{2}]", FormatSchema(expression.Index.SchemaName), expression.Index.Name, expression.Index.TableName);
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
                result.Append(String.Format("INSERT INTO {0}[{1}] ({2}) VALUES ({3});", FormatSchema(expression.SchemaName), expression.TableName, columns, data));
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

                set += String.Format("[{0}] = {1}", item.Key, Constant.Format(item.Value));
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

                where += String.Format("[{0}] {1} {2}", item.Key, item.Value == null ? "IS" : "=", Constant.Format(item.Value));
                i++;
            }

            result.Append(String.Format("UPDATE {0}[{1}] SET {2} WHERE {3};", FormatSchema(expression.SchemaName), expression.TableName, set, where));

            return result.ToString();
        }

        public override string Generate(DeleteDataExpression expression)
        {
            var result = new StringBuilder();

            if (expression.IsAllRows)
            {
                result.Append(String.Format("DELETE FROM {0}[{1}];", FormatSchema(expression.SchemaName), expression.TableName));
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

                        where += String.Format("[{0}] {1} {2}", item.Key, item.Value == null ? "IS" : "=", Constant.Format(item.Value));
                        i++;
                    }

                    result.Append(String.Format("DELETE FROM {0}[{1}] WHERE {2};", FormatSchema(expression.SchemaName), expression.TableName, where));
                }
            }

            return result.ToString();
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            const string sql =
                @"
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

			-- create alter table command to drop contraint as string and run it
			SET @sql = N'ALTER TABLE {0}[{2}] DROP CONSTRAINT ' + @default;
			EXEC sp_executesql @sql;

			-- create alter table command to create new default constraint as string and run it
			SET @sql = N'ALTER TABLE {0}[{2}] WITH NOCHECK ADD CONSTRAINT [' + @default + '] DEFAULT({4}) FOR {3}';
			EXEC sp_executesql @sql;";

            return String.Format(sql, FormatSchema(expression.SchemaName), FormatSchema(expression.SchemaName, false), expression.TableName, expression.ColumnName, FormatSqlEscape(Constant.Format(expression.DefaultValue)));
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
                result += Constant.Format(column) + ",";
            }
            return result.TrimEnd(',');
        }

        protected string FormatSqlEscape(string sql)
        {
            return sql.Replace("'", "''");
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
