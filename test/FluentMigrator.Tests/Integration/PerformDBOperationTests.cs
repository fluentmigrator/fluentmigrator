using FluentMigrator.Expressions;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class PerformDBOperationTests : IntegrationTestBase
    {
        [Test]
        [Category("SqlServer2008")]
        [Category("SqlServer2012")]
        [Category("SqlServer2014")]
        public void CanCreateAndDeleteTableUsingThePerformDBOperationExpressions()
        {
            if (!IntegrationTestOptions.SqlServer2008.IsEnabled
             && !IntegrationTestOptions.SqlServer2012.IsEnabled
             && !IntegrationTestOptions.SqlServer2014.IsEnabled)
            {
                Assert.Ignore("No processor found for the given action.");
            }

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

            if (IntegrationTestOptions.SqlServer2008.IsEnabled)
            {
                ExecuteWithSqlServer2008(processor => processor.Process(expression), true, IntegrationTestOptions.SqlServer2008);
            }

            if (IntegrationTestOptions.SqlServer2012.IsEnabled)
            {
                ExecuteWithSqlServer2012(processor => processor.Process(expression), true, IntegrationTestOptions.SqlServer2012);
            }

            if (IntegrationTestOptions.SqlServer2014.IsEnabled)
            {
                ExecuteWithSqlServer2014(processor => processor.Process(expression), true, IntegrationTestOptions.SqlServer2014);
            }
        }
    }
}
