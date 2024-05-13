using System.Collections.Generic;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.Constraint
{
    /// <summary>
    /// An expression builder for a <see cref="CreateConstraintExpression"/>
    /// </summary>
    public class CreateConstraintExpressionBuilder : ExpressionBuilderBase<CreateConstraintExpression>,
        ICreateConstraintOnTableSyntax,
        ICreateConstraintWithSchemaOrColumnSyntax,
        ICreateConstraintOptionsSyntax,
        ISupportAdditionalFeatures,
        ICreateConstraintColumnsOptionsSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CreateConstraintExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        public CreateConstraintExpressionBuilder(CreateConstraintExpression expression, IMigrationContext context, IMigration migration)
            : base(expression)
        {
            _context = context;
            _migration = migration;
        }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures => Expression.Constraint.AdditionalFeatures;

        /// <summary>
        /// The context where the expression was added
        /// </summary>
        private readonly IMigrationContext _context;

        /// <summary>
        /// The base migration instance
        /// </summary>
        private readonly IMigration _migration;

        /// <inheritdoc />
        public ICreateConstraintWithSchemaOrColumnSyntax OnTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
            return this;
        }

        /// <inheritdoc />
        public ICreateConstraintOptionsSyntax Column(string columnName)
        {
            Expression.Constraint.Columns.Add(columnName);
            return this;
        }

        /// <inheritdoc />
        public ICreateConstraintOptionsSyntax Columns(params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                Expression.Constraint.Columns.Add(columnName);
            }
            return this;
        }

        /// <inheritdoc />
        public ICreateConstraintColumnsSyntax WithSchema(string schemaName)
        {
            Expression.Constraint.SchemaName = schemaName;
            return this;
        }

        /// <inheritdoc />
        public ICreateConstraintColumnsOptionsSyntax WithOptions()
        {
            return this;
        }

        /// <inheritdoc />
        public  ICreateConstraintOptionsSyntax IfNotExists()
        {
            var exists = _migration.Schema
                .Schema(Expression.Constraint.SchemaName)
                .Table(Expression.Constraint.TableName)
                .Constraint(Expression.Constraint.ConstraintName)
                .Exists();
            if (exists) _context.Expressions.Remove(Expression);
            return this;
        }
    }
}
