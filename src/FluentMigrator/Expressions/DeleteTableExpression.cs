using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
	public class DeleteTableExpression : IMigrationExpression
	{
		public string Table { get; set; }

		public DeleteTableExpression(string name)
		{
			Table = name;
		}

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}