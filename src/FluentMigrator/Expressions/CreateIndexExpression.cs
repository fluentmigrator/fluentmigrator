using System;
using System.Collections.Generic;
using FluentMigrator.Model;
using System.Linq;

namespace FluentMigrator.Expressions
{
	public class CreateIndexExpression : MigrationExpressionBase
	{
		public virtual IndexDefinition Index { get; set; }

		public CreateIndexExpression()
		{
			Index = new IndexDefinition();
		}

      public override void ApplyConventions(IMigrationConventions conventions)
      {
         Index.ApplyConventions(conventions);
      }

		public override void CollectValidationErrors(ICollection<string> errors)
		{
			Index.CollectValidationErrors(errors);
		}

		public override void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}

		public override IMigrationExpression Reverse()
		{
			return new DeleteIndexExpression { Index = Index.Clone() as IndexDefinition };
		}

		public override string ToString()
		{
			return base.ToString() + Index.TableName + " (" + string.Join(", ", Index.Columns.Select(x => x.Name).ToArray()) + ")";
		}
	}
}