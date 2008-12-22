using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
	public class DeleteColumnExpression : MigrationExpressionBase
	{
		public virtual string TableName { get; set; }
		public virtual string ColumnName { get; set; }

		public override void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(TableName))
				errors.Add(ErrorMessages.TableNameCannotBeNullOrEmpty);

			if (String.IsNullOrEmpty(ColumnName))
				errors.Add(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
		}

		public override void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}