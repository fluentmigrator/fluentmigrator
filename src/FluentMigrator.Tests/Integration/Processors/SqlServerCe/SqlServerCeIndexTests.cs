using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.SqlServerCe
{
    [TestFixture]
    [Category("Integration")]
    public class SqlServerCeIndexTests : BaseIndexTests
    {
        [Test]
        public override void CallingIndexExistsCanAcceptIndexNameWithSingleQuote()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                {
                    table.WithIndexOn("id", "UI'id");
                    processor.IndexExists("NOTUSED", table.Name, "UI'id").ShouldBeTrue();
                }
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingIndexExistsCanAcceptTableNameWithSingleQuote()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable("Test'Table", processor as SqlServerCeProcessor, "id int"))
                {
                    table.WithIndexOn("id");
                    processor.IndexExists("NOTUSED", table.Name, "UI_id").ShouldBeTrue();
                }
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                {
                    table.WithIndexOn("id");
                    processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
                }
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                {
                    table.WithIndexOn("id");
                    processor.IndexExists("NOTUSED", table.Name, "DoesNotExist").ShouldBeFalse();
                }
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExist()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                processor.IndexExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                processor.IndexExists("NOTUSED", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }


        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExists()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                {
                    table.WithIndexOn("id");
                    processor.IndexExists(null, table.Name, "UI_id").ShouldBeTrue();
                }
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                {
                    table.WithIndexOn("id");
                    processor.IndexExists("NOTUSED", table.Name, "UI_id").ShouldBeTrue();
                }
            }, cleanUpAfterwords: true);
        }
    }
}
