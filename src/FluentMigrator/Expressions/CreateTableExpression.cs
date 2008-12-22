using System;
using System.Collections.Generic;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
	public class CreateTableExpression : MigrationExpressionBase
	{
		public virtual string TableName { get; set; }
		public virtual IList<ColumnDefinition> Columns { get; set; }

		public CreateTableExpression()
		{
			Columns = new List<ColumnDefinition>();
		}

		public override void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(TableName))
				errors.Add(String.Format("The {0} does not have a valid table name", GetType().Name));

			foreach (ColumnDefinition column in Columns)
				column.CollectValidationErrors(errors);
		}

		public override void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}

		public override IMigrationExpression Reverse()
		{
			return new DeleteTableExpression { TableName = TableName };
		}
	}
}