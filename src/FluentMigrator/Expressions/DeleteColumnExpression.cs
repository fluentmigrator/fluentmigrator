using System;

namespace FluentMigrator.Expressions
{
	public class DeleteColumnExpression : IMigrationExpression
	{
		public string TableName { get; set; }
		public string ColumnName { get; set; }

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}