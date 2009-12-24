using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			var expression = new ExecuteSqlScriptExpression { SqlScript = "somefile.sql" };

			var processor = new Mock<IMigrationProcessor>();
			processor.Setup(x => x.Execute(expression.SqlScript)).Verifiable();

			expression.ExecuteWith(processor.Object);
			processor.Verify();
		}

		[Test]
		public void ToStringIsDescriptive()
		{
			var expression = new ExecuteSqlScriptExpression { SqlScript = "somefile.sql" };
			expression.ToString().ShouldBe("ExecuteSqlScript somefile.sql");
		}
	}
}
