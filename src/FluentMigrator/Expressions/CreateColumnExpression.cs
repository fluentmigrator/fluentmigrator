using System;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
	public class CreateColumnExpression : IMigrationExpression
	{
		public string Table { get; set; }
		public ColumnDefinition Column { get; set; }

		public CreateColumnExpression()
		{
			Column = new ColumnDefinition();
		}

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}