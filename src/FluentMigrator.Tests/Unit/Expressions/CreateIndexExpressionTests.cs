using System.Collections.ObjectModel;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
	[TestFixture]
	public class CreateIndexExpressionTests
	{
		[Test]
		public void ToStringIsDescriptive()
		{
			new CreateIndexExpression{ Index = new IndexDefinition
			                                   	{
			                                   		Columns = new Collection<IndexColumnDefinition>
			                                   		          	{
			                                   		          		new IndexColumnDefinition
			                                   		          			{
																			Name = "Name"
			                                   		          			},
																	new IndexColumnDefinition
																		{
																			Name = "Slug"
																		}
			                                   		          	},
													TableName = "Table",
													Name = "NameIndex"
												}
			}.ToString().ShouldBe("CreateIndex Table (Name, Slug)");
			
		}
	}
}
