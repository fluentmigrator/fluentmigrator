using System;

namespace FluentMigrator.Expressions
{
	public interface IMigrationExpression
	{
		void ExecuteWith(IMigrationProcessor processor);
	}
}