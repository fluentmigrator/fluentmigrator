using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Execute
{
	public class ExecuteExpressionRoot : IExecuteExpressionRoot
	{
		private readonly IMigrationContext _context;

		public ExecuteExpressionRoot(IMigrationContext context)
		{
			_context = context;
		}

		public void Sql(string sqlStatement)
		{
			var expression = new ExecuteSqlStatementExpression {SqlStatement = sqlStatement};
			_context.Expressions.Add(expression);
		}

		public void Script(string sqlScript)
		{
			var expression = new ExecuteSqlScriptExpression {SqlScript = sqlScript};
			_context.Expressions.Add(expression);
		}
	}
}