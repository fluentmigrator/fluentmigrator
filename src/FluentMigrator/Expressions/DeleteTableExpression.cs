using System;
using System.Collections.Generic;

namespace FluentMigrator.Expressions
{
	public class DeleteTableExpression : IMigrationExpression
	{
		public string TableName { get; set; }

		public void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(TableName))
				errors.Add(String.Format("The {0} does not have a valid table name", GetType().Name));
		}

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}