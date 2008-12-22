using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
	public class RenameColumnExpression : MigrationExpressionBase
	{
		public virtual string TableName { get; set; }
		public virtual string OldName { get; set; }
		public virtual string NewName { get; set; }

		public override void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(OldName))
				errors.Add(ErrorMessages.OldColumnNameCannotBeNullOrEmpty);

			if (String.IsNullOrEmpty(NewName))
				errors.Add(ErrorMessages.NewColumnNameCannotBeNullOrEmpty);
		}

		public override void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}

		public override IMigrationExpression Reverse()
		{
			return new RenameColumnExpression { TableName = TableName, OldName = NewName, NewName = OldName };
		}
	}
}