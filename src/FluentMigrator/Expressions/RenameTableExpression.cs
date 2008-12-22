using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
	public class RenameTableExpression : MigrationExpressionBase
	{
		public virtual string OldName { get; set; }
		public virtual string NewName { get; set; }

		public override void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(OldName))
				errors.Add(ErrorMessages.OldTableNameCannotBeNullOrEmpty);

			if (String.IsNullOrEmpty(NewName))
				errors.Add(ErrorMessages.NewTableNameCannotBeNullOrEmpty);
		}

		public override void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}

		public override IMigrationExpression Reverse()
		{
			return new RenameTableExpression { OldName = NewName, NewName = OldName };
		}
	}
}