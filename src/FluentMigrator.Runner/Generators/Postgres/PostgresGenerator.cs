using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Postgres
{
    public class PostgresGenerator : GenericGenerator
    {
        public PostgresGenerator() : base(new PostgresColumn(), new PostgresQuoter(), new PostgresDescriptionGenerator())
        {
        }

        public override string Generate(AlterTableExpression expression)
        {
            var alterStatement = new StringBuilder();
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);
            alterStatement.Append(base.Generate(expression));
            if (string.IsNullOrEmpty(descriptionStatement))
            {
                alterStatement.Append(descriptionStatement);
            }
            return alterStatement.ToString();
        }

        public override string Generate(CreateSchemaExpression expression)
        {
            return string.Format("CREATE SCHEMA {0};", Quoter.QuoteSchemaName(expression.SchemaName));
        }

        public override string Generate(DeleteSchemaExpression expression)
        {
            return string.Format("DROP SCHEMA {0};", Quoter.QuoteSchemaName(expression.SchemaName));
        }

        public override string Generate(CreateTableExpression expression)
        {
            var createStatement = new StringBuilder();
            var tableName = Quoter.QuoteTableName(expression.TableName);
            createStatement.Append(string.Format("CREATE TABLE {0}.{1} ({2})", Quoter.QuoteSchemaName(expression.SchemaName), tableName, Column.Generate(expression.Columns, tableName)));
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatements(expression);
            createStatement.Append(";");

            if (descriptionStatement != null && descriptionStatement.Any())
            {
                createStatement.Append(string.Join(";", descriptionStatement.ToArray()));
                createStatement.Append(";");
            }
            return createStatement.ToString();
        }

        public override string Generate(AlterColumnExpression expression)
        {
            var alterStatement = new StringBuilder();
            alterStatement.Append(String.Format("ALTER TABLE {0}.{1} {2};", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName), ((PostgresColumn)Column).GenerateAlterClauses(expression.Column)));
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);
            if (!string.IsNullOrEmpty(descriptionStatement))
            {
                alterStatement.Append(";");
                alterStatement.Append(descriptionStatement);
            }
            return alterStatement.ToString();
        }

        public override string Generate(CreateColumnExpression expression)
        {
            var createStatement = new StringBuilder();
            createStatement.Append(string.Format("ALTER TABLE {0}.{1} ADD {2};", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName), Column.Generate(expression.Column)));
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);
            if (!string.IsNullOrEmpty(descriptionStatement))
            {
                createStatement.Append(";");
                createStatement.Append(descriptionStatement);
            }
            return createStatement.ToString();
        }

        public override string Generate(DeleteTableExpression expression)
        {
            return String.Format("DROP TABLE {0}.{1};", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName));
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string columnName in expression.ColumnNames) {
                if (expression.ColumnNames.First() != columnName) builder.AppendLine("");
                builder.AppendFormat("ALTER TABLE {0}.{1} DROP COLUMN {2};", 
                    Quoter.QuoteSchemaName(expression.SchemaName), 
                    Quoter.QuoteTableName(expression.TableName), 
                    Quoter.QuoteColumnName(columnName));
            }
            return builder.ToString();
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            var primaryColumns = GetColumnList(expression.ForeignKey.PrimaryColumns);
            var foreignColumns = GetColumnList(expression.ForeignKey.ForeignColumns);

            const string sql = "ALTER TABLE {0}.{1} ADD CONSTRAINT {2} FOREIGN KEY ({3}) REFERENCES {4}.{5} ({6}){7}{8};";

            return string.Format(sql,
                                Quoter.QuoteSchemaName(expression.ForeignKey.ForeignTableSchema),
                                Quoter.QuoteTableName(expression.ForeignKey.ForeignTable),
                                Quoter.Quote(expression.ForeignKey.Name),
                                foreignColumns,
                                Quoter.QuoteSchemaName(expression.ForeignKey.PrimaryTableSchema),
                                Quoter.QuoteTableName(expression.ForeignKey.PrimaryTable),
                                primaryColumns,
                                FormatCascade("DELETE", expression.ForeignKey.OnDelete),
                                FormatCascade("UPDATE", expression.ForeignKey.OnUpdate)
                );
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1} DROP CONSTRAINT {2};", Quoter.QuoteSchemaName(expression.ForeignKey.ForeignTableSchema), Quoter.QuoteTableName(expression.ForeignKey.ForeignTable), Quoter.Quote(expression.ForeignKey.Name));
        }

        public override string Generate(CreateIndexExpression expression)
        {
            var result = new StringBuilder("CREATE");
            if (expression.Index.IsUnique)
                result.Append(" UNIQUE");

            result.Append(" INDEX {0} ON {1}.{2} (");

            var first = true;
            foreach (var column in expression.Index.Columns)
            {
                if (first)
                    first = false;
                else
                    result.Append(",");

                result.Append("\"" + column.Name + "\"");
                result.Append(column.Direction == Direction.Ascending ? " ASC" : " DESC");
            }
            result.Append(");");

            return String.Format(result.ToString(), Quoter.QuoteIndexName(expression.Index.Name), Quoter.QuoteSchemaName(expression.Index.SchemaName), Quoter.QuoteTableName(expression.Index.TableName));

            /*
            var idx = String.Format(result.ToString(), expression.Index.Name, Quoter.QuoteSchemaName(expression.Index.SchemaName), expression.Index.TableName); 
            if (!expression.Index.IsClustered)
                return idx;
              
             // Clustered indexes in Postgres do not cluster updates/inserts to the table after the initial cluster operation is applied.
             // To keep the clustered index up to date run CLUSTER TableName periodically
             
            return string.Format("{0}; CLUSTER {1}\"{2}\" ON \"{3}\"", idx, Quoter.QuoteSchemaName(expression.Index.SchemaName), expression.Index.TableName, expression.Index.Name);
             */
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            return string.Format("DROP INDEX {0}.{1};", Quoter.QuoteSchemaName(expression.Index.SchemaName), Quoter.QuoteIndexName(expression.Index.Name));
        }

        public override string Generate(RenameTableExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1} RENAME TO {2};", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.OldName), Quoter.QuoteTableName(expression.NewName));
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1} RENAME COLUMN {2} TO {3};", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName), Quoter.QuoteColumnName(expression.OldName), Quoter.QuoteColumnName(expression.NewName));
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
                result.Append(String.Format("INSERT INTO {0}.{1} ({2}) VALUES ({3});", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName), columns, data));
            }
            return result.ToString();
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return String.Format("ALTER TABLE {0}.{1} ALTER {2} DROP DEFAULT, ALTER {2} {3};", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName), Quoter.QuoteColumnName(expression.ColumnName), ((PostgresColumn)Column).FormatAlterDefaultValue(expression.ColumnName, expression.DefaultValue));
        }

        public override string Generate(DeleteDataExpression expression)
        {
            var result = new StringBuilder();

            if (expression.IsAllRows)
            {
                result.Append(String.Format("DELETE FROM {0}.{1};", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName)));
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

                        where += String.Format("{0} {1} {2}", Quoter.QuoteColumnName(item.Key), item.Value == null ? "IS" : "=", Quoter.QuoteValue(item.Value));
                        i++;
                    }

                    result.Append(String.Format("DELETE FROM {0}.{1} WHERE {2};", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName), where));
                }
            }

            return result.ToString();
        }

        public override string Generate(UpdateDataExpression expression)
        {
            var updateItems = new List<string>();
            var whereClauses = new List<string>();

            foreach (var item in expression.Set)
            {
                updateItems.Add(string.Format("{0} = {1}", Quoter.QuoteColumnName(item.Key), Quoter.QuoteValue(item.Value)));
            }

            if (expression.IsAllRows)
            {
                whereClauses.Add("1 = 1");
            }
            else
            {
                foreach (var item in expression.Where)
                {
                    whereClauses.Add(string.Format("{0} {1} {2}", Quoter.QuoteColumnName(item.Key),
                                                   item.Value == null ? "IS" : "=", Quoter.QuoteValue(item.Value)));
                }
            }
            return String.Format("UPDATE {0}.{1} SET {2} WHERE {3};", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName), String.Join(", ", updateItems.ToArray()), String.Join(" AND ", whereClauses.ToArray()));
        }

        public override string Generate(AlterSchemaExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1} SET SCHEMA {2};", Quoter.QuoteSchemaName(expression.SourceSchemaName), Quoter.QuoteTableName(expression.TableName), Quoter.QuoteSchemaName(expression.DestinationSchemaName));
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1} ALTER {2} DROP DEFAULT;", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName), Quoter.Quote(expression.ColumnName));
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1} DROP CONSTRAINT {2};", Quoter.QuoteSchemaName(expression.Constraint.SchemaName), Quoter.QuoteTableName(expression.Constraint.TableName), Quoter.Quote(expression.Constraint.ConstraintName));
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            var constraintType = (expression.Constraint.IsPrimaryKeyConstraint) ? "PRIMARY KEY" : "UNIQUE";

            string[] columns = new string[expression.Constraint.Columns.Count];

            for (int i = 0; i < expression.Constraint.Columns.Count; i++)
            {
                columns[i] = Quoter.QuoteColumnName(expression.Constraint.Columns.ElementAt(i));
            }

            return string.Format("ALTER TABLE {0}.{1} ADD CONSTRAINT {2} {3} ({4});", Quoter.QuoteSchemaName(expression.Constraint.SchemaName),
                Quoter.QuoteTableName(expression.Constraint.TableName),
                Quoter.QuoteConstraintName(expression.Constraint.ConstraintName),
                constraintType,
                String.Join(", ", columns));
        }

        protected string GetColumnList(IEnumerable<string> columns)
        {
            var result = "";
            foreach (var column in columns)
            {
                result += Quoter.QuoteColumnName(column) + ",";
            }
            return result.TrimEnd(',');
        }

        protected string GetDataList(List<object> data)
        {
            var result = "";
            foreach (var column in data)
            {
                result += Quoter.QuoteValue(column) + ",";
            }
            return result.TrimEnd(',');
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            return String.Format("{0};", base.Generate(expression));
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return String.Format("{0};", base.Generate(expression));
        }
    }
}
