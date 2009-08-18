using System;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Definitions
{
	public class IndexColumnDefinitionTests
	{
		[Fact]
		public void DirectionIsAscendingIfNotSpecified()
		{
			var column = new IndexColumnDefinition();
			Assert.Equal(Direction.Ascending, column.Direction);
		}

		[Fact]
		public void ErrorIsReturnedWhenColumnNameIsNull()
		{
			var column = new IndexColumnDefinition { Name = null };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.ColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenColumnNameIsEmptyString()
		{
			var column = new IndexColumnDefinition { Name = String.Empty };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.ColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenColumnNameIsNotNullOrEmptyString()
		{
			var column = new IndexColumnDefinition { Name = "Bacon" };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.DoesNotContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty, errors);
		}
	}
}