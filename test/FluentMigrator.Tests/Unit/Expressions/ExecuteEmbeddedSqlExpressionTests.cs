using NUnit.Framework;
using FluentMigrator.Expressions;
using FluentMigrator.Tests.Helpers;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Should;
using System;
using System.Reflection;

namespace FluentMigrator.Tests.Unit.Expressions
{

    [TestFixture]
    public class ExecuteEmbeddedSqlScriptExpressionTests
    {
        private const string testSqlScript = "embeddedtestscript.sql";
        private const string scriptContents = "TEST SCRIPT";

        [Test]
        public void ErrorIsReturnWhenSqlScriptIsNullOrEmpty()
        {
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.SqlScriptCannotBeNullOrEmpty);
        }

        [Test]
        public void ExecutesTheStatement()
        {
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = testSqlScript, MigrationAssemblies = new SingleAssembly(Assembly.GetExecutingAssembly()) };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(scriptContents)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ResourceFinderIsCaseInsensitive()
        {
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = testSqlScript.ToUpper(), MigrationAssemblies = new SingleAssembly(Assembly.GetExecutingAssembly()) };
            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(scriptContents)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ResourceFinderFindFileWithFullName()
        {
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = "InitialSchema.sql", MigrationAssemblies = new SingleAssembly(Assembly.GetExecutingAssembly()) };
            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute("InitialSchema")).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ResourceFinderFindFileWithFullNameAndNamespace()
        {
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = "FluentMigrator.Tests.EmbeddedResources.InitialSchema.sql", MigrationAssemblies = new SingleAssembly(Assembly.GetExecutingAssembly()) };
            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute("InitialSchema")).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ResourceFinderFindThrowsExceptionIfFoundMoreThenOneResource()
        {
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = "NotUniqueResource.sql", MigrationAssemblies = new SingleAssembly(Assembly.GetExecutingAssembly()) };
            var processor = new Mock<IMigrationProcessor>();

            Assert.Throws<InvalidOperationException>(() => expression.ExecuteWith(processor.Object));
            processor.Verify(x => x.Execute("NotUniqueResource"), Times.Never());
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            var expression = new ExecuteSqlScriptExpression { SqlScript = testSqlScript };
            expression.ToString().ShouldBe("ExecuteSqlScript embeddedtestscript.sql");
        }
    }
}
