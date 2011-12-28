using FluentMigrator.Builders.Delete.Column;
using FluentMigrator.Builders.Delete.DefaultConstraint;
using FluentMigrator.Builders.Delete.ForeignKey;
using FluentMigrator.Builders.Delete.Index;
using FluentMigrator.Builders.Delete.Sequence;
using FluentMigrator.Builders.Delete.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete
{
    public class DeleteExpressionRoot : IDeleteExpressionRoot
    {
        private readonly IMigrationContext _context;

        public DeleteExpressionRoot(IMigrationContext context)
        {
            _context = context;
        }

        public void Schema(string schemaName)
        {
            var expression = new DeleteSchemaExpression {SchemaName = schemaName};
            _context.Expressions.Add(expression);
        }

        public IInSchemaSyntax Table(string tableName)
        {
            var expression = new DeleteTableExpression {TableName = tableName};
            _context.Expressions.Add(expression);
            return new DeleteTableExpressionBuilder(expression);
        }

        public IDeleteColumnFromTableSyntax Column(string columnName)
        {
            var expression = new DeleteColumnExpression {ColumnName = columnName};
            _context.Expressions.Add(expression);
            return new DeleteColumnExpressionBuilder(expression);
        }

        public IDeleteForeignKeyFromTableSyntax ForeignKey()
        {
            var expression = new DeleteForeignKeyExpression();
            _context.Expressions.Add(expression);
            return new DeleteForeignKeyExpressionBuilder(expression);
        }

        public IDeleteForeignKeyOnTableSyntax ForeignKey(string foreignKeyName)
        {
            var expression = new DeleteForeignKeyExpression {ForeignKey = {Name = foreignKeyName}};
            _context.Expressions.Add(expression);
            return new DeleteForeignKeyExpressionBuilder(expression);
        }

        public IDeleteDataOrInSchemaSyntax FromTable(string tableName)
        {
            var expression = new DeleteDataExpression {TableName = tableName};
            _context.Expressions.Add(expression);
            return new DeleteDataExpressionBuilder(expression);
        }

        public IDeleteIndexForTableSyntax Index(string indexName)
        {
            var expression = new DeleteIndexExpression();
            expression.Index.Name = indexName;
            _context.Expressions.Add(expression);
            return new DeleteIndexExpressionBuilder(expression);
        }

        public IDeleteIndexForTableSyntax Index()
        {
            var expression = new DeleteIndexExpression();
            _context.Expressions.Add(expression);
            return new DeleteIndexExpressionBuilder(expression);
        }

        public IInSchemaSyntax Sequence(string sequenceName)
        {
            var expression = new DeleteSequenceExpression {SequenceName = sequenceName};
            _context.Expressions.Add(expression);
            return new DeleteSequenceExpressionBuilder(expression);
        }

        public IDeleteDefaultConstraintOnTableSyntax DefaultConstraint()
        {
            var expression = new DeleteDefaultConstraintExpression();
            _context.Expressions.Add(expression);
            return new DeleteDefaultConstraintExpressionBuilder(expression);
        }
    }
}