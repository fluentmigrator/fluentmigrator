using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
    public class PostgresGenerator : GeneratorBase
    {
        public PostgresGenerator() : base(new PostgresColumn(), new PostgresFormatter()) { }

        public override string Generate(CreateSchemaExpression expression)
        {
            return string.Format("CREATE SCHEMA {0}", FormatSchema(expression.SchemaName));
        }

        public override string Generate(DeleteSchemaExpression expression)
        {
            return string.Format("DROP SCHEMA {0}", FormatSchema(expression.SchemaName));
        }

        public override string Generate(CreateTableExpression expression)
        {
            return String.Format("CREATE TABLE {0}.{1} ({2})",FormatSchema(expression.SchemaName), FormatIdentifier(expression.TableName), Column.Generate(expression));
        }

        public override string Generate(AlterColumnExpression expression)
        {
            return String.Format("ALTER TABLE {0}.{1} ALTER {2} TYPE {3}", FormatSchema(expression.SchemaName), FormatIdentifier(expression.TableName), FormatIdentifier(expression.Column.Name), ((PostgresColumn)Column).GetColumnType(expression.Column));
        }

        public override string Generate(CreateColumnExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1} ADD {2}", FormatSchema(expression.SchemaName), FormatIdentifier(expression.TableName), Column.Generate(expression.Column));
        }

        public override string Generate(DeleteTableExpression expression)
        {
            return String.Format("DROP TABLE {0}.{1}", FormatSchema(expression.SchemaName), FormatIdentifier(expression.TableName));
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1} DROP COLUMN {2}", FormatSchema(expression.SchemaName), FormatIdentifier(expression.TableName), FormatIdentifier(expression.ColumnName));
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            var primaryColumns = GetColumnList(expression.ForeignKey.PrimaryColumns);
            var foreignColumns = GetColumnList(expression.ForeignKey.ForeignColumns);

            const string sql = "ALTER TABLE {0}.{1} ADD CONSTRAINT {2} FOREIGN KEY ({3}) REFERENCES {4}.{5} ({6}){7}{8}";

            return string.Format(sql,
                                FormatSchema(expression.ForeignKey.ForeignTableSchema),
                                FormatIdentifier(expression.ForeignKey.ForeignTable),
                                FormatIdentifier(expression.ForeignKey.Name),
                                foreignColumns,
                                FormatSchema(expression.ForeignKey.PrimaryTableSchema),
                                FormatIdentifier(expression.ForeignKey.PrimaryTable),
                                primaryColumns,
                                FormatCascade("DELETE", expression.ForeignKey.OnDelete),
                                FormatCascade("UPDATE", expression.ForeignKey.OnUpdate)
                );
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1} DROP CONSTRAINT {2}", FormatSchema(expression.ForeignKey.ForeignTableSchema), FormatIdentifier(expression.ForeignKey.ForeignTable), FormatIdentifier(expression.ForeignKey.Name));
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
            result.Append(")");

            return String.Format(result.ToString(), FormatIdentifier(expression.Index.Name), FormatSchema(expression.Index.SchemaName), FormatIdentifier(expression.Index.TableName));

            /*
            var idx = String.Format(result.ToString(), expression.Index.Name, FormatSchema(expression.Index.SchemaName), expression.Index.TableName); 
            if (!expression.Index.IsClustered)
                return idx;
              
             // Clustered indexes in Postgres do not cluster updates/inserts to the table after the initial cluster operation is applied.
             // To keep the clustered index up to date run CLUSTER TableName periodically
             
            return string.Format("{0}; CLUSTER {1}\"{2}\" ON \"{3}\"", idx, FormatSchema(expression.Index.SchemaName), expression.Index.TableName, expression.Index.Name);
             */
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            return string.Format("DROP INDEX {0}.{1}", FormatSchema(expression.Index.SchemaName), FormatIdentifier(expression.Index.Name));
        }

        public override string Generate(RenameTableExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1} RENAME TO {2}", FormatSchema(expression.SchemaName), FormatIdentifier(expression.OldName), FormatIdentifier(expression.NewName));
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1} RENAME COLUMN {2} TO {3}", FormatSchema(expression.SchemaName), FormatIdentifier(expression.TableName), FormatIdentifier(expression.OldName), FormatIdentifier(expression.NewName));
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
                result.Append(String.Format("INSERT INTO {0}.{1} ({2}) VALUES ({3});", FormatSchema(expression.SchemaName), FormatIdentifier(expression.TableName), columns, data));
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

            if (expression.IsAllRows)
            {
                result.Append(String.Format("DELETE FROM {0}.{1};", FormatSchema(expression.SchemaName), FormatIdentifier(expression.TableName)));
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

                        where += String.Format("{0} {1} {2}", FormatIdentifier(item.Key), item.Value == null ? "IS" : "=", Constant.Format(item.Value));
                        i++;
                    }

                    result.Append(String.Format("DELETE FROM {0}.{1} WHERE {2};", FormatSchema(expression.SchemaName), FormatIdentifier(expression.TableName), where));
                }
            }

            return result.ToString();
        }

        public override string Generate(UpdateDataExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(AlterSchemaExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1} SET SCHEMA {2}", FormatSchema(expression.SourceSchemaName), FormatIdentifier(expression.TableName), FormatSchema(expression.DestinationSchemaName));
        }

        protected string FormatIdentifier(string identifier)
        {
            return string.Format("\"{0}\"", identifier.Replace("'", "''"));
        }

        protected string FormatSchema(string schemaName)
        {
            return FormatIdentifier(schemaName ?? "public");
        }

        protected string GetColumnList(IEnumerable<string> columns)
        {
            var result = "";
            foreach (var column in columns)
            {
                result += FormatIdentifier(column) + ",";
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

        protected string GetDataList(List<object> data)
        {
            var result = "";
            foreach (var column in data)
            {
                result += Constant.Format(column) + ",";
            }
            return result.TrimEnd(',');
        }
    }
}
