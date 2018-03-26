using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.SqlServerCe
{
    [TestFixture]
    [Category("Integration")]
    public class SqlServerCeColumnTests : BaseColumnTests
    {
        [Test]
        public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, new SqlServerQuoter().QuoteColumnName("i'd") + " int"))
                    processor.ColumnExists("NOTUSED", table.Name, "i'd").ShouldBeTrue();
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, new SqlServerQuoter().QuoteColumnName("id") + " int"))
                    processor.ColumnExists("NOTUSED", table.Name, "id").ShouldBeTrue();
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                    processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                    processor.ColumnExists("NOTUSED", table.Name, "DoesNotExist").ShouldBeFalse();
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                processor.ColumnExists("NOTUSED", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, new SqlServerQuoter().QuoteColumnName("id") + " int"))
                    processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, new SqlServerQuoter().QuoteColumnName("id") + " int"))
                    processor.ColumnExists("NOTUSED", table.Name, "id").ShouldBeTrue();
            }, cleanUpAfterwords: true);
        }

    }
}
