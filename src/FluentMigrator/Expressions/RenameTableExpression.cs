using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
	public class RenameTableExpression : IMigrationExpression
	{
		public string OldName { get; set; }
		public string NewName { get; set; }

		public RenameTableExpression(string oldName)
		{
			OldName = oldName;
		}

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}
	}
}