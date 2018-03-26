using FluentMigrator.Runner.Processors.Firebird;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    [TestFixture]
    [Category("Integration")]
    [Category("Firebird")]
    public class FirebirdTableTests : BaseTableTests
    {
        [Test]
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                using (var table = new FirebirdTestTable("\"Test'Table\"", processor as FirebirdProcessor, null, "id int"))
                    processor.TableExists(null, table.Name).ShouldBeTrue();
            });
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                processor.TableExists("TestSchema", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                using (var table = new FirebirdTestTable(processor as FirebirdProcessor, null, "id int"))
                    processor.TableExists(null, table.Name).ShouldBeTrue();
            });
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                using (var table = new FirebirdTestTable(processor as FirebirdProcessor, "TestSchema", "id int"))
                    processor.TableExists("TestSchema", table.Name).ShouldBeTrue();
            });
        }


    }
}