using System.Collections.ObjectModel;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class DeleteIndexExpressionTests
    {
        [Test]
        public void ShouldDelegateApplyConventionsToIndexDefinition()
        {
            var definitionMock = new Mock<IndexDefinition>();
            var createIndexExpression = new DeleteIndexExpression {Index = definitionMock.Object};
            var migrationConventions = new Mock<IMigrationConventions>(MockBehavior.Strict).Object;

            definitionMock.Setup(id => id.ApplyConventions(migrationConventions)).Verifiable();

            createIndexExpression.ApplyConventions(migrationConventions);

            definitionMock.VerifyAll();
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            new DeleteIndexExpression
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
                }.ToString().ShouldBe("DeleteIndex Table (Name, Slug)");
        }
    }
}