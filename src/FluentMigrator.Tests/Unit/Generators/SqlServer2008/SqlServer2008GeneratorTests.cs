using System.Collections.Generic;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.SqlServer;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2008
{
    public class SqlServer2008GeneratorTests
    {
        protected SqlServer2008Generator Generator;

        public SqlServer2008GeneratorTests()
        {
            Generator = new SqlServer2008Generator();
        }

        [Fact]
        public void CanCreateTableWithDateTimeOffsetColumn()
        {
            var expression = new CreateTableExpression {TableName = "TestTable1"};
            expression.Columns.Add(new ColumnDefinition {TableName = "TestTable1", Name = "TestColumn1", Type = DbType.DateTimeOffset});
            expression.Columns.Add(new ColumnDefinition {TableName = "TestTable1", Name = "TestColumn2", Type = DbType.DateTime2});
            expression.Columns.Add(new ColumnDefinition {TableName = "TestTable1", Name = "TestColumn3", Type = DbType.Date});
            expression.Columns.Add(new ColumnDefinition { TableName = "TestTable1", Name = "TestColumn4", Type = DbType.Time });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] DATETIMEOFFSET NOT NULL, [TestColumn2] DATETIME2 NOT NULL, [TestColumn3] DATE NOT NULL, [TestColumn4] TIME NOT NULL)");
        }

        [Fact]
        public void CanInsertScopeIdentity()
        {
            var expression = new InsertDataExpression {TableName = "TestTable"};
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Id", 1),
                                        new KeyValuePair<string, object>("Name", RawSql.Insert("SCOPE_IDENTITY()")),
                                        new KeyValuePair<string, object>("Website", "codethinked.com")
                                    });

            var result = Generator.Generate(expression);
            result.ShouldBe("INSERT INTO [dbo].[TestTable] ([Id], [Name], [Website]) VALUES (1, SCOPE_IDENTITY(), 'codethinked.com')");
        }

        [Fact]
        public void CanInsertAtAtIdentity()
        {
            var expression = new InsertDataExpression {TableName = "TestTable"};
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Id", 1),
                                        new KeyValuePair<string, object>("Name", RawSql.Insert("@@IDENTITY")),
                                        new KeyValuePair<string, object>("Website", "codethinked.com")
                                    });

            var result = Generator.Generate(expression);
            result.ShouldBe("INSERT INTO [dbo].[TestTable] ([Id], [Name], [Website]) VALUES (1, @@IDENTITY, 'codethinked.com')");
        }

        [Fact]
        public void ExplicitUnicodeQuotesCorrectly()
        {
            var expression = new InsertDataExpression {TableName = "TestTable"};
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("UnicodeStringValue", new ExplicitUnicodeString("UnicodeString")),
                                        new KeyValuePair<string, object>("StringValue", "AnsiiString")
                                    });

            var result = Generator.Generate(expression);
            result.ShouldBe("INSERT INTO [dbo].[TestTable] ([UnicodeStringValue], [StringValue]) VALUES (N'UnicodeString', 'AnsiiString')");

        }

    }
}
