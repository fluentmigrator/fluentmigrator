using System;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Definitions
{
	[TestFixture]
	public class IndexColumnDefinitionTests
	{
		[Test]
		public void DirectionIsAscendingIfNotSpecified()
		{
			var column = new IndexColumnDefinition();
			column.Direction.ShouldBe(Direction.Ascending);
		}

		[Test]
		public void ErrorIsReturnedWhenColumnNameIsNull()
		{
			var column = new IndexColumnDefinition { Name = null };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenColumnNameIsEmptyString()
		{
			var column = new IndexColumnDefinition { Name = String.Empty };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsNotReturnedWhenColumnNameIsNotNullOrEmptyString()
		{
			var column = new IndexColumnDefinition { Name = "Bacon" };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldNotContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
		}
	}
}