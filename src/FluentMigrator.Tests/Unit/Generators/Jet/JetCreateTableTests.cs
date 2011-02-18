

namespace FluentMigrator.Tests.Unit.Generators
{
    using NUnit.Framework;
    using NUnit.Should;

    
    public class JetGeneratorCreateTableTests : JetGeneratorTestBase
    {
        private string tableName = "NewTable";

        [Test]
        public void CanCreateTable()
        {
            var expression = GetCreateTableExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE [NewTable] ([ColumnName1] VARCHAR(255) NOT NULL, [ColumnName2] INTEGER NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithCustomColumnType()
        {
            var expression = GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [NewTable] ([ColumnName1] VARCHAR(255) NOT NULL PRIMARY KEY, [ColumnName2] [timestamp] NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithPrimaryKey()
        {
            var expression = GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [NewTable] ([ColumnName1] VARCHAR(255) NOT NULL PRIMARY KEY, [ColumnName2] INTEGER NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithIdentity()
        {
            var expression = GetCreateTableExpression();
            expression.Columns[0].IsIdentity = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [NewTable] ([ColumnName1] COUNTER NOT NULL, [ColumnName2] INTEGER NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithNullField()
        {
            var expression = GetCreateTableExpression();
            expression.Columns[0].IsNullable = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [NewTable] ([ColumnName1] VARCHAR(255), [ColumnName2] INTEGER NOT NULL)");
        }

        [Test]
        public void CanCreateTableWithDefaultValue()
        {
            var expression = GetCreateTableExpression();
            expression.Columns[0].DefaultValue = "Default";
            expression.Columns[1].DefaultValue = 0;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [NewTable] ([ColumnName1] VARCHAR(255) NOT NULL DEFAULT 'Default', [ColumnName2] INTEGER NOT NULL DEFAULT 0)");
        }

        [Test]
        public void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            var expression = GetCreateTableExpression();
            expression.Columns[0].DefaultValue = null;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [NewTable] ([ColumnName1] VARCHAR(255) NOT NULL DEFAULT NULL, [ColumnName2] INTEGER NOT NULL)");
        }


        
    }
}
