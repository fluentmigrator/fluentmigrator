using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Processors;

namespace FluentMigrator.Runner.Processors
{
	public class RenameColumnExpression : IMigrationExpression
	{
		public virtual string OldName { get; set; }
		public virtual string NewName { get; set; }

		public void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(OldName))
				errors.Add(String.Format("The {0} does not have a valid old column name", OldName));

			if (String.IsNullOrEmpty(NewName))
				errors.Add(String.Format("The {0} does not have a valid new column name", NewName));

		}

		public void ExecuteWith(IMigrationProcessor processor)
		{
			throw new System.NotImplementedException();
		}
	}
}