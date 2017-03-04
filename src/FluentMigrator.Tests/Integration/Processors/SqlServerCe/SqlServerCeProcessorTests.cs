using System;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.SqlServerCe
{

    [TestFixture]
    [Category("Integration")]
    public class SqlServerCeProcessorTests : IntegrationTestBase
    {

        [Test]
        public void CallingSchemaExistsReturnsTrueAlways()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                processor.SchemaExists("NOTUSED").ShouldBeTrue();
            });
        }

        [Test]
        public void CallingExecuteWithMultilineSqlShouldExecuteInBatches()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                processor.Execute("CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL);" + Environment.NewLine +
                              "GO" + Environment.NewLine +
                              "INSERT INTO TestTable1 VALUES('abc', 1);");

                processor.TableExists("NOTUSED", "TestTable1");

                var dataset = processor.ReadTableData("NOTUSED", "TestTable1");
                dataset.Tables[0].Rows.Count.ShouldBe(1);
            }, cleanUpAfterwords: true);
        }

        [Test]
        public void CallingExecuteWithMultilineSqlAsLowercaseShouldExecuteInBatches()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                processor.Execute("create table [TestTable1] ([TestColumn1] nvarchar(255) not null, [TestColumn2] int not null);" + Environment.NewLine +
                              "go" + Environment.NewLine +
                              "insert into testtable1 values('abc', 1);");

                processor.TableExists("NOTUSED", "TestTable1");

                var dataset = processor.ReadTableData("NOTUSED", "TestTable1");
                dataset.Tables[0].Rows.Count.ShouldBe(1);
            }, cleanUpAfterwords: true);
        }

        [Test]
        public void CallingExecuteWithMultilineSqlWithNoTrailingSemicolonShouldExecuteInBatches()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                processor.Execute("CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL);" + Environment.NewLine +
                              "GO" + Environment.NewLine +
                              "INSERT INTO TestTable1 VALUES('abc', 1)");

                processor.TableExists("NOTUSED", "TestTable1");

                var dataset = processor.ReadTableData("NOTUSED", "TestTable1");
                dataset.Tables[0].Rows.Count.ShouldBe(1);
            }, cleanUpAfterwords: true);
        }
    }
}