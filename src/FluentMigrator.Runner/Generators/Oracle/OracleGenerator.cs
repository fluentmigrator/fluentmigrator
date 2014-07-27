using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Helpers;

namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleGenerator : GenericGenerator
    {

        
        public OracleGenerator()
            : base(new OracleColumn(new OracleQuoter()), new OracleQuoter(), new OracleDescriptionGenerator())
        {
        }

        public OracleGenerator(bool useQuotedIdentifiers)
            : base(new OracleColumn(GetQuoter(useQuotedIdentifiers)), GetQuoter(useQuotedIdentifiers), new OracleDescriptionGenerator())
        {
        }

        private static IQuoter GetQuoter(bool useQuotedIdentifiers)
        {
            return useQuotedIdentifiers ? (IQuoter)new OracleQuoterQuotedIdentifier() : (IQuoter)new OracleQuoter();
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
            return String.Format(DropTable, ExpandTableName(Quoter.QuoteTableName(expression.SchemaName),Quoter.QuoteTableName(expression.TableName)));
        }

        public override string AddColumn
        {
            get { return "ALTER TABLE {0} ADD {1}"; }
        }

        public override string AlterColumn
        {
            get { return "ALTER TABLE {0} MODIFY {1}"; }
        }

        public override string RenameTable
        {
            get { return "ALTER TABLE {0} RENAME TO {1}"; }
        }

        public override string InsertData
        {
            get { return "INTO {0} ({1}) VALUES ({2})"; }
        }

        private string ExpandTableName(string schema, string table)
        { 
            return String.IsNullOrEmpty(schema) ? table : String.Concat(schema,".",table);
        }

         private string innerGenerate(CreateTableExpression expression)
        {
            var tableName = Quoter.QuoteTableName(expression.TableName);
            var schemaName = Quoter.QuoteSchemaName(expression.SchemaName);
             
            return string.Format("CREATE TABLE {0} ({1})",ExpandTableName(schemaName,tableName), Column.Generate(expression.Columns, tableName));
        }


        public override string Generate(CreateTableExpression expression)
        {
            var descriptionStatements = DescriptionGenerator.GenerateDescriptionStatements(expression);
            var statements = descriptionStatements as string[] ?? descriptionStatements.ToArray();

            if (!statements.Any())
                return innerGenerate(expression);

            var wrappedCreateTableStatement = WrapStatementInExecuteImmediateBlock(innerGenerate(expression));
            var createTableWithDescriptionsBuilder = new StringBuilder(wrappedCreateTableStatement);

            foreach (var descriptionStatement in statements)
            {
                if (!string.IsNullOrEmpty(descriptionStatement))
                {
                    var wrappedStatement = WrapStatementInExecuteImmediateBlock(descriptionStatement);
                    createTableWithDescriptionsBuilder.Append(wrappedStatement);
                }
            }

            return WrapInBlock(createTableWithDescriptionsBuilder.ToString());
        }

        public override string Generate(AlterTableExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return base.Generate(expression);

            return descriptionStatement;
        }

        public override string Generate(CreateColumnExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return base.Generate(expression);

            var wrappedCreateColumnStatement = WrapStatementInExecuteImmediateBlock(base.Generate(expression));

            var createColumnWithDescriptionBuilder = new StringBuilder(wrappedCreateColumnStatement);
            createColumnWithDescriptionBuilder.Append(WrapStatementInExecuteImmediateBlock(descriptionStatement));

            return WrapInBlock(createColumnWithDescriptionBuilder.ToString());
        }

        public override string Generate(AlterColumnExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return base.Generate(expression);

            var wrappedAlterColumnStatement = WrapStatementInExecuteImmediateBlock(base.Generate(expression));

            var alterColumnWithDescriptionBuilder = new StringBuilder(wrappedAlterColumnStatement);
            alterColumnWithDescriptionBuilder.Append(WrapStatementInExecuteImmediateBlock(descriptionStatement));

            return WrapInBlock(alterColumnWithDescriptionBuilder.ToString());
        }

        public override string Generate(InsertDataExpression expression)
        {
            var columnNames = new List<string>();
            var columnValues = new List<string>();
            var insertStrings = new List<string>();

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
                insertStrings.Add(String.Format(InsertData, ExpandTableName(Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName)), columns, values));
            }
            return "INSERT ALL " + String.Join(" ", insertStrings.ToArray()) + " SELECT 1 FROM DUAL";
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Default constraints are not supported");
        }

        private string WrapStatementInExecuteImmediateBlock(string statement)
        {
            if (string.IsNullOrEmpty(statement))
                return string.Empty;

            return string.Format("EXECUTE IMMEDIATE '{0}';", FormatHelper.FormatSqlEscape(statement));
        }

        private string WrapInBlock(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                return string.Empty;

            return string.Format("BEGIN {0} END;", sql);
        }
    }
}