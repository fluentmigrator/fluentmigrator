using System;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Integration.TestCases;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class PerformDBOperationTests : IntegrationTestBase
    {
        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<
            SqlServerProcessor
        >))]
        public void CanCreateAndDeleteTableUsingThePerformDBOperationExpressions(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                _ => { },
                (_, processor) => processor.Process(new PerformDBOperationExpression
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
                }),
                serverOptions,
                true);
        }
    }
}
