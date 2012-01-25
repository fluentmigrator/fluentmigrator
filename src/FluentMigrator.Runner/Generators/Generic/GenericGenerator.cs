using System;
using System.Collections.Generic;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;
using System.Linq;
using System.Data;

namespace FluentMigrator.Runner.Generators.Generic
{
    public abstract class GenericGenerator : GeneratorBase
    {
        public GenericGenerator(IColumn column, IQuoter quoter)
            : base(column, quoter)
        { }

        public virtual string CreateTable { get { return "CREATE TABLE {0} ({1})"; } }
        public virtual string DropTable { get { return "DROP TABLE {0}"; } }

        public virtual string AddColumn { get { return "ALTER TABLE {0} ADD COLUMN {1}"; } }
        public virtual string DropColumn { get { return "ALTER TABLE {0} DROP COLUMN {1}"; } }
        public virtual string AlterColumn { get { return "ALTER TABLE {0} ALTER COLUMN {1}"; } }
        public virtual string RenameColumn { get { return "ALTER TABLE {0} RENAME COLUMN {1} TO {2}"; } }

        public virtual string RenameTable { get { return "RENAME TABLE {0} TO {1}"; } }

        public virtual string CreateSchema { get { return "CREATE SCHEMA {0}"; } }
        public virtual string AlterSchema { get { return "ALTER SCHEMA {0} TRANSFER {1}.{2}"; } }
        public virtual string DropSchema { get { return "DROP SCHEMA {0}"; } }

        public virtual string CreateIndex { get { return "CREATE {0}{1}INDEX {2} ON {3} ({4})"; } }
        public virtual string DropIndex { get { return "DROP INDEX {0}"; } }

        public virtual string InsertData { get { return "INSERT INTO {0} ({1}) VALUES ({2})"; } }
        public virtual string UpdateData { get { return "UPDATE {0} SET {1} WHERE {2}"; } }
        public virtual string DeleteData { get { return "DELETE FROM {0} WHERE {1}"; } }

