#region License
// 
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

using System.Text;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;

    public class SqlServer2005Generator : SqlServer2000Generator
    {
        public SqlServer2005Generator()
            : base(new SqlServerColumn(new SqlServer2005TypeMap()), new SqlServer2005DescriptionGenerator())
        {
        }

        protected SqlServer2005Generator(IColumn column, IDescriptionGenerator descriptionGenerator)
            : base(column, descriptionGenerator)
        {
        }

        public override string CreateTable { get { return "{0} ({1})"; } }
        public override string DropTable { get { return "{0}"; } }

        public override string AddColumn { get { return "{0} ADD {1}"; } }

        public override string AlterColumn { get { return "{0} ALTER COLUMN {1}"; } }

        public override string RenameColumn { get { return "{0}.{1}', '{2}'"; } }
        public override string RenameTable { get { return "{0}', '{1}'"; } }

        public override string CreateIndex { get { return "CREATE {0}{1}INDEX {2} ON {3}.{4} ({5}{6}{7})"; } }
        public override string DropIndex { get { return "DROP INDEX {0} ON {1}.{2}"; } }

        public override string InsertData { get { return "INSERT INTO {0}.{1} ({2}) VALUES ({3})"; } }
        public override string UpdateData { get { return "{0} SET {1} WHERE {2}"; } }
        public override string DeleteData { get { return "DELETE FROM {0}.{1} WHERE {2}"; } }
        public override string IdentityInsert { get { return "SET IDENTITY_INSERT {0}.{1} {2}"; } }

        public override string CreateForeignKeyConstraint { get { return "ALTER TABLE {0}.{1} ADD CONSTRAINT {2} FOREIGN KEY ({3}) REFERENCES {4}.{5} ({6}){7}{8}"; } }
        public override string CreateConstraint { get { return "{0} ADD CONSTRAINT {1} {2}{3} ({4})"; } }
        public override string DeleteConstraint { get { return "{0} DROP CONSTRAINT {1}"; } }

        public virtual string GetIncludeString(CreateIndexExpression column)
        {
            return column.Index.Includes.Count > 0 ? ") INCLUDE (" : string.Empty;
        }

        public override string Generate(CreateTableExpression expression)
        {
            var descriptionStatements = DescriptionGenerator.GenerateDescriptionStatements(expression);
            var createTableStatement = string.Format("CREATE TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), base.Generate(expression));
            var descriptionStatementsArray = descriptionStatements as string[] ?? descriptionStatements.ToArray();

            if (!descriptionStatementsArray.Any())
                return createTableStatement;

            return ComposeStatements(createTableStatement, descriptionStatementsArray);
        }

        public override string Generate(AlterTableExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return base.Generate(expression);

            return descriptionStatement;
        }

        public override string Generate(DeleteTableExpression expression)
        {
            return string.Format("DROP TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), base.Generate(expression));
        }

        public override string Generate(CreateColumnExpression expression)
        {
            var alterTableStatement = string.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), base.Generate(expression));
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return alterTableStatement;

            return ComposeStatements(alterTableStatement, new[] { descriptionStatement });
        }

        public override string Generate(AlterColumnExpression expression)
        {
            var alterTableStatement = string.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), base.Generate(expression));
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return alterTableStatement;

            return ComposeStatements(alterTableStatement, new[] { descriptionStatement });
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return string.Format("sp_rename '{0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), base.Generate(expression));
        }

        public override string Generate(RenameTableExpression expression)
        {
            return string.Format("sp_rename '{0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), base.Generate(expression));
        }

        public override string Generate(UpdateDataExpression expression)
        {
            return string.Format("UPDATE {0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), base.Generate(expression));
        }

        public override string Generate(DeleteDataExpression expression)
        {
            var deleteItems = new List<string>();


            if (expression.IsAllRows)
            {
                deleteItems.Add(string.Format(DeleteData, Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName), "1 = 1"));
            }
            else
            {
                foreach (var row in expression.Rows)
                {
                    var whereClauses = new List<string>();
                    foreach (KeyValuePair<string, object> item in row)
                    {
                        whereClauses.Add(string.Format("{0} {1} {2}", Quoter.QuoteColumnName(item.Key), item.Value == null ? "IS" : "=", Quoter.QuoteValue(item.Value)));
                    }

                    deleteItems.Add(string.Format(DeleteData, Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName), String.Join(" AND ", whereClauses.ToArray())));
                }
            }

            return String.Join("; ", deleteItems.ToArray());
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.ForeignKey.ForeignTableSchema), base.Generate(expression));
        }

        public override string Generate(InsertDataExpression expression)
        {
            List<string> columnNames = new List<string>();
            List<string> columnValues = new List<string>();
            List<string> insertStrings = new List<string>();

            if (IsUsingIdentityInsert(expression))
            {
                insertStrings.Add(string.Format(IdentityInsert,
                            Quoter.QuoteSchemaName(expression.SchemaName),
                            Quoter.QuoteTableName(expression.TableName),
                            "ON"));
            }

            foreach (InsertionDataDefinition row in expression.Rows)
            {
                columnNames.Clear();
                columnValues.Clear();
                foreach (KeyValuePair<string, object> item in row)
                {
                    columnNames.Add(Quoter.QuoteColumnName(item.Key));
                    columnValues.Add(Quoter.QuoteValue(item.Value));
                }

                string columns = String.Join(", ", columnNames.ToArray());
                string values = String.Join(", ", columnValues.ToArray());
                insertStrings.Add(String.Format(InsertData
                    , Quoter.QuoteSchemaName(expression.SchemaName)
                    , Quoter.QuoteTableName(expression.TableName)
                    , columns
                    , values));
            }

            if (IsUsingIdentityInsert(expression))
            {
                insertStrings.Add(string.Format(IdentityInsert,
                            Quoter.QuoteSchemaName(expression.SchemaName),
                            Quoter.QuoteTableName(expression.TableName),
                            "OFF"));
            }

            return String.Join("; ", insertStrings.ToArray());
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            if (expression.ForeignKey.PrimaryColumns.Count != expression.ForeignKey.ForeignColumns.Count)
            {
                throw new ArgumentException("Number of primary columns and secondary columns must be equal");
            }

            List<string> primaryColumns = new List<string>();
            List<string> foreignColumns = new List<string>();
            foreach (var column in expression.ForeignKey.PrimaryColumns)
            {
                primaryColumns.Add(Quoter.QuoteColumnName(column));
            }

            foreach (var column in expression.ForeignKey.ForeignColumns)
            {
                foreignColumns.Add(Quoter.QuoteColumnName(column));
            }
            return string.Format(
                CreateForeignKeyConstraint,
                Quoter.QuoteSchemaName(expression.ForeignKey.ForeignTableSchema),
                Quoter.QuoteTableName(expression.ForeignKey.ForeignTable),
                Quoter.QuoteColumnName(expression.ForeignKey.Name),
                String.Join(", ", foreignColumns.ToArray()),
                Quoter.QuoteSchemaName(expression.ForeignKey.PrimaryTableSchema),
                Quoter.QuoteTableName(expression.ForeignKey.PrimaryTable),
                String.Join(", ", primaryColumns.ToArray()),
                FormatCascade("DELETE", expression.ForeignKey.OnDelete),
                FormatCascade("UPDATE", expression.ForeignKey.OnUpdate)
                );
        }

        public override string Generate(CreateIndexExpression expression)
        {

            string[] indexColumns = new string[expression.Index.Columns.Count];
            IndexColumnDefinition columnDef;


            for (int i = 0; i < expression.Index.Columns.Count; i++)
            {
                columnDef = expression.Index.Columns.ElementAt(i);
                if (columnDef.Direction == Direction.Ascending)
                {
                    indexColumns[i] = Quoter.QuoteColumnName(columnDef.Name) + " ASC";
                }
                else
                {
                    indexColumns[i] = Quoter.QuoteColumnName(columnDef.Name) + " DESC";
                }
            }

            string[] indexIncludes = new string[expression.Index.Includes.Count];
            IndexIncludeDefinition includeDef;

            for (int i = 0; i < expression.Index.Includes.Count; i++)
            {
                includeDef = expression.Index.Includes.ElementAt(i);
                indexIncludes[i] = Quoter.QuoteColumnName(includeDef.Name);
            }

            return String.Format(CreateIndex
                , GetUniqueString(expression)
                , GetClusterTypeString(expression)
                , Quoter.QuoteIndexName(expression.Index.Name)
                , Quoter.QuoteSchemaName(expression.Index.SchemaName)
                , Quoter.QuoteTableName(expression.Index.TableName)
                , String.Join(", ", indexColumns)
                , GetIncludeString(expression)
                , String.Join(", ", indexIncludes));
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            return String.Format(DropIndex, Quoter.QuoteIndexName(expression.Index.Name), Quoter.QuoteSchemaName(expression.Index.SchemaName), Quoter.QuoteTableName(expression.Index.TableName));
        }

        protected override void BuildDelete(DeleteColumnExpression expression, string columnName, StringBuilder builder)
        {
            builder.AppendLine(Generate(new DeleteDefaultConstraintExpression
            {
                ColumnName = columnName,
                SchemaName = expression.SchemaName,
                TableName = expression.TableName
            }));

            builder.AppendLine();

            builder.AppendLine(String.Format("-- now we can finally drop column" + Environment.NewLine + "ALTER TABLE {2}.{0} DROP COLUMN {1};",
                                         Quoter.QuoteTableName(expression.TableName),
                                         Quoter.QuoteColumnName(columnName),
                                         Quoter.QuoteSchemaName(expression.SchemaName)));
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            // before we alter a default constraint on a column, we have to drop any default value constraints in SQL Server
            var builder = new StringBuilder();

            builder.AppendLine(Generate(new DeleteDefaultConstraintExpression
            {
                ColumnName = expression.ColumnName,
                SchemaName = expression.SchemaName,
                TableName = expression.TableName
            }));

            builder.AppendLine();

            builder.Append(String.Format("-- create alter table command to create new default constraint as string and run it" + Environment.NewLine + "ALTER TABLE {3}.{0} WITH NOCHECK ADD CONSTRAINT {4} DEFAULT({2}) FOR {1};",
                Quoter.QuoteTableName(expression.TableName),
                Quoter.QuoteColumnName(expression.ColumnName),
                ((SqlServerColumn)Column).FormatDefaultValue(expression.DefaultValue),
                Quoter.QuoteSchemaName(expression.SchemaName),
                Quoter.QuoteConstraintName(SqlServerColumn.GetDefaultConstraintName(expression.TableName, expression.ColumnName))));

            return builder.ToString();
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.Constraint.SchemaName), base.Generate(expression));
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            string sql =
                "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
                "-- get name of default constraint" + Environment.NewLine +
                "SELECT @default = name" + Environment.NewLine +
                "FROM sys.default_constraints" + Environment.NewLine +
                "WHERE parent_object_id = object_id('{2}.{0}')" + Environment.NewLine +
                "AND type = 'D'" + Environment.NewLine +
                "AND parent_column_id = (" + Environment.NewLine +
                "SELECT column_id" + Environment.NewLine +
                "FROM sys.columns" + Environment.NewLine +
                "WHERE object_id = object_id('{2}.{0}')" + Environment.NewLine +
                "AND name = '{1}'" + Environment.NewLine +
                ");" + Environment.NewLine + Environment.NewLine +
                "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                "SET @sql = N'ALTER TABLE {2}.{0} DROP CONSTRAINT ' + @default;" + Environment.NewLine +
                "EXEC sp_executesql @sql;";
            return String.Format(sql, Quoter.QuoteTableName(expression.TableName), expression.ColumnName, Quoter.QuoteSchemaName(expression.SchemaName));
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.Constraint.SchemaName), base.Generate(expression));
        }

        public override string Generate(CreateSchemaExpression expression)
        {
            return String.Format(CreateSchema, Quoter.QuoteSchemaName(expression.SchemaName));
        }

        public override string Generate(DeleteSchemaExpression expression)
        {
            return String.Format(DropSchema, Quoter.QuoteSchemaName(expression.SchemaName));
        }

        public override string Generate(AlterSchemaExpression expression)
        {
            return String.Format(AlterSchema, Quoter.QuoteSchemaName(expression.DestinationSchemaName), Quoter.QuoteSchemaName(expression.SourceSchemaName), Quoter.QuoteTableName(expression.TableName));
        }

        private string ComposeStatements(string ddlStatement, IEnumerable<string> otherStatements)
        {
            var otherStatementsArray = otherStatements.ToArray();

            var statementsBuilder = new StringBuilder();
            statementsBuilder.AppendLine(ddlStatement);
            statementsBuilder.AppendLine("GO");
            statementsBuilder.AppendLine(string.Join(";", otherStatementsArray));

            return statementsBuilder.ToString();
        }
    }
}