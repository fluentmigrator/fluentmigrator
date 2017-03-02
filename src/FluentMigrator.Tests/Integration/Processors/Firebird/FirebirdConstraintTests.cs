using System;
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
        private static readonly Type FB_PROCESSOR = typeof(FirebirdProcessor);

        [Test]
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            ExecuteFor(FB_PROCESSOR, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int", string.Format("wibble int CONSTRAINT {0} CHECK(wibble > 0)", "\"c'1\"")))
                    fbProcessor.ConstraintExists(null, table.Name, "\"c'1\"").ShouldBeTrue();
            });
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            ExecuteFor(FB_PROCESSOR, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable("\"Test'Table\"", fbProcessor, null, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                    fbProcessor.ConstraintExists(null, table.Name, "C1").ShouldBeTrue();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            ExecuteFor(FB_PROCESSOR, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int"))
                    fbProcessor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            ExecuteFor(FB_PROCESSOR, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, "TestSchema", "id int"))
                    fbProcessor.ConstraintExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            ExecuteFor(FB_PROCESSOR, processor =>
            {
                processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            ExecuteFor(FB_PROCESSOR, processor =>
            {
                processor.ConstraintExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            ExecuteFor(FB_PROCESSOR, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, null, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                    fbProcessor.ConstraintExists(null, table.Name, "C1").ShouldBeTrue();
            });
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            ExecuteFor(FB_PROCESSOR, processor =>
            {
                var fbProcessor = processor as FirebirdProcessor;
                using (var table = new FirebirdTestTable(fbProcessor, "TestSchema", "id int", "wibble int CONSTRAINT C1 CHECK(wibble > 0)"))
                    fbProcessor.ConstraintExists("TestSchema", table.Name, "C1").ShouldBeTrue();
            });
        }
    }
}