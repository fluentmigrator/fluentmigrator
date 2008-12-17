using System;
using System.Collections.Generic;

namespace FluentMigrator.Expressions
{
	public class RenameTableExpression : IMigrationExpression
	{
		public string OldName { get; set; }
		public string NewName { get; set; }

		public void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(OldName))
				errors.Add(String.Format("The {0} does not have a valid old table name", GetType().Name));

			if (String.IsNullOrEmpty(NewName))
				errors.Add(String.Format("The {0} does not have a valid new table name", GetType().Name));
		}

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}