        public virtual string CreateConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} {2} ({3})"; } }
        public virtual string DeleteConstraint { get { return "ALTER TABLE {0} DROP CONSTRAINT {1}"; } }
        public virtual string CreateForeignKeyConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6}"; } }

        public virtual string GetUniqueString(CreateIndexExpression column)
        {
            return column.Index.IsUnique ? "UNIQUE " : string.Empty;
        }

        public virtual string GetClusterTypeString(CreateIndexExpression column)
        {
            return string.Empty;
        }

        /// <summary>Outputs a create table string</summary>
        /// <param name="expression">The expression to generate.</param>
        public override string Generate(CreateTableExpression expression)
        {
            if (string.IsNullOrEmpty(expression.TableName))
                throw new ArgumentNullException("expression", "expression.TableName cannot be empty");
            if (expression.Columns.Count == 0)
                throw new ArgumentException("You must specifiy at least one column");

            string table = this.GenerateTableName(expression.SchemaName, expression.TableName);
            return string.Format(CreateTable, Quoter.QuoteTableName(table), Column.Generate(expression.Columns, table));
        }

        public override string Generate(DeleteTableExpression expression)
        {
            string table = this.GenerateTableName(expression.SchemaName, expression.TableName);
            return String.Format(DropTable, Quoter.QuoteTableName(table));
        }

        public override string Generate(RenameTableExpression expression)
        {
            string oldName = this.GenerateTableName(expression.SchemaName, expression.OldName);
            string newName = this.GenerateTableName(expression.SchemaName, expression.NewName);
            return String.Format(RenameTable, Quoter.QuoteTableName(oldName), Quoter.QuoteTableName(newName));
        }

        public override string Generate(CreateColumnExpression expression)
        {
            string table = this.GenerateTableName(expression.SchemaName, expression.TableName);
            return String.Format(AddColumn, Quoter.QuoteTableName(table), Column.Generate(expression.Column));
        }


        public override string Generate(AlterColumnExpression expression)
        {
            string table = this.GenerateTableName(expression.SchemaName, expression.TableName);
            return String.Format(AlterColumn, Quoter.QuoteTableName(table), Column.Generate(expression.Column));
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            string table = this.GenerateTableName(expression.SchemaName, expression.TableName);
            return String.Format(DropColumn, Quoter.QuoteTableName(table), Quoter.QuoteColumnName(expression.ColumnName));
        }

        public override string Generate(RenameColumnExpression expression)
        {
            string table = this.GenerateTableName(expression.SchemaName, expression.TableName);
            return String.Format(RenameColumn, table, expression.OldName, expression.NewName);
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

            string table = this.GenerateTableName(expression.Index.SchemaName, expression.Index.TableName);
            return String.Format(CreateIndex
                , GetUniqueString(expression)
                , GetClusterTypeString(expression)
                , Quoter.QuoteIndexName(expression.Index.Name)
                , Quoter.QuoteTableName(table)
                , String.Join(", ", indexColumns));
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            string table = this.GenerateTableName(expression.Index.SchemaName, expression.Index.TableName);
            return String.Format(DropIndex, Quoter.QuoteIndexName(expression.Index.Name), Quoter.QuoteTableName(table));
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            if (expression.ForeignKey.PrimaryColumns.Count != expression.ForeignKey.ForeignColumns.Count)
            {
                throw new ArgumentException("Number of primary columns and secondary columns must be equal");
            }

            string keyName = string.IsNullOrEmpty(expression.ForeignKey.Name)
                ? GenerateForeignKeyName(expression)
                : expression.ForeignKey.Name;

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
            string primaryTable = this.GenerateTableName(expression.ForeignKey.PrimaryTableSchema, expression.ForeignKey.PrimaryTable);
            string foreignTable = this.GenerateTableName(expression.ForeignKey.ForeignTableSchema, expression.ForeignKey.ForeignTable);
            return string.Format(
                CreateForeignKeyConstraint,
                Quoter.QuoteTableName(foreignTable),
                Quoter.QuoteColumnName(keyName),
                String.Join(", ", foreignColumns.ToArray()),
                Quoter.QuoteTableName(primaryTable),
                String.Join(", ", primaryColumns.ToArray()),
                FormatCascade("DELETE", expression.ForeignKey.OnDelete),
                FormatCascade("UPDATE", expression.ForeignKey.OnUpdate)
                );
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            var constraintType = (expression.Constraint.IsPrimaryKeyConstraint) ? "PRIMARY KEY" : "UNIQUE";

            string[] columns = new string[expression.Constraint.Columns.Count];

            for (int i = 0; i < expression.Constraint.Columns.Count; i++)
            {
                columns[i] = Quoter.QuoteColumnName(expression.Constraint.Columns.ElementAt(i));
            }

            string table = this.GenerateTableName(expression.Constraint.SchemaName, expression.Constraint.TableName);
            return string.Format(CreateConstraint, Quoter.QuoteTableName(table),
                Quoter.Quote(expression.Constraint.ConstraintName),
                constraintType,
                String.Join(", ", columns));
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            string table = this.GenerateTableName(expression.Constraint.SchemaName, expression.Constraint.TableName);
            return string.Format(DeleteConstraint, Quoter.QuoteTableName(table), Quoter.Quote(expression.Constraint.ConstraintName));
        }

        public virtual string GenerateForeignKeyName(CreateForeignKeyExpression expression)
        {
            return string.Format("FK_{0}_{1}", expression.ForeignKey.PrimaryTable.Substring(0, 5), expression.ForeignKey.ForeignTable.Substring(0, 5));
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            string foreignTable = this.GenerateTableName(expression.ForeignKey.ForeignTableSchema, expression.ForeignKey.ForeignTable);
            return string.Format(DeleteConstraint, Quoter.QuoteTableName(foreignTable), Quoter.QuoteColumnName(expression.ForeignKey.Name));
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


        public override string Generate(InsertDataExpression expression)
        {
            if (this.CompatibilityMode.HasFlag(CompatibilityMode.Strict))
            {
                List<string> unsupportedFeatures = expression.AdditionalFeatures.Keys.Where(x => !IsAdditionalFeatureSupported(x)).ToList();

                if (unsupportedFeatures.Any())
                {
                    string errorMessage =
                        string.Format("The following database specific additional features are not supported in strict mode [{0}]",
                                      expression.AdditionalFeatures.Keys.Aggregate((x, y) => x + ", " + y));
                    return this.CompatibilityMode.GetNotSupported(errorMessage);
                }
            }

            List<string> columnNames = new List<string>();
            List<string> columnValues = new List<string>();
            List<string> insertStrings = new List<string>();

            foreach (InsertionDataDefinition row in expression.Rows)
            {
                columnNames.Clear();
                columnValues.Clear();
                foreach (KeyValuePair<string, object> item in row)
                {
                    columnNames.Add(Quoter.QuoteColumnName(item.Key));
                    columnValues.Add(Quoter.QuoteValue(item.Value));
                }

                string table = this.GenerateTableName(expression.SchemaName, expression.TableName);
                string columns = String.Join(", ", columnNames.ToArray());
                string values = String.Join(", ", columnValues.ToArray());
                insertStrings.Add(String.Format(InsertData, Quoter.QuoteTableName(table), columns, values));
            }
            return String.Join("; ", insertStrings.ToArray());
        }

        public override string Generate(UpdateDataExpression expression)
        {

            List<string> updateItems = new List<string>();
            List<string> whereClauses = new List<string>();

            foreach (var item in expression.Set)
            {
                updateItems.Add(string.Format("{0} = {1}", Quoter.QuoteColumnName(item.Key), Quoter.QuoteValue(item.Value)));
            }

            foreach (var item in expression.Where)
            {
                whereClauses.Add(string.Format("{0} {1} {2}", Quoter.QuoteColumnName(item.Key), item.Value == null ? "IS" : "=", Quoter.QuoteValue(item.Value)));
            }

            string table = this.GenerateTableName(expression.SchemaName, expression.TableName);
            return String.Format(UpdateData, Quoter.QuoteTableName(table), String.Join(", ", updateItems.ToArray()), String.Join(" AND ", whereClauses.ToArray()));
        }

        public override string Generate(DeleteDataExpression expression)
        {
            var deleteItems = new List<string>();


            if (expression.IsAllRows)
            {
                deleteItems.Add(string.Format(DeleteData, Quoter.QuoteTableName(expression.TableName), "1 = 1"));
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

                    string table = this.GenerateTableName(expression.SchemaName, expression.TableName);
                    deleteItems.Add(string.Format(DeleteData, Quoter.QuoteTableName(table), String.Join(" AND ", whereClauses.ToArray())));
                }
            }

            return String.Join("; ", deleteItems.ToArray());
        }


        //All Schema method throw by default as only Sql server 2005 and up supports them.
        public override string Generate(CreateSchemaExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("This database does not support schemas.");
        }

        public override string Generate(DeleteSchemaExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("This database does not support schemas.");
        }

        public override string Generate(AlterSchemaExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("This database does not support schemas.");
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            var result = new StringBuilder(string.Format("CREATE SEQUENCE "));
            var seq = expression.Sequence;
            result.AppendFormat("{0}.{1}", Quoter.QuoteSchemaName(seq.SchemaName), Quoter.QuoteSequenceName(seq.Name));

            if (seq.Increment.HasValue)
            {
                result.AppendFormat(" INCREMENT {0}", seq.Increment);
            }

            if (seq.MinValue.HasValue)
            {
                result.AppendFormat(" MINVALUE {0}", seq.MinValue);
            }

            if (seq.MaxValue.HasValue)
            {
                result.AppendFormat(" MAXVALUE {0}", seq.MaxValue);
            }

            if (seq.StartWith.HasValue)
            {
                result.AppendFormat(" START WITH {0}", seq.StartWith);
            }

            if (seq.Cache.HasValue)
            {
                result.AppendFormat(" CACHE {0}", seq.Cache);
            }

            if (seq.Cycle)
            {
                result.Append(" CYCLE");
            }

            return result.ToString();
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            var result = new StringBuilder(string.Format("DROP SEQUENCE "));
            result.AppendFormat("{0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteSequenceName(expression.SequenceName));

            return result.ToString();
        }

        /// <summary>Get the name of a table. If <see cref="GeneratorBase.CompatibilityMode"/> includes <see cref="CompatibilityMode.Emulate"/>, the table name is prefixed with the schema name.</summary>
        /// <param name="schema">The name of the database schema.</param>
        /// <param name="table">The name of the table.</param>
        protected virtual string GenerateTableName(string schema, string table)
        {
            // validate
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException("table");

            // handle schema
            if (!string.IsNullOrEmpty(schema))
            {
                if (this.CompatibilityMode.HasFlag(CompatibilityMode.Emulate))
                    table = schema + "_" + table;
                else
                    this.CompatibilityMode.GetNotSupported("This database does not support schemas.");
            }
            return table;
        }
    }
}