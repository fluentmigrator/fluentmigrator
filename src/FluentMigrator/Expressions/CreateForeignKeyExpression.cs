using System;
using System.Collections.Generic;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
	public class CreateForeignKeyExpression : IMigrationExpression
	{
		public virtual ForeignKeyDefinition ForeignKey { get; private set; }

		public CreateForeignKeyExpression()
		{
			ForeignKey = new ForeignKeyDefinition();
		}

		public virtual void CollectValidationErrors(ICollection<string> errors)
		{
			ForeignKey.CollectValidationErrors(errors);
		}

		public virtual void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}