using FluentMigrator.Runner.Processors.Firebird;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    [TestFixture]
    [Category("Integration")]
    [Category("Firebird")]
    public class FirebirdConstraintTests : BaseConstraintTests
    {
        [Test]
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int", string.Format("wibble int CONSTRAINT {0} CHECK(wibble > 0)", "\"c'1\"")))
                    fbProcessor.ConstraintExists(null, table.Name, "\"c'1\"").ShouldBeTrue();
            });
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable("\"Test'Table\"", fbProcessor, null, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                    fbProcessor.ConstraintExists(null, table.Name, "C1").ShouldBeTrue();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int"))
                    fbProcessor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, "TestSchema", "id int"))
                    fbProcessor.ConstraintExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                processor.ConstraintExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                    fbProcessor.ConstraintExists(null, table.Name, "C1").ShouldBeTrue();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            ExecuteFor(FIREBIRD, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, "TestSchema", "id int", "wibble int CONSTRAINT C1 CHECK(wibble > 0)"))
                    fbProcessor.ConstraintExists("TestSchema", table.Name, "C1").ShouldBeTrue();
            });
        }
    }
}