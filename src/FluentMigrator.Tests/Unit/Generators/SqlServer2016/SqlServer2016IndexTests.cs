using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2016
{
    [TestFixture]
    public class SqlServer2016IndexTests
    {
        protected SqlServer2016Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2016Generator();
        }
        
        [Test]
        public void CanCreateIndexWithOnlineOn()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.ApplyOnline = Model.OnlineMode.On;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC) WITH (ONLINE = ON)");
        }

        [Test]
        public void CanCreateIndexWithOnlineOff()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.ApplyOnline = Model.OnlineMode.Off;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC) WITH (ONLINE = OFF)");
        }

        [Test]
        public void CanCreateIndexWithoutOnlineModeSet()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.ApplyOnline = null;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanDropIndexWithOnlineOn()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            expression.Index.ApplyOnline = Model.OnlineMode.On;

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP INDEX [TestIndex] ON [dbo].[TestTable1] WITH (ONLINE = ON)");
        }

        [Test]
        public void CanDropIndexWithOnlineOff()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            expression.Index.ApplyOnline = Model.OnlineMode.Off;

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP INDEX [TestIndex] ON [dbo].[TestTable1] WITH (ONLINE = OFF)");
        }

        [Test]
        public void CanDropIndexWithoutOnlineModeSet()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            expression.Index.ApplyOnline = null;

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP INDEX [TestIndex] ON [dbo].[TestTable1]");
        }
    }
}