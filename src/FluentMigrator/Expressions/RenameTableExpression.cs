using System;

namespace FluentMigrator.Expressions
{
	public class RenameTableExpression : IMigrationExpression
	{
		public string OldName { get; set; }
		public string NewName { get; set; }

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}