using NUnit.Framework;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Tests.Unit
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using FluentMigrator.Expressions;
    using Moq;

    [TestFixture]
    public class AutoReversingMigrationTests
    {
        private Mock<IMigrationContext> context;

        [SetUp]
        public void SetUp()
        {
            context = new Mock<IMigrationContext>();
            context.SetupAllProperties();
        }

        [Test]
        public void CreateTableUpAutoReversingMigrationGivesDeleteTableDown()
        {
            var autoReversibleMigration = new TestAutoReversingMigration();
            context.Object.Expressions = new Collection<IMigrationExpression>();
            autoReversibleMigration.GetDownExpressions(context.Object);

            Assert.True(context.Object.Expressions.Any(me => me is DeleteTableExpression && ((DeleteTableExpression)me).TableName == "Foo"));
        }

        [Test]
        public void DownMigrationsAreInReverseOrderOfUpMigrations()
        {
            var autoReversibleMigration = new TestAutoReversingMigration();
            context.Object.Expressions = new Collection<IMigrationExpression>();
            autoReversibleMigration.GetDownExpressions(context.Object);

            Assert.IsAssignableFrom(typeof(RenameTableExpression), context.Object.Expressions.ToList()[0]);
            Assert.IsAssignableFrom(typeof(DeleteTableExpression), context.Object.Expressions.ToList()[1]);
        }

    }

    internal class TestAutoReversingMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Foo");
            Rename.Table("Foo").InSchema("FooSchema").To("Bar");
        }
    }
}