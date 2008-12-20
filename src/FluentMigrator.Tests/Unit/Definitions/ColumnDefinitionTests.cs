using System;
using System.Data;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Definitions
{
	public class ColumnDefinitionTests
	{
		[Fact]
		public void ErrorIsReturnedWhenColumnNameIsNull()
		{
			var column = new ColumnDefinition { Name = null };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.ColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenColumnNameIsEmptyString()
		{
			var column = new ColumnDefinition { Name = String.Empty };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.ColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenColumnNameIsNotNullOrEmptyString()
		{
			var column = new ColumnDefinition { Name = "Bacon" };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.DoesNotContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenColumnTypeIsNotSet()
		{
			var column = new ColumnDefinition { Type = null };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.ColumnTypeMustBeDefined, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenColumnTypeIsSet()
		{
			var column = new ColumnDefinition { Type = DbType.String };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.DoesNotContain(ErrorMessages.ColumnTypeMustBeDefined, errors);
		}
	}
}