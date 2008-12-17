using System;

namespace FluentMigrator.Expressions
{
	public class DeleteTableExpression : IMigrationExpression
	{
		public string TableName { get; set; }

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}