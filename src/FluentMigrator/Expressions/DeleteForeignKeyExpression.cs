using System;
using System.Collections.Generic;
using FluentMigrator.Model;
using System.Linq;

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

		public override string ToString()
		{
			return base.ToString() + ForeignKey.Name + " "
				+ ForeignKey.ForeignTable + " (" + string.Join(", ", ForeignKey.ForeignColumns.ToArray()) + ") "
				+ ForeignKey.PrimaryTable + " (" + string.Join(", ", ForeignKey.PrimaryColumns.ToArray()) + ")";
		}
	}
}