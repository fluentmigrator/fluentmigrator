using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Xunit;

namespace FluentMigrator.Tests
{
	public class DefaultMigrationConventionsTests
	{
		[Fact]
		public void GetPrimaryKeyNamePrefixesTableNameWithPKAndUnderscore()
		{
			var expression = new CreateTableExpression { TableName = "Foo" };
			Assert.Equal("PK_Foo", DefaultMigrationConventions.GetPrimaryKeyName(expression));
		}
	}
}
