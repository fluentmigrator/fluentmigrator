using System.Collections.ObjectModel;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
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

        [Test]
        public void ErrorIsReturnedWhenNameIsNull()
        {
            var expression = new CreateIndexExpression {Index = {Name = null, TableName = "test"}};
            expression.Index.Columns.Add(new IndexColumnDefinition());

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.IndexNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenTableNameIsNull()
        {
            var expression = new CreateIndexExpression { Index = { Name = "IX", TableName = null } };
            expression.Index.Columns.Add(new IndexColumnDefinition());

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenColumnCountIsZero()
        {
            var expression = new CreateIndexExpression { Index = { Name = "IX", TableName = "test" } };

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.IndexMustHaveOneOrMoreColumns);
        }

        [Test]
        public void ErrorIsNotReturnedWhenValidExpression()
        {
            var expression = new CreateIndexExpression { Index = { Name = "IX", TableName = "test" } };
            expression.Index.Columns.Add(new IndexColumnDefinition{ Name = "Column1"});

            var errors = ValidationHelper.CollectErrors(expression);

            Assert.That(errors.Count, Is.EqualTo(0));
        }
    }
}