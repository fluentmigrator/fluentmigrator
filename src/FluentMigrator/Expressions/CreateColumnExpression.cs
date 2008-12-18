using System;
using System.Collections.Generic;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
	public class CreateColumnExpression : IMigrationExpression
	{
		public virtual string TableName { get; set; }
		public virtual ColumnDefinition Column { get; set; }

		public CreateColumnExpression()
		{
			Column = new ColumnDefinition();
		}

		public virtual void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(TableName))
				errors.Add(String.Format("The {0} does not have a valid table name", GetType().Name));

			Column.CollectValidationErrors(errors);
		}

		public virtual void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}