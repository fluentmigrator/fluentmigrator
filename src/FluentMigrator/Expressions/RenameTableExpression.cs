using System;
using System.Collections.Generic;

namespace FluentMigrator.Expressions
{
	public class RenameTableExpression : IMigrationExpression
	{
		public virtual string OldName { get; set; }
		public virtual string NewName { get; set; }

		public virtual void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(OldName))
				errors.Add(String.Format("The {0} does not have a valid old table name", GetType().Name));

			if (String.IsNullOrEmpty(NewName))
				errors.Add(String.Format("The {0} does not have a valid new table name", GetType().Name));
		}

		public virtual void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}