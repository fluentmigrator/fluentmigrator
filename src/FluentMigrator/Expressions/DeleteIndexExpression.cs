using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
	public class DeleteIndexExpression : IMigrationExpression
	{
		public virtual string TableName { get; set; }
		public virtual ICollection<string> Columns { get; set; }

		public DeleteIndexExpression()
		{
			Columns = new List<string>();
		}

		public virtual void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(TableName))
				errors.Add(ErrorMessages.TableNameCannotBeNullOrEmpty);

			if (Columns.Count == 0)
				errors.Add(ErrorMessages.IndexMustHaveOneOrMoreColumns);
		}

		public virtual void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}