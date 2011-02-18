

namespace FluentMigrator.Tests.Unit.Generators

{
    using FluentMigrator.Runner.Generators;
    using NUnit.Framework;
    using NUnit.Should;
    using FluentMigrator.Runner.Generators.Generic;

    [TestFixture]
    public class Quote_UnquoteTests 
    {
        GenericGenerator SUT = default(GenericGenerator);

        [SetUp]
        public void Setup(){
            SUT = new GenericGeneratorImplementor();
        }

        [Test]
        public void CanQuoteAString()
        {
            SUT.Quote("TestString").ShouldBe("\"TestString\"");
        }

        [Test]
        public void CanEscapeAString()
        {
            SUT.Quote("Test\"String").ShouldBe("\"Test\"\"String\"");
        }

        [Test]
        public void CanRecogniseAQuotedString()
        {
            SUT.IsQuoted("\"QuotedString\"").ShouldBeTrue();
        }

        [Test]
        public void CanRecogniseAnUnQuotedString()
        {
            SUT.IsQuoted("UnQuotedString").ShouldBeFalse();
        }

        [Test]
        public void CanHandleAnUnQuotedColumnName()
        {
            SUT.QuoteForColumnName("ColumnName").ShouldBe("\"ColumnName\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedColumnName()
        {
            SUT.QuoteForColumnName("\"ColumnName\"").ShouldBe("\"ColumnName\"");
        }

        [Test]
        public void CanHandleAnUnQuotedTableName()
        {
            SUT.QuoteForColumnName("TableName").ShouldBe("\"TableName\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedTableName()
        {
            SUT.QuoteForColumnName("\"TableName\"").ShouldBe("\"TableName\"");
        }

        [Test]
        public void CanHandleAnUnQuotedSchemaName()
        {
            SUT.QuoteForColumnName("SchemaName").ShouldBe("\"SchemaName\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedSchemaName()
        {
            SUT.QuoteForColumnName("\"SchemaName\"").ShouldBe("\"SchemaName\"");
        }
    }
}
