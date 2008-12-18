using System;
using System.Data.SqlClient;
using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Processors
{
	public class SqliteMigrationProcessor : IMigrationProcessor
	{
		public SqlConnection Connection { get; set; }

		public SqliteMigrationProcessor(SqlConnection connection)
		{
			Connection = connection;
		}

		public void Process(CreateTableExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(CreateColumnExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(RenameColumnExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(DeleteTableExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(DeleteColumnExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(CreateForeignKeyExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(DeleteForeignKeyExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(DeleteIndexExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(RenameTableExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(CreateIndexExpression expression)
		{
			throw new NotImplementedException();
		}
	}
}