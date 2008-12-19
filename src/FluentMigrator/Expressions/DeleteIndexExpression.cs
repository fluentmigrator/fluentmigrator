using System;
using System.Collections.Generic;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
	public class DeleteIndexExpression : IMigrationExpression
	{
		public virtual IndexDefinition Index { get; set; }

		public DeleteIndexExpression()
		{
			Index = new IndexDefinition();
		}

		public virtual void CollectValidationErrors(ICollection<string> errors)
		{
			Index.CollectValidationErrors(errors);
		}

		public virtual void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}