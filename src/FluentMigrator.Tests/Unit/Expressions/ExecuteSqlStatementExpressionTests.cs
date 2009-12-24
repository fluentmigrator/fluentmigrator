using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class ExecuteSqlStatementExpressionTests
    {
        [Test]
        public void ErrorIsReturnWhenSqlStatementIsNullOrEmpty()
        {
            var expression = new ExecuteSqlStatementExpression() {SqlStatement = null};
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.SqlStatementCannotBeNullOrEmpty);
        }

        [Test]
        public void ExecutesTheStatement()
        {
            var expression = new ExecuteSqlStatementExpression() { SqlStatement = "INSERT INTO BLAH" };
            
            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(expression.SqlStatement)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            var expression = new ExecuteSqlStatementExpression() { SqlStatement = "INSERT INTO BLAH" };
            expression.ToString().ShouldBe("ExecuteSqlStatement INSERT INTO BLAH");
        }
    }
}
