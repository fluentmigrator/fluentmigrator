using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;
using System.Data;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2012
{
    [TestFixture]
    public class SqlServer2012GeneratorTests
    {
        protected SqlServer2012Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2012Generator();
        }

        [Test]
        public void CanUseSystemMethodCurrentDateTimeOffsetAsADefaultValueForAColumn()
        {
            var expression = new CreateColumnExpression
            {
                Column = new ColumnDefinition
                {
                    Name = "NewColumn",
                    Type = DbType.DateTime,
                    DefaultValue = SystemMethods.CurrentDateTimeOffset
                },
                TableName = "NewTable"
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[NewTable] ADD [NewColumn] DATETIME NOT NULL CONSTRAINT [DF__NewColumn] DEFAULT SYSDATETIMEOFFSET()");
        }
    }
}
