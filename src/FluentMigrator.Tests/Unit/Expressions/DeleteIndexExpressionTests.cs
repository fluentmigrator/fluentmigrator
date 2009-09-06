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
	public class DeleteIndexExpressionTests
	{
		[Test]
		public void ToStringIsDescriptive()
		{
			new DeleteIndexExpression
			{
				Index = new IndexDefinition
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
			}.ToString().ShouldBe("DeleteIndex Table (Name, Slug)");
		}
	}
}