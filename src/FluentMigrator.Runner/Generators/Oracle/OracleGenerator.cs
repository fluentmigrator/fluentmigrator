using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleGenerator : GenericGenerator
    {
        public OracleGenerator()
            : base(new OracleColumn(), new OracleQuoter())
        {
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
                insertStrings.Add(String.Format(InsertData, Quoter.QuoteTableName(expression.TableName), columns, values));
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
    }
}