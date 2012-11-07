using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleGenerator : GenericGenerator
    {
        public OracleGenerator() : base(new OracleColumn(), new OracleQuoter(), new GenericEvaluator()) { }

        public override string MultilineDelimiter { get { return ";\r\n"; } }

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

        public override string Generate(InsertDataExpression expression)
        {
            var insertStrings = new List<string>();

            foreach (IDataDefinition row in expression.Rows)
            {
                IEnumerable<IDataValue> dataValues = evaluator.Evaluate(row).ToArray();

                string columns = String.Join(", ", dataValues.Select(dataValue => Quoter.QuoteColumnName(dataValue.ColumnName)).ToArray());
                string values = String.Join(", ", dataValues.Select(dataValue => Quoter.QuoteDataValue(dataValue)).ToArray());

                insertStrings.Add(String.Format(InsertData, Quoter.QuoteTableName(expression.TableName), columns, values, CommandDelimiter));
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

        public override string Generate(DeleteIndexExpression expression)
        {
            return String.Format(DropIndex, Quoter.QuoteIndexName(expression.Index.Name), CommandDelimiter);
        }
    }
}