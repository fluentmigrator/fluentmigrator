using System;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Definitions
{
	public class ForeignKeyDefinitionTests
	{
		[Test]
		public void ErrorIsReturnedWhenNameIsNull()
		{
			var column = new ForeignKeyDefinition { Name = null };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldContain(ErrorMessages.ForeignKeyNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsNotReturnedWhenNameIsNotNullOrEmptyString()
		{
			var column = new ForeignKeyDefinition { Name = "Bacon" };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldNotContain(ErrorMessages.ForeignKeyNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenForeignTableNameIsNull()
		{
			var column = new ForeignKeyDefinition { ForeignTable = null };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldContain(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsNotReturnedWhenTableNameIsNotNullOrEmptyString()
		{
			var column = new ForeignKeyDefinition { ForeignTable = "Bacon" };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldNotContain(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenForeignTableNameIsEmptyString()
		{
			var column = new ForeignKeyDefinition { ForeignTable = String.Empty };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldContain(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenPrimaryTableNameIsNull()
		{
			var column = new ForeignKeyDefinition { PrimaryTable = null };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldContain(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenPrimaryTableNameIsEmptyString()
		{
			var column = new ForeignKeyDefinition { PrimaryTable = String.Empty };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldContain(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsNotReturnedWhenPrimaryTableNameIsNotNullOrEmptyString()
		{
			var column = new ForeignKeyDefinition { PrimaryTable = "Bacon" };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldNotContain(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty);
		}

		[Test]
		public void ErrorIsReturnedWhenPrimaryTableNameIsSameAsForeignTableName()
		{
			var column = new ForeignKeyDefinition { PrimaryTable = "Bacon", ForeignTable = "Bacon" };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldContain(ErrorMessages.ForeignKeyCannotBeSelfReferential);
		}

		[Test]
		public void ErrorIsNotReturnedWhenPrimaryTableNameIsDifferentThanForeignTableName()
		{
			var column = new ForeignKeyDefinition { PrimaryTable = "Bacon", ForeignTable = "NotBacon" };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldNotContain(ErrorMessages.ForeignKeyCannotBeSelfReferential);
		}

		[Test]
		public void ErrorIsReturnedWhenForeignColumnsIsEmpty()
		{
			var column = new ForeignKeyDefinition { ForeignColumns = new string[0] };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldContain(ErrorMessages.ForeignKeyMustHaveOneOrMoreForeignColumns);
		}

		[Test]
		public void ErrorIsNotReturnedWhenForeignColumnsIsNotEmpty()
		{
			var column = new ForeignKeyDefinition { ForeignColumns = new[] { "Bacon" } };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldNotContain(ErrorMessages.ForeignKeyMustHaveOneOrMoreForeignColumns);
		}

		[Test]
		public void ErrorIsReturnedWhenPrimaryColumnsIsEmpty()
		{
			var column = new ForeignKeyDefinition { PrimaryColumns = new string[0] };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldContain(ErrorMessages.ForeignKeyMustHaveOneOrMorePrimaryColumns);
		}

		[Test]
		public void ErrorIsNotReturnedWhenPrimaryColumnsIsNotEmpty()
		{
			var column = new ForeignKeyDefinition { PrimaryColumns = new[] { "Bacon" } };
			var errors = ValidationHelper.CollectErrors(column);
			errors.ShouldNotContain(ErrorMessages.ForeignKeyMustHaveOneOrMorePrimaryColumns);
		}
	}
}