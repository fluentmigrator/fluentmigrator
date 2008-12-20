using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using Xunit;

namespace FluentMigrator.Tests.Unit
{
	public class DefaultMigrationConventionsTests
	{
		[Fact]
		public void GetPrimaryKeyNamePrefixesTableNameWithPKAndUnderscore()
		{
			var expression = new CreateTableExpression { TableName = "Foo" };
			Assert.Equal("PK_Foo", DefaultMigrationConventions.GetPrimaryKeyName(expression));
		}

		[Fact]
		public void GetForeignKeyNameReturnsValidForeignKeyNameForSimpleForeignKey()
		{
			var foreignKey = new ForeignKeyDefinition
			{
				ForeignTable = "Users", ForeignColumns = new[] { "GroupId" },
				PrimaryTable = "Groups", PrimaryColumns = new[] { "Id" }
			};

			Assert.Equal("FK_Users_GroupId_Groups_Id", DefaultMigrationConventions.GetForeignKeyName(foreignKey));
		}

		[Fact]
		public void GetForeignKeyNameReturnsValidForeignKeyNameForComplexForeignKey()
		{
			var foreignKey = new ForeignKeyDefinition
			{
				ForeignTable = "Users", ForeignColumns = new[] { "ColumnA", "ColumnB" },
				PrimaryTable = "Groups", PrimaryColumns = new[] { "ColumnC", "ColumnD" }
			};

			Assert.Equal("FK_Users_ColumnA_ColumnB_Groups_ColumnC_ColumnD", DefaultMigrationConventions.GetForeignKeyName(foreignKey));
		}

		[Fact]
		public void GetIndexNameReturnsValidIndexNameForSimpleIndex()
		{
			var index = new IndexDefinition
			{
				TableName = "Bacon",
				Columns =
				{
					new IndexColumnDefinition { Name = "BaconName", Direction = Direction.Ascending }
				}
			};

			Assert.Equal("IX_Bacon_BaconName", DefaultMigrationConventions.GetIndexName(index));
		}

		[Fact]
		public void GetIndexNameReturnsValidIndexNameForComplexIndex()
		{
			var index = new IndexDefinition
			{
				TableName = "Bacon",
				Columns =
				{
					new IndexColumnDefinition { Name = "BaconName", Direction = Direction.Ascending },
					new IndexColumnDefinition { Name = "BaconSpice", Direction = Direction.Descending }
				}
			};

			Assert.Equal("IX_Bacon_BaconName_BaconSpice", DefaultMigrationConventions.GetIndexName(index));
		}

		[Fact]
		public void TypeIsMigrationReturnsTrueIfTypeExtendsMigrationAndHasMigrationAttribute()
		{
			Assert.True(DefaultMigrationConventions.TypeIsMigration(typeof(DefaultConventionMigration)));
		}

		[Fact]
		public void TypeIsMigrationReturnsFalseIfTypeDoesNotExtendMigration()
		{
			Assert.False(DefaultMigrationConventions.TypeIsMigration(typeof(object)));
		}

		[Fact]
		public void TypeIsMigrationReturnsFalseIfTypeDoesNotHaveMigrationAttribute()
		{
			Assert.False(DefaultMigrationConventions.TypeIsMigration(typeof(MigrationWithoutAttribute)));
		}

		[Fact]
		public void MigrationMetadataTypePropertyMatchesDecoratedType()
		{
			var metadata = DefaultMigrationConventions.GetMetadataForMigration(typeof(DefaultConventionMigration));
			Assert.Equal(typeof(DefaultConventionMigration), metadata.Type);
		}

		[Fact]
		public void MigrationMetadataCollectsVersionFromMigrationAttribute()
		{
			var metadata = DefaultMigrationConventions.GetMetadataForMigration(typeof(DefaultConventionMigration));
			Assert.Equal(123, metadata.Version);
		}
	}

	[Migration(123)]
	internal class DefaultConventionMigration : Migration
	{
		public override void Up() { throw new NotImplementedException(); }
		public override void Down() { throw new NotImplementedException(); }
	}

	internal class MigrationWithoutAttribute : Migration
	{
		public override void Up() { throw new NotImplementedException(); }
		public override void Down() { throw new NotImplementedException(); }
	}
}
