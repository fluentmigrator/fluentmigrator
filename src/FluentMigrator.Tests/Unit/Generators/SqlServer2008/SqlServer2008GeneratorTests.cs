using System.Collections.Generic;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators
{
    public class SqlServer2008GeneratorTests
    {
        private SqlServer2008Generator generator;

        [SetUp]
        public void SetUp()
        {
            generator = new SqlServer2008Generator();
        }

        [Test]
        public void CanCreateTableWithDateTimeOffsetColumn()
        {
            var expression = new CreateTableExpression {TableName = "TestTable1"};
            expression.Columns.Add(new ColumnDefinition {TableName = "TestTable1", Name = "TestColumn1", Type = DbType.DateTimeOffset});
            expression.Columns.Add(new ColumnDefinition {TableName = "TestTable1", Name = "TestColumn2", Type = DbType.DateTime2});
            expression.Columns.Add(new ColumnDefinition {TableName = "TestTable1", Name = "TestColumn3", Type = DbType.Date});
            expression.Columns.Add(new ColumnDefinition {TableName = "TestTable1", Name = "TestColumn4", Type = DbType.Time});

            var sql = generator.Generate(expression);

            sql.ShouldBe(
                "CREATE TABLE [dbo].[TestTable1] ([TestColumn1] DATETIMEOFFSET NOT NULL, [TestColumn2] DATETIME2 NOT NULL, [TestColumn3] DATE NOT NULL, [TestColumn4] TIME NOT NULL)");
        }

        [Test]
        public void CanInsertScopeIdentity()
        {
            var expression = new InsertDataExpression();
            expression.TableName = "TestTable";
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Id", 1),
                                        new KeyValuePair<string, object>("Name", RawSql.Insert("SCOPE_IDENTITY()")),
                                        new KeyValuePair<string, object>("Website", "codethinked.com")
                                    });

            var sql = generator.Generate(expression);

            var expected = "INSERT INTO [dbo].[TestTable] ([Id], [Name], [Website]) VALUES (1, SCOPE_IDENTITY(), 'codethinked.com')";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanInsertAtAtIdentity()
        {
            var expression = new InsertDataExpression();
            expression.TableName = "TestTable";
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Id", 1),
                                        new KeyValuePair<string, object>("Name", RawSql.Insert("@@IDENTITY")),
                                        new KeyValuePair<string, object>("Website", "codethinked.com")
                                    });

            var sql = generator.Generate(expression);

            var expected = "INSERT INTO [dbo].[TestTable] ([Id], [Name], [Website]) VALUES (1, @@IDENTITY, 'codethinked.com')";

            sql.ShouldBe(expected);
        }

    }
}