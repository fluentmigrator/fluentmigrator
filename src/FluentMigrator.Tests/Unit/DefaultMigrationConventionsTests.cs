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
using Xunit;

namespace FluentMigrator.Tests.Unit
{
    public class DefaultMigrationConventionsTests
    {
        [Fact]
        public void GetPrimaryKeyNamePrefixesTableNameWithPKAndUnderscore()
        {
            DefaultMigrationConventions.GetPrimaryKeyName("Foo").ShouldBe("PK_Foo");
        }

        [Fact]
        public void GetForeignKeyNameReturnsValidForeignKeyNameForSimpleForeignKey()
        {
            var foreignKey = new ForeignKeyDefinition
            {
                ForeignTable = "Users", ForeignColumns = new[] { "GroupId" },
                PrimaryTable = "Groups", PrimaryColumns = new[] { "Id" }
            };

            DefaultMigrationConventions.GetForeignKeyName(foreignKey).ShouldBe("FK_Users_GroupId_Groups_Id");
        }

        [Fact]
        public void GetForeignKeyNameReturnsValidForeignKeyNameForComplexForeignKey()
        {
            var foreignKey = new ForeignKeyDefinition
            {
                ForeignTable = "Users", ForeignColumns = new[] { "ColumnA", "ColumnB" },
                PrimaryTable = "Groups", PrimaryColumns = new[] { "ColumnC", "ColumnD" }
            };

            DefaultMigrationConventions.GetForeignKeyName(foreignKey).ShouldBe("FK_Users_ColumnA_ColumnB_Groups_ColumnC_ColumnD");
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

            DefaultMigrationConventions.GetIndexName(index).ShouldBe("IX_Bacon_BaconName");
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

            DefaultMigrationConventions.GetIndexName(index).ShouldBe("IX_Bacon_BaconName_BaconSpice");
        }

        [Fact]
        public void TypeIsMigrationReturnsTrueIfTypeExtendsMigrationAndHasMigrationAttribute()
        {
            DefaultMigrationConventions.TypeIsMigration(typeof(DefaultConventionMigrationFake))
                .ShouldBeTrue();
        }

        [Fact]
        public void TypeIsMigrationReturnsFalseIfTypeDoesNotExtendMigration()
        {
            DefaultMigrationConventions.TypeIsMigration(typeof(object))
                .ShouldBeFalse();
        }

        [Fact]
        public void TypeIsMigrationReturnsFalseIfTypeDoesNotHaveMigrationAttribute()
        {
            DefaultMigrationConventions.TypeIsMigration(typeof(MigrationWithoutAttributeFake))
                .ShouldBeFalse();
        }

        [Fact]
        public void GetMaintenanceStageReturnsCorrectStage()
        {
            DefaultMigrationConventions.GetMaintenanceStage(typeof (MaintenanceAfterEach))
                .ShouldBe(MigrationStage.AfterEach);
        }

        [Fact]
        public void MigrationInfoShouldRetainMigration()
        {
            var migrationType = typeof(DefaultConventionMigrationFake);
            var migrationinfo = DefaultMigrationConventions.GetMigrationInfoFor(migrationType);
            migrationinfo.Migration.GetType().ShouldBeSameAs(migrationType);
        }

        [Fact]
        public void MigrationInfoShouldExtractVersion()
        {
            var migrationType = typeof(DefaultConventionMigrationFake);
            var migrationinfo = DefaultMigrationConventions.GetMigrationInfoFor(migrationType);
            migrationinfo.Version.ShouldBe(123);
        }

        [Fact]
        public void MigrationInfoShouldExtractTransactionBehavior()
        {
            var migrationType = typeof(DefaultConventionMigrationFake);
            var migrationinfo = DefaultMigrationConventions.GetMigrationInfoFor(migrationType);
            migrationinfo.TransactionBehavior.ShouldBe(TransactionBehavior.None);
        }

