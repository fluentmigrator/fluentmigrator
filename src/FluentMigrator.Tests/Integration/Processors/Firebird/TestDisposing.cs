using System;
using System.Data;
using FirebirdSql.Data.FirebirdClient;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Firebird;
using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
	[Trait("Category", "Integration")]
    [Trait("DbEngine", "Firebird")]
    public class TestDisposing : IDisposable
	{
		private FbConnection _connection;
		private FirebirdProcessor _processor;

		public TestDisposing()
		{
            FbDatabase.CreateDatabase(IntegrationTestOptions.Firebird.ConnectionString);

			_connection = new FbConnection(IntegrationTestOptions.Firebird.ConnectionString);
			_processor = MakeProcessor();
			_connection.Open();
			_processor.BeginTransaction();
		}

		public void Dispose()
		{
			if (!_processor.WasCommitted)
				_processor.CommitTransaction();
			_connection.Close();

            FbDatabase.DropDatabase(IntegrationTestOptions.Firebird.ConnectionString);
		}

		[Fact]
		public void Dispose_WasCommited_ShouldNotRollback()
		{
			var createTable = new CreateTableExpression { TableName = "silly" };
			createTable.Columns.Add(new Model.ColumnDefinition { Name = "one", Type = DbType.Int32 });
			_processor.Process(createTable);

			// this will close the connection
			_processor.CommitTransaction();
			// and this will reopen it again causing Dispose->RollbackTransaction not to throw
			var tableExists = _processor.TableExists("", createTable.TableName); 
			tableExists.ShouldBeTrue();

			// Now dispose (->RollbackTransaction)
			_processor.Dispose();

			_processor = MakeProcessor();

			// Check that the table still exists after dispose
			_processor.TableExists("", createTable.TableName).ShouldBeTrue();
		}

		private FirebirdProcessor MakeProcessor()
		{
			var options = FirebirdOptions.AutoCommitBehaviour();
			return new FirebirdProcessor(_connection, new FirebirdGenerator(options), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new FirebirdDbFactory(), options);
		}

	}
}

