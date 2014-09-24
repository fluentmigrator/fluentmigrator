using System;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Hana
{
    public class HanaGenerator : GenericGenerator
    {


        public HanaGenerator()
            : base(new HanaColumn(new HanaQuoter()), new HanaQuoter(), new HanaDescriptionGenerator())
        {
        }

        public override string DropTable
        {
            get
            {
                return "DROP TABLE {0}";
            }
        }
        public override string Generate(DeleteTableExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }

        public override string Generate(RenameTableExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            var result = new StringBuilder(string.Format("CREATE SEQUENCE "));
            var seq = expression.Sequence;

            if (string.IsNullOrEmpty(seq.SchemaName))
            {
                result.AppendFormat(Quoter.QuoteSequenceName(seq.Name));
            }
            else
            {
                result.AppendFormat("{0}.{1}", Quoter.QuoteSchemaName(seq.SchemaName), Quoter.QuoteSequenceName(seq.Name));
            }

            if (seq.Increment.HasValue)
            {
                result.AppendFormat(" INCREMENT BY {0}", seq.Increment);
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

            result.Append(";");


            return result.ToString();
        }

        public override string AddColumn
        {
            get { return "ALTER TABLE {0} ADD ({1})"; }
        }

        public override string AlterColumn
        {
            get { return "ALTER TABLE {0} ALTER ({1})"; }
        }

        public override string DropColumn
        {
            get { return "ALTER TABLE {0} DROP ({1})"; }
        }

        public override string RenameColumn { get { return "RENAME COLUMN {0}.{1} TO {2}"; } }

        private string ExpandTableName(string schema, string table)
        {
            return String.IsNullOrEmpty(schema) ? table : String.Concat(schema, ".", table);
        }

        public override string Generate(DeleteDataExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }

        public override string Generate(InsertDataExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }

        private string InnerGenerate(CreateTableExpression expression)
        {
            var tableName = Quoter.QuoteTableName(expression.TableName);
            var schemaName = Quoter.QuoteSchemaName(expression.SchemaName);

            return string.Format("CREATE COLUMN TABLE {0} ({1});", ExpandTableName(schemaName, tableName), Column.Generate(expression.Columns, tableName));
        }

        public override string Generate(UpdateDataExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }

        public override string Generate(CreateTableExpression expression)
        {
            var descriptionStatements = DescriptionGenerator.GenerateDescriptionStatements(expression);
            var statements = descriptionStatements as string[] ?? descriptionStatements.ToArray();

            if (!statements.Any())
                return InnerGenerate(expression);

            var wrappedCreateTableStatement = InnerGenerate(expression);
            var createTableWithDescriptionsBuilder = new StringBuilder(wrappedCreateTableStatement);

            foreach (var descriptionStatement in statements)
            {
                if (!string.IsNullOrEmpty(descriptionStatement))
                {
                    createTableWithDescriptionsBuilder.Append(descriptionStatement);
                }
            }

            return WrapInBlock(createTableWithDescriptionsBuilder.ToString());
        }

        public override string Generate(AlterTableExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            return string.Format("{0};",
                string.IsNullOrEmpty(descriptionStatement)
                ? base.Generate(expression) : descriptionStatement);;
        }

        public override string Generate(CreateColumnExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return string.Format("{0};",base.Generate(expression) );

            var wrappedCreateColumnStatement = base.Generate(expression);

            var createColumnWithDescriptionBuilder = new StringBuilder(wrappedCreateColumnStatement);
            createColumnWithDescriptionBuilder.Append(descriptionStatement);

            return WrapInBlock(createColumnWithDescriptionBuilder.ToString());
        }

        public override string Generate(AlterColumnExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return string.Format("{0};", base.Generate(expression));

            var wrappedAlterColumnStatement = base.Generate(expression);

            var alterColumnWithDescriptionBuilder = new StringBuilder(wrappedAlterColumnStatement);
            alterColumnWithDescriptionBuilder.Append(descriptionStatement);

            return WrapInBlock(alterColumnWithDescriptionBuilder.ToString());
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));            
        }
        public override string Generate(CreateConstraintExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Default constraints are not supported");
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Default constraints are not supported");
        }

        public override string Generate(CreateIndexExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }

        private static string WrapInBlock(string sql)
        {
            return string.IsNullOrEmpty(sql)
                ? string.Empty
                : string.Format("BEGIN {0} END;", sql);
        }
    }
}