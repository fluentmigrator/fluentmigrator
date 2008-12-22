using System;
using System.Collections.Generic;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
	public class DeleteForeignKeyExpression : MigrationExpressionBase
	{
		public virtual ForeignKeyDefinition ForeignKey { get; set; }

		public DeleteForeignKeyExpression()
		{
			ForeignKey = new ForeignKeyDefinition();
		}

		public override void CollectValidationErrors(ICollection<string> errors)
		{
			ForeignKey.CollectValidationErrors(errors);
		}

		public override void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}