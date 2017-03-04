using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.SqlServerCe
{
    [TestFixture]
    [Category("Integration")]
    public class SqlServerCeConstraintTests : BaseConstraintTests
    {
        [Test]
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                {
                    table.WithUniqueConstraintOn("id", "UC'id");
                    processor.ConstraintExists("NOTUSED", table.Name, "UC'id").ShouldBeTrue();
                }
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable("Test'Table", processor as SqlServerCeProcessor, "id int"))
                {
                    table.WithUniqueConstraintOn("id");
                    processor.ConstraintExists("NOTUSED", table.Name, "UC_id").ShouldBeTrue();
                }
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                {
                    table.WithUniqueConstraintOn("id");
                    processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
                }
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                {
                    table.WithUniqueConstraintOn("id");
                    processor.ConstraintExists("NotUsed", table.Name, "DoesNotExist").ShouldBeFalse();
                }
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                processor.ConstraintExists("NotUsed", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                {
                    table.WithUniqueConstraintOn("id");
                    processor.ConstraintExists(null, table.Name, "UC_id").ShouldBeTrue();
                }
            }, cleanUpAfterwords: true);
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            ExecuteFor(MS_SQL_CE, processor =>
            {
                using (var table = new SqlServerCeTestTable(processor as SqlServerCeProcessor, "id int"))
                {
                    table.WithUniqueConstraintOn("id");
                    processor.ConstraintExists("NOTUSED", table.Name, "UC_id").ShouldBeTrue();
                }
            }, cleanUpAfterwords: true);
        }
    }
}
