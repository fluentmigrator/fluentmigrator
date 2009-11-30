using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
	public interface IMigrationExpression : ICanBeValidated
	{
		void ExecuteWith(IMigrationProcessor processor);
		IMigrationExpression Reverse();
	   void ApplyConventions( IMigrationConventions conventions );
	}
}