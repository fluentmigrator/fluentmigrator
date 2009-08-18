using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Insert
{
    public class InsertExpressionRoot: IInsertExpressionRoot
    {
        private readonly IMigrationContext _context;

		public InsertExpressionRoot(IMigrationContext context)
		{
			_context = context;
		}

        public IInsertDataSyntax IntoTable(string tableName)
        {            
            var expression = new InsertDataExpression { TableName = tableName };
            _context.Expressions.Add(expression);
            return new InsertDataExpressionBuilder(expression);
        }
    }
}