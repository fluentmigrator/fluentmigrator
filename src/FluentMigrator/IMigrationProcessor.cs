using System;
using FluentMigrator.Expressions;

namespace FluentMigrator
{
	public interface IMigrationProcessor
	{
		void Process(CreateTableExpression expression);
		void Process(CreateColumnExpression expression);
		void Process(DeleteTableExpression expression);
		void Process(DeleteColumnExpression expression);
		void Process(CreateForeignKeyExpression expression);
		void Process(DeleteForeignKeyExpression expression);
		void Process(RenameTableExpression expression);
	}
}
