using System;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
	public class CreateColumnExpression : IMigrationExpression
	{
		public string Table { get; set; }
		public ColumnDefinition Column { get; set; }

		public CreateColumnExpression(string name)
		{
			Column = new ColumnDefinition(name);
		}

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}