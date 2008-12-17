using System;
using System.Collections.Generic;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
	public class CreateTableExpression : IMigrationExpression
	{
		public string TableName { get; set; }
		public IList<ColumnDefinition> Columns { get; private set; }

		public ColumnDefinition CurrentColumn
		{
			get { return Columns[Columns.Count - 1]; }
		}

		public CreateTableExpression()
		{
			Columns = new List<ColumnDefinition>();
		}

		public void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(TableName))
				errors.Add(String.Format("The {0} does not have a valid table name", GetType().Name));

			foreach (ColumnDefinition column in Columns)
				column.CollectValidationErrors(errors);
		}

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}