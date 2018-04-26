using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    [TestFixture]
    [Category("Integration")]
    [Category("Firebird")]
    public class FirebirdConstraintTests : BaseConstraintTests
    {
        private readonly FirebirdLibraryProber _prober = new FirebirdLibraryProber();
        private TemporaryDatabase _temporaryDatabase;

        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private FirebirdProcessor Processor { get; set; }

        [Test]
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable(Processor, "id int", string.Format("wibble int CONSTRAINT {0} CHECK(wibble > 0)", "\"c'1\"")))
                Processor.ConstraintExists(null, table.Name, "\"c'1\"").ShouldBeTrue();
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable("\"Test'Table\"", Processor, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists(null, table.Name, "C1").ShouldBeTrue();
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new FirebirdTestTable(Processor, "id int"))
                Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "id int"))
                Processor.ConstraintExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ConstraintExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            using (var table = new FirebirdTestTable(Processor, "id int", "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists(null, table.Name, "C1").ShouldBeTrue();
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "id int", "wibble int CONSTRAINT C1 CHECK(wibble > 0)"))
                Processor.ConstraintExists("TestSchema", table.Name, "C1").ShouldBeTrue();
        }

        [SetUp]
        public void SetUp()
        {
            if (!IntegrationTestOptions.Firebird.IsEnabled)
                Assert.Ignore();

            _temporaryDatabase = new TemporaryDatabase(
                IntegrationTestOptions.Firebird,
                _prober);

            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(builder => builder.AddFirebird())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(_temporaryDatabase.ConnectionString));

            ServiceProvider = serivces.BuildServiceProvider();
            ServiceScope = ServiceProvider.CreateScope();
            Processor = ServiceScope.ServiceProvider.GetRequiredService<FirebirdProcessor>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            ServiceProvider?.Dispose();
            if (_temporaryDatabase != null)
            {
                var connString = _temporaryDatabase.ConnectionString;
                _temporaryDatabase = null;
                FbDatabase.DropDatabase(connString);
            }
        }
    }
}
