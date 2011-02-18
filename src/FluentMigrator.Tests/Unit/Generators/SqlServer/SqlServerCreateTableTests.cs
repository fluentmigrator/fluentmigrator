
namespace FluentMigrator.Tests.Unit.Generators
{
    using NUnit.Framework;
    using NUnit.Should;


    [TestFixture]
    public class SqlServer2000GeneratorCreateTableTests 
    {
        

       

        

        //[Test]
        //public void CanCreateTableWithNullField()
        //{
        //    var expression = GetCreateTableExpression();
        //    expression.Columns[0].IsNullable = true;
        //    var sql = generator.Generate(expression);
        //    sql.ShouldBe(
        //        "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255), ColumnName2 INT NOT NULL)");
        //}

        //[Test]
        //public void CanCreateTableWithDefaultValue()
        //{
        //    var expression = GetCreateTableExpression();
        //    expression.Columns[0].DefaultValue = "Default";
        //    expression.Columns[1].DefaultValue = 0;
        //    var sql = generator.Generate(expression);
        //    sql.ShouldBe(
        //        "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL CONSTRAINT DF_NewTable_ColumnName1 DEFAULT 'Default', ColumnName2 INT NOT NULL CONSTRAINT DF_NewTable_ColumnName2 DEFAULT 0)");
        //}

        //[Test]
        //public void CanCreateTableWithDefaultValueExplicitlySetToNull()
        //{
        //    var expression = GetCreateTableExpression();
        //    expression.Columns[0].DefaultValue = null;
        //    var sql = generator.Generate(expression);
        //    sql.ShouldBe(
        //        "CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(255) NOT NULL CONSTRAINT DF_NewTable_ColumnName1 DEFAULT NULL, ColumnName2 INT NOT NULL)");

        //}
    }
}
