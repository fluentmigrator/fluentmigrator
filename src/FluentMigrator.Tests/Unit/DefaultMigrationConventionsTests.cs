#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System.IO;
using System.Reflection;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
	[TestFixture]
	public class DefaultMigrationConventionsTests
	{
		[Test]
		public void GetPrimaryKeyNamePrefixesTableNameWithPKAndUnderscore()
		{
			DefaultMigrationConventions.GetPrimaryKeyName("Foo").ShouldBe("PK_Foo");
		}

		[Test]
		public void GetForeignKeyNameReturnsValidForeignKeyNameForSimpleForeignKey()
		{
			var foreignKey = new ForeignKeyDefinition
			{
				ForeignTable = "Users", ForeignColumns = new[] { "GroupId" },
				PrimaryTable = "Groups", PrimaryColumns = new[] { "Id" }
			};

			DefaultMigrationConventions.GetForeignKeyName(foreignKey).ShouldBe("FK_Users_GroupId_Groups_Id");
		}

		[Test]
		public void GetForeignKeyNameReturnsValidForeignKeyNameForComplexForeignKey()
		{
			var foreignKey = new ForeignKeyDefinition
			{
				ForeignTable = "Users", ForeignColumns = new[] { "ColumnA", "ColumnB" },
				PrimaryTable = "Groups", PrimaryColumns = new[] { "ColumnC", "ColumnD" }
			};

			DefaultMigrationConventions.GetForeignKeyName(foreignKey).ShouldBe("FK_Users_ColumnA_ColumnB_Groups_ColumnC_ColumnD");
		}

		[Test]
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

			DefaultMigrationConventions.GetIndexName(index).ShouldBe("IX_Bacon_BaconName");
		}

		[Test]
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

			DefaultMigrationConventions.GetIndexName(index).ShouldBe("IX_Bacon_BaconName_BaconSpice");
		}

		[Test]
		public void TypeIsMigrationReturnsTrueIfTypeExtendsMigrationAndHasMigrationAttribute()
		{
			DefaultMigrationConventions.TypeIsMigration(typeof(DefaultConventionMigrationFake))
				.ShouldBeTrue();
		}

		[Test]
		public void TypeIsMigrationReturnsFalseIfTypeDoesNotExtendMigration()
		{
			DefaultMigrationConventions.TypeIsMigration(typeof(object))
				.ShouldBeFalse();
		}

		[Test]
		public void TypeIsMigrationReturnsFalseIfTypeDoesNotHaveMigrationAttribute()
		{
			DefaultMigrationConventions.TypeIsMigration(typeof(MigrationWithoutAttributeFake))
				.ShouldBeFalse();
		}

		[Test]
		public void MigrationMetadataTypePropertyMatchesDecoratedType()
		{
			var metadata = DefaultMigrationConventions.GetMetadataForMigration(typeof(DefaultConventionMigrationFake));
			metadata.Type.ShouldBe(typeof(DefaultConventionMigrationFake));
		}

		[Test]
		public void MigrationMetadataCollectsVersionFromMigrationAttribute()
		{
			var metadata = DefaultMigrationConventions.GetMetadataForMigration(typeof(DefaultConventionMigrationFake));
			metadata.Version.ShouldBe(123);
		}

		[Test]
		public void WorkingDirectoryConventionDefaultsToAssemblyFolder()
		{
			var defaultWorkingDirectory = DefaultMigrationConventions.GetWorkingDirectory();

			defaultWorkingDirectory.ShouldNotBeNull();
			defaultWorkingDirectory.Contains("bin").ShouldBeTrue();
		}
	}

	[Migration(123)]
	internal class DefaultConventionMigrationFake : Migration
	{
		public override void Up() { }
		public override void Down() { }
	}

	internal class MigrationWithoutAttributeFake : Migration
	{
		public override void Up() { }
		public override void Down() { }
	}
}
