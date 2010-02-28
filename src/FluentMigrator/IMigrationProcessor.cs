using System.Data;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;

namespace FluentMigrator
{
	public interface IMigrationProcessor : IQuerySchema
	{
		void Execute(string template, params object[] args);
		DataSet ReadTableData(string tableName);
		DataSet Read(string template, params object[] args);
		bool Exists(string template, params object[] args);
		void CommitTransaction();
		void RollbackTransaction();

		void Process(CreateTableExpression expression);
		void Process(CreateColumnExpression expression);
		void Process(DeleteTableExpression expression);
		void Process(DeleteColumnExpression expression);
		void Process(CreateForeignKeyExpression expression);
		void Process(DeleteForeignKeyExpression expression);
		void Process(CreateIndexExpression expression);
		void Process(DeleteIndexExpression expression);
		void Process(RenameTableExpression expression);
		void Process(RenameColumnExpression expression);
		void Process(InsertDataExpression expression);
	}
}