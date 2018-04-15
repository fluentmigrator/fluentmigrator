using System.Collections.Generic;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.Constraint
{
    public class CreateConstraintExpressionBuilder : ExpressionBuilderBase<CreateConstraintExpression>,
        ICreateConstraintOnTableSyntax,
        ICreateConstraintWithSchemaOrColumnSyntax,
        ICreateConstraintOptionsSyntax,
        ISupportAdditionalFeatures
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CreateConstraintExpressionBuilder"/> class.
        /// </summary>
        public CreateConstraintExpressionBuilder(CreateConstraintExpression expression)
            : base(expression)
        {
        }

        public IDictionary<string, object> AdditionalFeatures => Expression.Constraint.AdditionalFeatures;

        public ICreateConstraintWithSchemaOrColumnSyntax OnTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
            return this;
        }

        public ICreateConstraintOptionsSyntax Column(string columnName)
        {
            Expression.Constraint.Columns.Add(columnName);
            return this;
        }

        public ICreateConstraintOptionsSyntax Columns(params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                Expression.Constraint.Columns.Add(columnName);
            }
            return this;
        }

        public ICreateConstraintColumnsSyntax WithSchema(string schemaName)
        {
            Expression.Constraint.SchemaName = schemaName;
            return this;
        }
    }
}
