using System;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using Xunit;

namespace FluentMigrator.Tests.Definitions
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
			var column = new IndexColumnDefinition { ColumnName = null };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.ColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenColumnNameIsEmptyString()
		{
			var column = new IndexColumnDefinition { ColumnName = String.Empty };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.ColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenColumnNameIsNotNullOrEmptyString()
		{
			var column = new IndexColumnDefinition { ColumnName = "Bacon" };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.DoesNotContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty, errors);
		}
	}
}