using System;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Generic;
using Moq;
using NUnit.Framework;
using FluentMigrator.Model;
namespace FluentMigrator.Tests.Unit.Generators
{
	[TestFixture]
	public class GenericGeneratorTests
	{
		/// <summary>
		/// Mock class implementing GenericGenerator, for purposes of testing
		/// </summary>
		public class MockGenerator : GenericGenerator
		{

			 public MockGenerator(IColumn column, IQuoter quoter)
				: base(column, quoter) { }

			public override string Generate(AlterDefaultConstraintExpression expression)
			{
				throw new NotImplementedException();
			}

			public override string Generate(DeleteDefaultConstraintExpression expression)
			{
				throw new NotImplementedException();
			}
		}


		[Test]
		public void Delete_Foreign_Key_Should_Throw_Exception_If_Table_Name_Is_Null()
		{
			// Setup empty FK
			var deleteFKExpression = new DeleteForeignKeyExpression();
			var fkDef = new ForeignKeyDefinition();
			deleteFKExpression.ForeignKey = fkDef;

			// Setup empty mock object
			var mockGenerator = new MockGenerator(null, null);

			Assert.Throws<ArgumentNullException>(() => mockGenerator.Generate(deleteFKExpression));
		}
	}
}