using System;
using System.Collections.ObjectModel;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
	[TestFixture]
	public class DeleteForeignKeyExpressionTests
	{
		[Test]
		public void ToStringIsDescriptive()
		{
			new DeleteForeignKeyExpression
			{
				ForeignKey = new ForeignKeyDefinition
				{
					ForeignColumns = new Collection<string> { "User_id" },
					ForeignTable = "UserRoles",
					PrimaryColumns = new Collection<string> { "Id" },
					PrimaryTable = "User",
					Name = "FK"
				}
			}.ToString().ShouldBe("DeleteForeignKey FK UserRoles (User_id) User (Id)");
		}
	}
}