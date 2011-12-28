using System.Collections.ObjectModel;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class CreateIndexExpressionTests
    {
        [Test]
        public void ShouldDelegateApplyConventionsToIndexDefinition()
        {
            var definitionMock = new Mock<IndexDefinition>();
            var createIndexExpression = new CreateIndexExpression {Index = definitionMock.Object};
            var migrationConventions = new Mock<IMigrationConventions>(MockBehavior.Strict).Object;

            definitionMock.Setup(id => id.ApplyConventions(migrationConventions)).Verifiable();

            createIndexExpression.ApplyConventions(migrationConventions);

            definitionMock.VerifyAll();
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            new CreateIndexExpression
                {
                    Index = new IndexDefinition
                                {
                                    Columns = new Collection<IndexColumnDefinition>
                                                  {
                                                      new IndexColumnDefinition {Name = "Name"},
                                                      new IndexColumnDefinition {Name = "Slug"}
                                                  },
                                    TableName = "Table",
                                    Name = "NameIndex"
                                }
                }.ToString().ShouldBe("CreateIndex Table (Name, Slug)");
        }
    }
}