using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer
{
    public class SqlServer2000Data : BaseDataTests
    {
        protected SqlServer2000Generator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2000Generator();


        }

        public override void CanInsertData()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            var sql = generator.Generate(expression);

            var expected = "INSERT INTO [TestTable] ([Id],[Name],[Website]) VALUES (1,'Just''in','codethinked.com');";
            expected += @"INSERT INTO [TestTable] ([Id],[Name],[Website]) VALUES (2,'Na\te','kohari.org');";

            sql.ShouldBe(expected);
        }

        public override void CanDeleteData()
        {
            throw new NotImplementedException();
        }

        public override void CanInsertGuidData()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();

            var sql = generator.Generate(expression);

            var expected = String.Format("INSERT INTO [TestTable] ([guid]) VALUES ('{0}');", GeneratorTestHelper.TestGuid.ToString());

            sql.ShouldBe(expected);
        }

        public override void CanUpdateData()
        {
            throw new NotImplementedException();
        }
    }
}
