using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
	public class ExecuteSqlStatementExpression : MigrationExpressionBase
	{
		public virtual string SqlStatement { get; set;}

		public override void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Execute(SqlStatement);
		}

		public override void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(SqlStatement))
				errors.Add(ErrorMessages.SqlStatementCannotBeNullOrEmpty);
		}

		public override string ToString()
		{
			return base.ToString() + SqlStatement;
		}
	}
}
