using System.Collections.ObjectModel;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
	[TestFixture]
	public class CreateForeignKeyExpressionTests
	{
		[Test]
		public void ToStringIsDescriptive()
		{
			new CreateForeignKeyExpression
				{
					ForeignKey = new ForeignKeyDefinition
					{
						ForeignColumns = new Collection<string> { "User_id" },
						ForeignTable = "UserRoles",
						PrimaryColumns = new Collection<string> { "Id" },
						PrimaryTable = "User",
						Name = "FK"
					}
			}.ToString().ShouldBe("CreateForeignKey FK UserRoles (User_id) User (Id)");
			
		}
	}
}
