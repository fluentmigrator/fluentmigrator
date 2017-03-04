using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.SqlServerCe
{
    [TestFixture]
    [Category("Integration")]
    public class SqlServerCeTableTests : BaseTableTests
    {
        [Test]
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable("Test'Table", processor as SqlServerCeProcessor, "id int"))
                    processor.TableExists("NOTUSED", table.Name).ShouldBeTrue();
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                processor.TableExists("NOTUSED", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                    processor.TableExists(null, table.Name).ShouldBeTrue();
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                    processor.TableExists("NOTUSED", table.Name).ShouldBeTrue();
            }, cleanUpAfterwords: true);
        }
    }
}
