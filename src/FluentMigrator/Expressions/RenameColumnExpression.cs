using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
	public class RenameColumnExpression : IMigrationExpression
	{
		public virtual string OldName { get; set; }
		public virtual string NewName { get; set; }

		public void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(OldName))
				errors.Add(ErrorMessages.OldColumnNameCannotBeNullOrEmpty);

			if (String.IsNullOrEmpty(NewName))
				errors.Add(ErrorMessages.NewColumnNameCannotBeNullOrEmpty);
		}

		public virtual void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}