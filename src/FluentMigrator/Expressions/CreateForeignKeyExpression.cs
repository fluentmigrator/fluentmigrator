using System;
using System.Collections.Generic;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
	public class CreateForeignKeyExpression : IMigrationExpression
	{
		public ForeignKeyDefinition ForeignKey { get; private set; }

		public CreateForeignKeyExpression()
		{
			ForeignKey = new ForeignKeyDefinition();
		}

		public void CollectValidationErrors(ICollection<string> errors)
		{
			ForeignKey.CollectValidationErrors(errors);
		}

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}