using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class MigrationDescriptionAttributeTests
    {
        private System.Globalization.CultureInfo oldCulture;

        [SetUp]
        public void BeforeTest()
        {
            oldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
        }

        [TearDown]
        public void AfterTest()
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = oldCulture;
        }

        [Test]
        public void ThrowArgumentNullExceptionWhenResourceTypeIsNull()
        {
            try
            {
                var da = new MigrationDescriptionAttribute(null);
            }
            catch (Exception ex)
            {
                (ex is ArgumentNullException && ((ArgumentNullException)ex).ParamName == "resourceType").ShouldBeTrue();
            }
        }

        [Test]
        public void ThrowArgumentExceptionWhenNameNotStartWithLetter()
        {
            try
            {
                var da = new MigrationDescriptionAttribute(typeof(FluentMigrator.Tests.Properties.Resources), "1Invalid");
            }
            catch (Exception ex)
            {
                (ex is ArgumentException && ((ArgumentException)ex).ParamName == "name").ShouldBeTrue();
            }
        }

        [Test]
        public void WithoutNameArgument()
        {
            var currentThread = System.Threading.Thread.CurrentThread;
            currentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            var convention = new MigrationConventions();
            var migrationInfo = convention.GetMigrationInfo(new Migrations.WithoutName());
            migrationInfo.GetName()
                .ShouldBe("20131212145340: WithoutName - Test migration description without name argument.");
        }

        [Test]
        public void WithNameArgument()
        {
            var currentThread = System.Threading.Thread.CurrentThread;
            currentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            var convention = new MigrationConventions();
            var migrationInfo = convention.GetMigrationInfo(new Migrations.WithName());
            migrationInfo.GetName()
                .ShouldBe("20131212150730: WithName - Migration description with name.");
        }

        [Test]
        public void Lacalizzation()
        {
            var currentThread = System.Threading.Thread.CurrentThread;
            var convention = new MigrationConventions();
            var migrationInfoWithName = convention.GetMigrationInfo(new Migrations.WithName());
            var migrationInfoWithoutName = convention.GetMigrationInfo(new Migrations.WithoutName());

            currentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            migrationInfoWithName.GetName()
                .ShouldBe("20131212150730: WithName - Migration description with name.");
            migrationInfoWithoutName.GetName()
                .ShouldBe("20131212145340: WithoutName - Test migration description without name argument.");

            currentThread.CurrentUICulture = new System.Globalization.CultureInfo("de-DE");
            migrationInfoWithName.GetName()
                .ShouldBe("20131212150730: WithName - Migration Beschreibung mit dem name-Argument.");
            migrationInfoWithoutName.GetName()
                .ShouldBe("20131212145340: WithoutName - Test-Migration ohne Durch Beschreibung des Themas Namen.");

            currentThread.CurrentUICulture = new System.Globalization.CultureInfo("it-IT");
            migrationInfoWithName.GetName()
                .ShouldBe("20131212150730: WithName - Migration description con l'argomento name.");
            migrationInfoWithoutName.GetName()
                .ShouldBe("20131212145340: WithoutName - Test migration description senza passaggio dell'argomento name .");
        }
    }

    namespace Migrations
    {
        [Migration(20131212145340)]
        [MigrationDescription(typeof(Properties.Resources))]
        class WithoutName : Migration
        {
            public override void Down()
            {

            }

            public override void Up()
            {

            }
        }

        [Migration(20131212150730)]
        [MigrationDescription(typeof(Properties.Resources), "Name")]
        class WithName : Migration
        {
            public override void Down()
            {

            }

            public override void Up()
            {

            }
        }
    }

}
