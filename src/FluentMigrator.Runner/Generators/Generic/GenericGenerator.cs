using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Generic
{
    public abstract class GenericGenerator : GeneratorBase
    {
        public CompatabilityMode compatabilityMode;

        public GenericGenerator(IColumn column, IQuoter quoter, IEvaluator evaluator) : base(column, quoter, evaluator)
        {
            compatabilityMode = CompatabilityMode.LOOSE;
        }

        public virtual string CommandDelimiter { get { return string.Empty; } }
        public virtual string MultilineDelimiter { get { return "; "; } }

        public virtual string CreateTable { get { return "CREATE TABLE {0} ({1}){2}"; } }
        public virtual string DropTable { get { return "DROP TABLE {0}{1}"; } }

        public virtual string AddColumn { get { return "ALTER TABLE {0} ADD COLUMN {1}{2}"; } }
        public virtual string DropColumn { get { return "ALTER TABLE {0} DROP COLUMN {1}{2}"; } }
        public virtual string AlterColumn { get { return "ALTER TABLE {0} ALTER COLUMN {1}{2}"; } }
        public virtual string RenameColumn { get { return "ALTER TABLE {0} RENAME COLUMN {1} TO {2}{3}"; } }

        public virtual string RenameTable { get { return "RENAME TABLE {0} TO {1}{2}"; } }

        public virtual string CreateSchema { get { return "CREATE SCHEMA {0}{1}"; } }
        public virtual string AlterSchema { get { return "ALTER SCHEMA {0} TRANSFER {1}.{2}{3}"; } }
        public virtual string DropSchema { get { return "DROP SCHEMA {0}{1}"; } }

        public virtual string CreateIndex { get { return "CREATE {0}{1}INDEX {2} ON {3} ({4}){5}"; } }
        public virtual string DropIndex { get { return "DROP INDEX {0}{1}" ; } }

        public virtual string InsertData { get { return "INSERT INTO {0} ({1}) VALUES ({2}){3}"; } }
        public virtual string UpdateData { get { return "UPDATE {0} SET {1} WHERE {2}{3}"; } }
        public virtual string DeleteData { get { return "DELETE FROM {0} WHERE {1}{2}"; } }

        public virtual string SetData { get { return "{0} = {1}"; } }
        public virtual string BinaryExpression { get { return "{0} {1} {2}"; } }

        public virtual string CreateConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} {2} ({3}){4}"; } }
        public virtual string DeleteConstraint { get { return "ALTER TABLE {0} DROP CONSTRAINT {1}{2}"; } }
        public virtual string CreateForeignKeyConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6}{7}"; } }

        public virtual string GetUniqueString(CreateIndexExpression column)
        {
            return column.Index.IsUnique ? "UNIQUE " : string.Empty;
        }

        public virtual string GetClusterTypeString(CreateIndexExpression column)
        {
            return string.Empty;
        }

        /// <summary>
        /// Outputs a create table string
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override string Generate(CreateTableExpression expression)
        {
            if (string.IsNullOrEmpty(expression.TableName)) throw new ArgumentNullException("expression", "expression.TableName cannot be empty");
            if (expression.Columns.Count == 0) throw new ArgumentException("You must specifiy at least one column");

            string quotedTableName = Quoter.QuoteTableName(expression.TableName);

            string errors = ValidateAdditionalFeatureCompatibility(expression.Columns.SelectMany(x => x.AdditionalFeatures));
            if (!string.IsNullOrEmpty(errors)) return errors;

            return string.Format(CreateTable, quotedTableName, Column.Generate(expression.Columns, quotedTableName), CommandDelimiter);
        }

        public override string Generate(DeleteTableExpression expression)
        {
            return String.Format(DropTable, Quoter.QuoteTableName(expression.TableName), CommandDelimiter);
        }

        public override string Generate(RenameTableExpression expression)
        {
            return String.Format(RenameTable, Quoter.QuoteTableName(expression.OldName), Quoter.QuoteTableName(expression.NewName), CommandDelimiter);
        }

        public override string Generate(CreateColumnExpression expression) 
        {
            string errors = ValidateAdditionalFeatureCompatibility(expression.Column.AdditionalFeatures);
            if (!string.IsNullOrEmpty(errors)) return errors;

            return String.Format(AddColumn, Quoter.QuoteTableName(expression.TableName), Column.Generate(expression.Column), CommandDelimiter);
        }


        public override string Generate(AlterColumnExpression expression)
        {
            string errors = ValidateAdditionalFeatureCompatibility(expression.Column.AdditionalFeatures);
            if (!string.IsNullOrEmpty(errors)) return errors;

            return String.Format(AlterColumn, Quoter.QuoteTableName(expression.TableName), Column.Generate(expression.Column), CommandDelimiter);
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            return string.Join(MultilineDelimiter, expression.ColumnNames.Select(columnName => String.Format(DropColumn, Quoter.QuoteTableName(expression.TableName), Quoter.QuoteColumnName(columnName), CommandDelimiter)).ToArray());
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return String.Format(RenameColumn, 
                Quoter.QuoteTableName(expression.TableName), 
                Quoter.QuoteColumnName(expression.OldName), 
                Quoter.QuoteColumnName(expression.NewName),
                CommandDelimiter
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

            return String.Format(CreateIndex
                , GetUniqueString(expression)
                , GetClusterTypeString(expression)
                , Quoter.QuoteIndexName(expression.Index.Name)
                , Quoter.QuoteTableName(expression.Index.TableName)
                , String.Join(", ", indexColumns)
                , CommandDelimiter);
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            return String.Format(DropIndex, Quoter.QuoteIndexName(expression.Index.Name), Quoter.QuoteTableName(expression.Index.TableName), CommandDelimiter);
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
            return string.Format(
                CreateForeignKeyConstraint,
                Quoter.QuoteTableName(expression.ForeignKey.ForeignTable),
                Quoter.QuoteColumnName(keyName),
                String.Join(", ", foreignColumns.ToArray()),
                Quoter.QuoteTableName(expression.ForeignKey.PrimaryTable),
                String.Join(", ", primaryColumns.ToArray()),
                FormatCascade("DELETE", expression.ForeignKey.OnDelete),
                FormatCascade("UPDATE", expression.ForeignKey.OnUpdate),
                CommandDelimiter
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

            return string.Format(CreateConstraint, Quoter.QuoteTableName(expression.Constraint.TableName),
                Quoter.Quote(expression.Constraint.ConstraintName),
                constraintType,
                String.Join(", ", columns),
                CommandDelimiter);
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            return string.Format(DeleteConstraint, Quoter.QuoteTableName(expression.Constraint.TableName), Quoter.Quote(expression.Constraint.ConstraintName), CommandDelimiter);
        }

        public virtual string GenerateForeignKeyName(CreateForeignKeyExpression expression)
        {
            return string.Format("FK_{0}_{1}", expression.ForeignKey.PrimaryTable.Substring(0, 5), expression.ForeignKey.ForeignTable.Substring(0, 5));
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            if (expression.ForeignKey.ForeignTable == null)
                throw new ArgumentNullException("Table name not specified, ensure you have appended the OnTable extension. Format should be Delete.ForeignKey(KeyName).OnTable(TableName)");

            return string.Format(DeleteConstraint, Quoter.QuoteTableName(expression.ForeignKey.ForeignTable), Quoter.QuoteColumnName(expression.ForeignKey.Name), CommandDelimiter);
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
            string errors = ValidateAdditionalFeatureCompatibility(expression.AdditionalFeatures);
            if (!string.IsNullOrEmpty(errors)) return errors;

            List<string> columnNames = new List<string>();
            List<string> columnValues = new List<string>();
            List<string> insertStrings = new List<string>();

            foreach (IDataDefinition row in expression.Rows)
            {
                IEnumerable<IDataValue> columnData = evaluator.Evaluate(row);

                columnNames.Clear();
                columnValues.Clear();

                foreach (IDataValue item in columnData)
                {
                    columnNames.Add(Quoter.QuoteColumnName(item.ColumnName));
                    columnValues.Add(Quoter.QuoteDataValue(item));
                }

                string columns = String.Join(", ", columnNames.ToArray());
                string values = String.Join(", ", columnValues.ToArray());
                insertStrings.Add(String.Format(InsertData, Quoter.QuoteTableName(expression.TableName), columns, values, CommandDelimiter));
            }
            return String.Join(MultilineDelimiter, insertStrings.ToArray());
        }

        private string ValidateAdditionalFeatureCompatibility(IEnumerable<KeyValuePair<string, object>> features)
        {
            if (compatabilityMode == CompatabilityMode.STRICT) {
                List<string> unsupportedFeatures =
                    features.Where(x => !IsAdditionalFeatureSupported(x.Key)).Select(x => x.Key).ToList();

                if (unsupportedFeatures.Any()) {
                    string errorMessage =
                        string.Format(
                            "The following database specific additional features are not supported in strict mode [{0}]",
                            unsupportedFeatures.Aggregate((x, y) => x + ", " + y));
                    {
                        return compatabilityMode.HandleCompatabilty(errorMessage);
                    }
                }
            }
            return string.Empty;
        }

        private string EvaluateSet(IEnumerable<IDataDefinition> dataDefinitions)
        {
            IEnumerable<IDataValue> columnData = dataDefinitions.SelectMany(dataDefinition => evaluator.Evaluate(dataDefinition));
            IEnumerable<string> setData = columnData.Select(data => string.Format(SetData, Quoter.QuoteColumnName(data.ColumnName), Quoter.QuoteDataValue(data)));

            return string.Join(", ", setData.ToArray());
        }

        private string EvaluateWhere(IEnumerable<IDataDefinition> dataDefinitions)
        {
            return string.Join(" OR ", dataDefinitions.Select(
                dataDefinition => string.Join(" AND ", evaluator.Evaluate(dataDefinition).Select(data => string.Format(BinaryExpression, Quoter.QuoteColumnName(data.ColumnName), Quoter.ComparisonFor(data.Value), Quoter.QuoteValue(data.Value))).ToArray())
            ).ToArray());
        }

        public override string Generate(UpdateDataExpression expression)
        {
            string setClause = EvaluateSet(expression.Set);
            string whereClause = expression.IsAllRows ? "1 = 1" : EvaluateWhere(expression.Where);

            return String.Format(UpdateData, Quoter.QuoteTableName(expression.TableName), setClause, whereClause, CommandDelimiter);
        }

        public override string Generate(DeleteDataExpression expression)
        {
            var deleteItems = new List<string>();


            if (expression.IsAllRows)
            {
                deleteItems.Add(string.Format(DeleteData, Quoter.QuoteTableName(expression.TableName), "1 = 1", CommandDelimiter));
            }
            else
            {
                foreach (var row in expression.Rows)
                {
                    IEnumerable<IDataValue> columnData = evaluator.Evaluate(row);

                    var whereClauses = new List<string>();

                    foreach (IDataValue item in columnData)
                    {
                        whereClauses.Add(string.Format("{0} {1} {2}", Quoter.QuoteColumnName(item.ColumnName), item.Value == null ? "IS" : "=", Quoter.QuoteDataValue(item)));
                    }

                    deleteItems.Add(string.Format(DeleteData, Quoter.QuoteTableName(expression.TableName), String.Join(" AND ", whereClauses.ToArray()), CommandDelimiter));
                }
            }

            return String.Join(MultilineDelimiter, deleteItems.ToArray());
        }


        //All Schema method throw by default as only Sql server 2005 and up supports them.
        public override string Generate(CreateSchemaExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Schemas are not supported");

        }

        public override string Generate(DeleteSchemaExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Schemas are not supported");
        }

        public override string Generate(AlterSchemaExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Schemas are not supported");
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

    }
}
