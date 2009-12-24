using System;
using System.IO;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
	[TestFixture]
	public class ExecuteSqlScriptExpressionTests
	{
		private string testSqlScript = "testscript.sql";
		private string scriptContents = "TEST SCRIPT";

		[Test]
		public void ErrorIsReturnWhenSqlScriptIsNullOrEmpty()
		{
			var expression = new ExecuteSqlScriptExpression { SqlScript = null };
			var errors = ValidationHelper.CollectErrors(expression);
			errors.ShouldContain(ErrorMessages.SqlScriptCannotBeNullOrEmpty);
		}

		[Test]
		public void ExecutesTheStatement()
		{
			var expression = new ExecuteSqlScriptExpression { SqlScript = testSqlScript };

			var processor = new Mock<IMigrationProcessor>();
			processor.Setup(x => x.Execute(scriptContents)).Verifiable();

			expression.ExecuteWith(processor.Object);
			processor.Verify();
		}

		[Test]
		public void ToStringIsDescriptive()
		{
			var expression = new ExecuteSqlScriptExpression { SqlScript = testSqlScript };
			expression.ToString().ShouldBe("ExecuteSqlScript testscript.sql");
		}
	}
}
