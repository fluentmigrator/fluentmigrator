using System.Data;
using FluentMigrator.Expressions;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    [TestFixture]
	[Category("Integration")]
    [Category("Firebird")]
    public class TestDisposing : IntegrationTestBase
	{
		[Test]
		public void Dispose_WasCommited_ShouldNotRollback()
		{
            var createTable = new CreateTableExpression { TableName = "silly" };
            ExecuteFor(FIREBIRD, processor =>
            {
                createTable.Columns.Add(new Model.ColumnDefinition { Name = "one", Type = DbType.Int32 });
                processor.Process(createTable);

                // this will close the connection
                processor.CommitTransaction();
                // and this will reopen it again causing Dispose->RollbackTransaction not to throw
                var tableExists = processor.TableExists("", createTable.TableName);
                tableExists.ShouldBeTrue();
            });

            // processor is now disposed (->RollbackTransaction)
            // call it again

            ExecuteFor(FIREBIRD, processor =>
            {
                // Check that the table still exists after dispose
                processor.TableExists("", createTable.TableName).ShouldBeTrue();
            });
		}
	}
}
