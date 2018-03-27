using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Runner.Generators.Generic
{
    public abstract class GenericDescriptionGenerator : IDescriptionGenerator
    {
        protected abstract string GenerateTableDescription(
            string schemaName, string tableName, string tableDescription);
        protected abstract string GenerateColumnDescription(
            string schemaName, string tableName, string columnName, string columnDescription);

        public virtual IEnumerable<string> GenerateDescriptionStatements(Expressions.CreateTableExpression expression)
        {
            var statements = new List<string>();

            if (!string.IsNullOrEmpty(expression.TableDescription))
                statements.Add(GenerateTableDescription(expression.SchemaName, expression.TableName, expression.TableDescription));

            foreach (var column in expression.Columns)
            {
                if (string.IsNullOrEmpty(column.ColumnDescription))
                    continue;

                statements.Add(GenerateColumnDescription(
                    expression.SchemaName,
                    expression.TableName,
                    column.Name,
                    column.ColumnDescription));
            }

            return statements;
        }

        public virtual string GenerateDescriptionStatement(Expressions.AlterTableExpression expression)
        {
            if (string.IsNullOrEmpty(expression.TableDescription))
                return string.Empty;

            return GenerateTableDescription(
                expression.SchemaName, expression.TableName, expression.TableDescription);
        }

        public virtual string GenerateDescriptionStatement(Expressions.CreateColumnExpression expression)
        {
            if (string.IsNullOrEmpty(expression.Column.ColumnDescription))
                return string.Empty;

            return GenerateColumnDescription(
                expression.SchemaName, expression.TableName, expression.Column.Name, expression.Column.ColumnDescription);
        }

        public virtual string GenerateDescriptionStatement(Expressions.AlterColumnExpression expression)
        {
            if (string.IsNullOrEmpty(expression.Column.ColumnDescription))
                return string.Empty;

            return GenerateColumnDescription(expression.SchemaName, expression.TableName, expression.Column.Name, expression.Column.ColumnDescription);
        }
    }
}
