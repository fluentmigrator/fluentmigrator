using System;
using System.Collections.Generic;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
	public class DeleteIndexExpression : MigrationExpressionBase
	{
		public virtual IndexDefinition Index { get; set; }

		public DeleteIndexExpression()
		{
			Index = new IndexDefinition();
		}

		public override void CollectValidationErrors(ICollection<string> errors)
		{
			Index.CollectValidationErrors(errors);
		}

		public override void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}