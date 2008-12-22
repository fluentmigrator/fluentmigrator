using System;
using FluentMigrator.Infrastructure;
using Xunit;

namespace FluentMigrator.Tests.Unit
{
	public class MigrationMetadataTests
	{
		private MigrationMetadata metadata = new MigrationMetadata();

		[Fact]
		public void HasTraitReturnsTrueWhenTraitIsDefined()
		{
			metadata.AddTrait("foo", 42);
			Assert.True(metadata.HasTrait("foo"));
		}

		[Fact]
		public void HasTraitReturnsFalseWhenTraitIsNotDefined()
		{
			Assert.False(metadata.HasTrait("foo"));
		}

		[Fact]
		public void TraitMethodReturnsTraitValue()
		{
			const string value = "bar";
			metadata.AddTrait("foo", value);
			Assert.Same(value, metadata.Trait("foo"));
		}

		[Fact]
		public void TraitMethodReturnsNullForNonExistentTrait()
		{
			Assert.Null(metadata.Trait("foo"));
		}
	}
}