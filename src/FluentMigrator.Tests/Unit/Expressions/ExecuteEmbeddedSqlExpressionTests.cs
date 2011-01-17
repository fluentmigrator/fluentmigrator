using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Expressions;
using FluentMigrator.Tests.Helpers;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Should;
using System.Reflection;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class ExecuteEmbeddedSqlScriptExpressionTests
    {
       
        private string testSqlScript = "embeddedtestscript.sql";
        private string scriptContents = "TEST SCRIPT";

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
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = testSqlScript, MigrationAssembly = Assembly.GetExecutingAssembly() };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(scriptContents)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ResourceFinderIsCaseInsensitive()
        {
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = testSqlScript.ToUpper(), MigrationAssembly = Assembly.GetExecutingAssembly() };
            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(scriptContents)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            var expression = new ExecuteSqlScriptExpression { SqlScript = testSqlScript };
            expression.ToString().ShouldBe("ExecuteSqlScript embeddedtestscript.sql");
        }
    }
}
