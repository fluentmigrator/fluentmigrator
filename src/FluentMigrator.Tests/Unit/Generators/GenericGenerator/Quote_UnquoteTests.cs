

namespace FluentMigrator.Tests.Unit.Generators

{
    using FluentMigrator.Runner.Generators;
    using NUnit.Framework;
    using NUnit.Should;
    using FluentMigrator.Runner.Generators.Generic;

    [TestFixture]
    public class Quote_UnquoteTests 
    {
        IQuoter SUT = default(GenericQuoter);

        [SetUp]
        public void Setup(){
            SUT = new GenericQuoter();
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
            SUT.QuoteColumnName("ColumnName").ShouldBe("\"ColumnName\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedColumnName()
        {
            SUT.QuoteColumnName("\"ColumnName\"").ShouldBe("\"ColumnName\"");
        }

        [Test]
        public void CanHandleAnUnQuotedTableName()
        {
            SUT.QuoteColumnName("TableName").ShouldBe("\"TableName\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedTableName()
        {
            SUT.QuoteColumnName("\"TableName\"").ShouldBe("\"TableName\"");
        }

        [Test]
        public void CanHandleAnUnQuotedSchemaName()
        {
            SUT.QuoteColumnName("SchemaName").ShouldBe("\"SchemaName\"");
        }

        [Test]
        public void CanHandleAnAlreadyQuotedSchemaName()
        {
            SUT.QuoteColumnName("\"SchemaName\"").ShouldBe("\"SchemaName\"");
        }
    }
}
