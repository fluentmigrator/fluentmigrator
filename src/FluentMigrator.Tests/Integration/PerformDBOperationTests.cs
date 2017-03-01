using Xunit;
using FluentMigrator.Builders.Execute;

namespace FluentMigrator.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class PerformDBOperationTests : IntegrationTestBase
    {
        [Fact]
        public void CanCreateAndDeleteTableUsingThePerformDBOperationExpressions()
        {
            var expression = new PerformDBOperationExpression
            {
                Operation = (connection, transaction) =>
                {
                    // I know I could be using the expressions to create and delete this table,
                    // but really I just want to test whether I can execute some commands against the connection.

                    var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = "CREATE TABLE dbo.TestTable(TestTableID int NULL)";

                    command.ExecuteNonQuery();

                    var command2 = connection.CreateCommand();
                    command2.Transaction = transaction;
                    command2.CommandText = "DROP TABLE dbo.TestTable";

                    command2.ExecuteNonQuery();


                }
            };

            ExecuteWithSqlServer2008(processor => processor.Process(expression), true);
            ExecuteWithSqlServer2012(processor => processor.Process(expression), true);
            ExecuteWithSqlServer2014(processor => processor.Process(expression), true);
        }
    }
}