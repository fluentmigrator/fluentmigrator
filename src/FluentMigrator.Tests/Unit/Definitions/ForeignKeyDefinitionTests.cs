using System;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Definitions
{
	public class ForeignKeyDefinitionTests
	{
		[Fact]
		public void ErrorIsReturnedWhenNameIsNull()
		{
			var column = new ForeignKeyDefinition { Name = null };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.ForeignKeyNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenNameIsNotNullOrEmptyString()
		{
			var column = new ForeignKeyDefinition { Name = "Bacon" };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.DoesNotContain(ErrorMessages.ForeignKeyNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenForeignTableNameIsNull()
		{
			var column = new ForeignKeyDefinition { ForeignTable = null };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenTableNameIsNotNullOrEmptyString()
		{
			var column = new ForeignKeyDefinition { ForeignTable = "Bacon" };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.DoesNotContain(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenForeignTableNameIsEmptyString()
		{
			var column = new ForeignKeyDefinition { ForeignTable = String.Empty };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenPrimaryTableNameIsNull()
		{
			var column = new ForeignKeyDefinition { PrimaryTable = null };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenPrimaryTableNameIsEmptyString()
		{
			var column = new ForeignKeyDefinition { PrimaryTable = String.Empty };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenPrimaryTableNameIsNotNullOrEmptyString()
		{
			var column = new ForeignKeyDefinition { PrimaryTable = "Bacon" };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.DoesNotContain(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenPrimaryTableNameIsSameAsForeignTableName()
		{
			var column = new ForeignKeyDefinition { PrimaryTable = "Bacon", ForeignTable = "Bacon" };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.ForeignKeyCannotBeSelfReferential, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenPrimaryTableNameIsDifferentThanForeignTableName()
		{
			var column = new ForeignKeyDefinition { PrimaryTable = "Bacon", ForeignTable = "NotBacon" };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.DoesNotContain(ErrorMessages.ForeignKeyCannotBeSelfReferential, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenForeignColumnsIsEmpty()
		{
			var column = new ForeignKeyDefinition { ForeignColumns = new string[0] };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.ForeignKeyMustHaveOneOrMoreForeignColumns, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenForeignColumnsIsNotEmpty()
		{
			var column = new ForeignKeyDefinition { ForeignColumns = new[] { "Bacon" } };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.DoesNotContain(ErrorMessages.ForeignKeyMustHaveOneOrMoreForeignColumns, errors);
		}

		[Fact]
		public void ErrorIsReturnedWhenPrimaryColumnsIsEmpty()
		{
			var column = new ForeignKeyDefinition { PrimaryColumns = new string[0] };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.Contains(ErrorMessages.ForeignKeyMustHaveOneOrMorePrimaryColumns, errors);
		}

		[Fact]
		public void ErrorIsNotReturnedWhenPrimaryColumnsIsNotEmpty()
		{
			var column = new ForeignKeyDefinition { PrimaryColumns = new[] { "Bacon" } };
			var errors = ValidationHelper.CollectErrors(column);
			Assert.DoesNotContain(ErrorMessages.ForeignKeyMustHaveOneOrMorePrimaryColumns, errors);
		}
	}
}