        [Fact]
        public void MigrationInfoShouldExtractTraits()
        {
            var migrationType = typeof(DefaultConventionMigrationFake);
            var migrationinfo = DefaultMigrationConventions.GetMigrationInfoFor(migrationType);
            migrationinfo.Trait("key").ShouldBe("test");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void WorkingDirectoryConventionDefaultsToAssemblyFolder()
        {
            var defaultWorkingDirectory = DefaultMigrationConventions.GetWorkingDirectory();

            defaultWorkingDirectory.ShouldNotBeNull();
            defaultWorkingDirectory.Contains("bin").ShouldBeTrue();
        }

        [Fact]
        public void TypeHasTagsReturnTrueIfTypeHasTagsAttribute()
        {
            DefaultMigrationConventions.TypeHasTags(typeof(TaggedWithUk))
                .ShouldBeTrue();
        }

        [Fact]
        public void TypeHasTagsReturnFalseIfTypeDoesNotHaveTagsAttribute()
        {
            DefaultMigrationConventions.TypeHasTags(typeof(HasNoTagsFake))
                .ShouldBeFalse();
        }

        [Fact]
        public void TypeHasTagsReturnTrueIfBaseTypeDoesHaveTagsAttribute()
        {
            DefaultMigrationConventions.TypeHasTags(typeof(ConcretehasTagAttribute))
                .ShouldBeTrue();
        }

        public class TypeHasMatchingTags
        {
            [Fact]
            [Trait("Category", "Tagging")]
            public void WhenTypeHasTagAttributeButNoTagsPassedInReturnsFalse()
            {
                DefaultMigrationConventions.TypeHasMatchingTags(typeof(TaggedWithUk), new string[] { })
                    .ShouldBeFalse();
            }

            [Fact]
            [Trait("Category", "Tagging")]
            public void WhenTypeHasTagAttributeWithNoTagNamesReturnsFalse()
            {
                DefaultMigrationConventions.TypeHasMatchingTags(typeof(HasTagAttributeWithNoTagNames), new string[] { })
                    .ShouldBeFalse();
            }

            [Fact]
            [Trait("Category", "Tagging")]
            public void WhenTypeHasOneTagThatDoesNotMatchSingleThenTagReturnsFalse()
            {
                DefaultMigrationConventions.TypeHasMatchingTags(typeof(TaggedWithUk), new[] { "IE" })
                    .ShouldBeFalse();
            }

            [Fact]
            [Trait("Category", "Tagging")]
            public void WhenTypeHasOneTagThatDoesMatchSingleTagThenReturnsTrue()
            {
                DefaultMigrationConventions.TypeHasMatchingTags(typeof(TaggedWithUk), new[] { "UK" })
                    .ShouldBeTrue();
            }

            [Fact]
            [Trait("Category", "Tagging")]
            public void WhenTypeHasOneTagThatPartiallyMatchesTagThenReturnsFalse()
            {
                DefaultMigrationConventions.TypeHasMatchingTags(typeof(TaggedWithUk), new[] { "UK2" })
                    .ShouldBeFalse();
            }

            [Fact]
            [Trait("Category", "Tagging")]
            public void WhenTypeHasOneTagThatDoesMatchMultipleTagsThenReturnsFalse()
            {
                DefaultMigrationConventions.TypeHasMatchingTags(typeof(TaggedWithUk), new[] { "UK", "Production" })
                    .ShouldBeFalse();
            }

            [Fact]
            [Trait("Category", "Tagging")]
            public void WhenTypeHasTagsInTwoAttributeThatDoesMatchSingleTagThenReturnsTrue()
            {
                DefaultMigrationConventions.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndProductionAndStagingInTwoTagsAttributes), new[] { "UK" })
                    .ShouldBeTrue();
            }

            [Fact]
            [Trait("Category", "Tagging")]
            public void WhenTypeHasTagsInTwoAttributesThatDoesMatchMultipleTagsThenReturnsTrue()
            {
                DefaultMigrationConventions.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndProductionAndStagingInTwoTagsAttributes), new[] { "UK", "Production" })
                    .ShouldBeTrue();
            }

            [Fact]
            [Trait("Category", "Tagging")]
            public void WhenTypeHasTagsInOneAttributeThatDoesMatchMultipleTagsThenReturnsTrue()
            {
                DefaultMigrationConventions.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndProductionAndStagingInOneTagsAttribute), new[] { "UK", "Production" })
                    .ShouldBeTrue();
            }

            [Fact]
            [Trait("Category", "Tagging")]
            public void WhenTypeHasTagsInTwoAttributesThatDontNotMatchMultipleTagsThenReturnsFalse()
            {
                DefaultMigrationConventions.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndProductionAndStagingInTwoTagsAttributes), new[] { "UK", "IE" })
                    .ShouldBeFalse();
            }

            [Fact]
            [Trait("Category", "Tagging")]
            public void WhenBaseTypeHasTagsThenConcreteTypeReturnsTrue()
            {
                DefaultMigrationConventions.TypeHasMatchingTags(typeof(ConcretehasTagAttribute), new[] { "UK" })
                    .ShouldBeTrue();
            }
        }

        [FluentMigrator.Migration(20130508175300)]
        class AutoScriptMigrationFake : AutoScriptMigration { }

        [Fact]
        public void GetAutoScriptUpName()
        {
            var type = typeof(AutoScriptMigrationFake);
            var databaseType = "sqlserver";

            DefaultMigrationConventions.GetAutoScriptUpName(type, databaseType)
                .ShouldBe("Scripts.Up.20130508175300_AutoScriptMigrationFake_sqlserver.sql");
        }

        [Fact]
        public void GetAutoScriptDownName()
        {
            var type = typeof(AutoScriptMigrationFake);
            var databaseType = "sqlserver";

            DefaultMigrationConventions.GetAutoScriptDownName(type, databaseType)
                .ShouldBe("Scripts.Down.20130508175300_AutoScriptMigrationFake_sqlserver.sql");
        }
    }


    [Tags("BE", "UK", "Staging", "Production")]
    public class TaggedWithBeAndUkAndProductionAndStagingInOneTagsAttribute
    {
    }

    [Tags("BE", "UK")]
    [Tags("Staging", "Production")]
    public class TaggedWithBeAndUkAndProductionAndStagingInTwoTagsAttributes
    {
    }

    [Tags("UK")]
    public class TaggedWithUk
    {
    }

    [Tags]
    public class HasTagAttributeWithNoTagNames
    {
    }

    public class HasNoTagsFake
    {
    }

    [Tags("UK")]
    public abstract class BaseHasTagAttribute : Migration
    { }

    public class ConcretehasTagAttribute : BaseHasTagAttribute
    {
        public override void Up(){}

        public override void Down(){}
    }



    [Migration(123, TransactionBehavior.None)]
    [MigrationTrait("key", "test")]
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

    [Maintenance(MigrationStage.AfterEach)]
    internal class MaintenanceAfterEach : Migration
    {
        public override void Up() { }
        public override void Down() { }
    }

}
