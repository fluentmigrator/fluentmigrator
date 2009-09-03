using FluentMigrator.Infrastructure;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
	[TestFixture]
	public class MigrationMetadataTests
	{
		private MigrationMetadata metadata;

		[SetUp]
		public void SetUp()
		{
			metadata = new MigrationMetadata();
		}

		[Test]
		public void HasTraitReturnsTrueWhenTraitIsDefined()
		{
			metadata.AddTrait("foo", 42);
			metadata.HasTrait("foo").ShouldBeTrue();
		}

		[Test]
		public void HasTraitReturnsFalseWhenTraitIsNotDefined()
		{
			metadata.HasTrait("foo").ShouldBeFalse();
		}

		[Test]
		public void TraitMethodReturnsTraitValue()
		{
			const string value = "bar";
			metadata.AddTrait("foo", value);
			value.ShouldBeSameAs(metadata.Trait("foo"));
		}

		[Test]
		public void TraitMethodReturnsNullForNonExistentTrait()
		{
			metadata.Trait("foo").ShouldBeNull();
		}
	}
}