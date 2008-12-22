using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
	public class CreateColumnExpression : MigrationExpressionBase
	{
		public virtual string TableName { get; set; }
		public virtual ColumnDefinition Column { get; set; }

		public CreateColumnExpression()
		{
			Column = new ColumnDefinition();
		}

		public override void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(TableName))
				errors.Add(ErrorMessages.TableNameCannotBeNullOrEmpty);

			Column.CollectValidationErrors(errors);
		}

		public override void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}

		public override IMigrationExpression Reverse()
		{
			return new DeleteColumnExpression { TableName = TableName, ColumnName = Column.Name };
		}
	}
}