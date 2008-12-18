using FluentMigrator.Expressions;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Processors
{
	public interface IMigrationProcessor
	{
		// Tables
		void Process(CreateTableExpression expression);
		void Process(RenameTableExpression expression);
		void Process(DeleteTableExpression expression);

		// Columns
		void Process(CreateColumnExpression expression);
		void Process(RenameColumnExpression expression);
		void Process(DeleteColumnExpression expression);

		// Keys
		void Process(CreateForeignKeyExpression expression);
		void Process(DeleteForeignKeyExpression expression);

		// Indexes
		void Process(CreateIndexExpression expression);
	}
}