using FluentMigrator.Runner.Processors.Firebird;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    [TestFixture]
    [Category("Integration")]
    [Category("Firebird")]
    public class FirebirdColumnTests : BaseColumnTests
    {
        [Test]
        public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                var columnNameWithSingleQuote = "\"i'd\"";
                using (var table = new FirebirdTestTable(fbProcessor, null, string.Format("{0} int", columnNameWithSingleQuote)))
                    processor.ColumnExists(null, table.Name, "\"i'd\"").ShouldBeTrue();
            });
        }

        [Test]
        public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable("\"Test'Table\"", fbProcessor, null, "id int"))
                    fbProcessor.ColumnExists(null, table.Name, "ID").ShouldBeTrue();
            });
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int"))
                    fbProcessor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, "TestSchema", "id int"))
                    fbProcessor.ColumnExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                processor.ColumnExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int"))
                    fbProcessor.ColumnExists(null, table.Name, "ID").ShouldBeTrue();
            });
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, "TestSchema", "id int"))
                    fbProcessor.ColumnExists("TestSchema", table.Name, "ID").ShouldBeTrue();
            });
        }
    }
}