using System;
using System.Data;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Definitions
{
	public class ColumnDefinitionTests
	{
		[Test]
		public void ErrorIsReturnedWhenColumnNameIsNull()
		{
			var column = new ColumnDefinition { Name = null };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenColumnNameIsEmptyString()
		{
			var column = new ColumnDefinition { Name = String.Empty };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsNotReturnedWhenColumnNameIsNotNullOrEmptyString()
		{
			var column = new ColumnDefinition { Name = "Bacon" };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldNotContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenColumnTypeIsNotSet()
		{
			var column = new ColumnDefinition { Type = null };
			var errors = ValidationHelper.CollectErrors(column);
			errors.Contains(ErrorMessages.ColumnTypeMustBeDefined);
		}

		[Test]
		public void ErrorIsNotReturnedWhenColumnTypeIsSet()
		{
			var column = new ColumnDefinition { Type = DbType.String };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldNotContain(ErrorMessages.ColumnTypeMustBeDefined);
		}
	}
}