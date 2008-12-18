using System.Collections.Generic;
using FluentMigrator.Model;
using FluentMigrator.Processors;

namespace FluentMigrator.Expressions
{
	public class DeleteForeignKeyExpression : IMigrationExpression
	{
		public virtual ForeignKeyDefinition ForeignKey { get; private set; }

		public DeleteForeignKeyExpression()
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