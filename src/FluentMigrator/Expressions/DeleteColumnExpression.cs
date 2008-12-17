using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
	public class DeleteColumnExpression : IMigrationExpression
	{
		public string Table { get; set; }
		public string Name { get; set; }

		public DeleteColumnExpression(string name)
		{
			Name = name;
		}

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}