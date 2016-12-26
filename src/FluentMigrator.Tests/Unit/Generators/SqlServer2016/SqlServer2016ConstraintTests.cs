using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2016
{
    public class SqlServer2016ConstraintTests
    {
        protected SqlServer2016Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2016Generator();
        }

        [Test]
        public void CanCreatePrimaryKeyWithoutOnlineModeSet()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            expression.Constraint.ApplyOnline = null;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1] PRIMARY KEY ([TestColumn1])");
        }

        [Test]
        public void CanCreatePrimaryKeyWithOnlineOn()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            expression.Constraint.ApplyOnline = Model.OnlineMode.On;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1] PRIMARY KEY ([TestColumn1]) WITH (ONLINE = ON)");
        }

        [Test]
        public void CanCreatePrimaryKeyWithOnlineOff()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            expression.Constraint.ApplyOnline = Model.OnlineMode.Off;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1] PRIMARY KEY ([TestColumn1]) WITH (ONLINE = OFF)");
        }
    }
}
