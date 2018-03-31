#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

using FluentMigrator.Model;
using FluentMigrator.Runner.Infrastructure;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class DefaultMigrationConventionsTests
    {
        private static readonly IMigrationConventions _default = DefaultMigrationConventions.Instance;

        [Test]
        public void GetPrimaryKeyNamePrefixesTableNameWithPKAndUnderscore()
        {
            _default.GetPrimaryKeyName("Foo").ShouldBe("PK_Foo");
        }

        [Test]
        public void GetForeignKeyNameReturnsValidForeignKeyNameForSimpleForeignKey()
        {
            var foreignKey = new ForeignKeyDefinition
            {
                ForeignTable = "Users", ForeignColumns = new[] { "GroupId" },
                PrimaryTable = "Groups", PrimaryColumns = new[] { "Id" }
            };

            _default.GetForeignKeyName(foreignKey).ShouldBe("FK_Users_GroupId_Groups_Id");
        }

        [Test]
        public void GetForeignKeyNameReturnsValidForeignKeyNameForComplexForeignKey()
        {
            var foreignKey = new ForeignKeyDefinition
            {
                ForeignTable = "Users", ForeignColumns = new[] { "ColumnA", "ColumnB" },
                PrimaryTable = "Groups", PrimaryColumns = new[] { "ColumnC", "ColumnD" }
            };

            _default.GetForeignKeyName(foreignKey).ShouldBe("FK_Users_ColumnA_ColumnB_Groups_ColumnC_ColumnD");
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

            _default.GetIndexName(index).ShouldBe("IX_Bacon_BaconName");
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

            _default.GetIndexName(index).ShouldBe("IX_Bacon_BaconName_BaconSpice");
        }

        [Test]
        public void TypeIsMigrationReturnsTrueIfTypeExtendsMigrationAndHasMigrationAttribute()
        {
            _default.TypeIsMigration(typeof(DefaultConventionMigrationFake))
                .ShouldBeTrue();
        }

        [Test]
        public void TypeIsMigrationReturnsFalseIfTypeDoesNotExtendMigration()
        {
            _default.TypeIsMigration(typeof(object))
                .ShouldBeFalse();
        }

        [Test]
        public void TypeIsMigrationReturnsFalseIfTypeDoesNotHaveMigrationAttribute()
        {
            _default.TypeIsMigration(typeof(MigrationWithoutAttributeFake))
                .ShouldBeFalse();
        }

        [Test]
        public void GetMaintenanceStageReturnsCorrectStage()
        {
            _default.GetMaintenanceStage(typeof (MaintenanceAfterEach))
                .ShouldBe(MigrationStage.AfterEach);
        }

        [Test]
        public void MigrationInfoShouldRetainMigration()
        {
            var migrationType = typeof(DefaultConventionMigrationFake);
            var migrationinfo = _default.GetMigrationInfo(migrationType);
            migrationinfo.Migration.GetType().ShouldBeSameAs(migrationType);
        }

        [Test]
        public void MigrationInfoShouldExtractVersion()
        {
            var migrationType = typeof(DefaultConventionMigrationFake);
            var migrationinfo = _default.GetMigrationInfo(migrationType);
            migrationinfo.Version.ShouldBe(123);
        }

        [Test]
        public void MigrationInfoShouldExtractTransactionBehavior()
        {
            var migrationType = typeof(DefaultConventionMigrationFake);
            var migrationinfo = _default.GetMigrationInfo(migrationType);
            migrationinfo.TransactionBehavior.ShouldBe(TransactionBehavior.None);
        }

        [Test]
        public void MigrationInfoShouldExtractTraits()
        {
            var migrationType = typeof(DefaultConventionMigrationFake);
            var migrationinfo = _default.GetMigrationInfo(migrationType);
            migrationinfo.Trait("key").ShouldBe("test");
        }

        [Test]
        [Category("Integration")]
        public void WorkingDirectoryConventionDefaultsToAssemblyFolder()
        {
            var defaultWorkingDirectory = _default.GetWorkingDirectory();

            defaultWorkingDirectory.ShouldNotBeNull();
            defaultWorkingDirectory.Contains("bin").ShouldBeTrue();
        }

        [Test]
        public void DefaultSchemaConventionDefaultsToNull()
        {
            _default.GetDefaultSchema()
                .ShouldBeNull();
        }

        [Test]
        public void TypeHasTagsReturnTrueIfTypeHasTagsAttribute()
        {
            _default.TypeHasTags(typeof(TaggedWithUk))
                .ShouldBeTrue();
        }

        [Test]
        public void TypeHasTagsReturnTrueIfInheritedTypeHasTagsAttribute()
        {
            _default.TypeHasTags(typeof(InheritedFromTaggedWithUk))
                .ShouldBeTrue();
        }

        [Test]
        public void TypeHasTagsReturnFalseIfTypeDoesNotHaveTagsAttribute()
        {
            _default.TypeHasTags(typeof(HasNoTagsFake))
                .ShouldBeFalse();
        }

        [Test]
        public void TypeHasTagsReturnTrueIfBaseTypeDoesHaveTagsAttribute()
        {
            _default.TypeHasTags(typeof(ConcretehasTagAttribute))
                .ShouldBeTrue();
        }

        public class TypeHasMatchingTags
        {
            [Test]
            [Category("Tagging")]
            public void WhenTypeHasTagAttributeButNoTagsPassedInReturnsFalse()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithUk), new string[] { })
                    .ShouldBeFalse();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasTagAttributeWithNoTagNamesReturnsFalse()
            {
                _default.TypeHasMatchingTags(typeof(HasTagAttributeWithNoTagNames), new string[] { })
                    .ShouldBeFalse();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasOneTagThatDoesNotMatchSingleThenTagReturnsFalse()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithUk), new[] { "IE" })
                    .ShouldBeFalse();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasOneTagThatDoesMatchSingleTagThenReturnsTrue()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithUk), new[] { "UK" })
                    .ShouldBeTrue();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasOneTagThatPartiallyMatchesTagThenReturnsFalse()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithUk), new[] { "UK2" })
                    .ShouldBeFalse();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasOneTagThatDoesMatchMultipleTagsThenReturnsFalse()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithUk), new[] { "UK", "Production" })
                    .ShouldBeFalse();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasTagsInTwoAttributeThatDoesMatchSingleTagThenReturnsTrue()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndProductionAndStagingInTwoTagsAttributes), new[] { "UK" })
                    .ShouldBeTrue();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasTagsInTwoAttributesThatDoesMatchMultipleTagsThenReturnsTrue()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndProductionAndStagingInTwoTagsAttributes), new[] { "UK", "Production" })
                    .ShouldBeTrue();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasTagsInOneAttributeThatDoesMatchMultipleTagsThenReturnsTrue()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndProductionAndStagingInOneTagsAttribute), new[] { "UK", "Production" })
                    .ShouldBeTrue();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasTagsInTwoAttributesThatDontNotMatchMultipleTagsThenReturnsFalse()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndProductionAndStagingInTwoTagsAttributes), new[] { "UK", "IE" })
                    .ShouldBeFalse();
            }

            [Test]
            [Category("Tagging")]
            public void WhenBaseTypeHasTagsThenConcreteTypeReturnsTrue()
            {
                _default.TypeHasMatchingTags(typeof(ConcretehasTagAttribute), new[] { "UK" })
                    .ShouldBeTrue();
            }


            //new
            [Test]
            [Category("Tagging")]
            public void WhenTypeHasSingleTagWithSingleTagNameAndBehaviorOfAnyAndHasMatchingTagNamesThenReturnTrue()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithUkAndAnyBehavior), new[] { "UK", "IE" })
                    .ShouldBeTrue();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasSingleTagWithSingleTagNameAndBehaviorOfAnyButNoMatchingTagNamesThenReturnFalse()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithUkAndAnyBehavior), new[] { "Chrome", "IE" })
                    .ShouldBeFalse();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasSingleTagWithMultipleTagNamesAndBehaviorOfAnyWithSomeMatchingTagNamesThenReturnTrue()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndProductionAndStagingAndAnyBehaviorInOneTagsAttribute), new[] { "UK", "Staging", "IE" })
                    .ShouldBeTrue();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasSingleTagWithMultipleTagNamesAndBehaviorOfAnyWithNoMatchingTagNamesThenReturnFalse()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndProductionAndStagingAndAnyBehaviorInOneTagsAttribute), new[] { "IE", "Chrome" })
                    .ShouldBeFalse();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasMultipleTagsWithMultipleTagNamesAndAllTagsHaveBehaviorOfAnyWithAllHavingAMatchingTagNameThenReturnTrue()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndProductionAndStagingInTwoTagsAttributesWithAnyBehaviorOnBoth), new[] { "UK", "Staging" })
                    .ShouldBeTrue();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasMultipleTagsWithMultipleTagNamesAndAllTagsHaveBehaviorOfAnyWithOneTagNotHavingAMatchingTagNameThenReturnTrue()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndProductionAndStagingInTwoTagsAttributesWithAnyBehaviorOnBoth), new[] { "UK", "IE" })
                    .ShouldBeTrue();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasMultipleTagsWithMultipleTagNamesAndOneHasBehaviorOfAnyAndOtherHasBehaviorOfAllWithAllTagNamesMatchingThenReturnTrue()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndAllBehaviorAndProductionAndStagingAndAnyBehaviorInTwoTagsAttributes), new[] { "UK", "Staging" })
                    .ShouldBeTrue();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasMultipleTagsWithMultipleTagNamesAndOneHasBehaviorOfAnyAndOtherHasBehaviorOfAllWithoutAllTagNamesMatchingThenReturnTrue()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndAllBehaviorAndProductionAndStagingAndAnyBehaviorInTwoTagsAttributes), new[] { "UK", "Staging", "IE" })
                    .ShouldBeTrue();
            }

            [Test]
            [Category("Tagging")]
            public void WhenTypeHasMultipleTagsWithMultipleTagNamesAndOneHasBehaviorOfAnyWithoutAnyMatchingTagNamesAndOtherHasBehaviorOfAllWithTagNamesMatchingThenReturnTrue()
            {
                _default.TypeHasMatchingTags(typeof(TaggedWithBeAndUkAndAllBehaviorAndProductionAndStagingAndAnyBehaviorInTwoTagsAttributes), new[] { "BE", "UK" })
                    .ShouldBeTrue();
            }
        }

        [Migration(20130508175300)]
        class AutoScriptMigrationFake : AutoScriptMigration { }

        [Test]
        public void GetAutoScriptUpName()
        {
            var type = typeof(AutoScriptMigrationFake);
            var databaseType = "sqlserver";

            _default.GetAutoScriptUpName(type, databaseType)
                .ShouldBe("Scripts.Up.20130508175300_AutoScriptMigrationFake_sqlserver.sql");
        }

        [Test]
        public void GetAutoScriptDownName()
        {
            var type = typeof(AutoScriptMigrationFake);
            var databaseType = "sqlserver";

            _default.GetAutoScriptDownName(type, databaseType)
                .ShouldBe("Scripts.Down.20130508175300_AutoScriptMigrationFake_sqlserver.sql");
        }
    }


    [Tags("BE", "UK", "Staging", "Production")]
    public class TaggedWithBeAndUkAndProductionAndStagingInOneTagsAttribute
    {
    }

    [Tags(TagBehavior.RequireAny, "BE", "UK", "Staging", "Production")]
    public class TaggedWithBeAndUkAndProductionAndStagingAndAnyBehaviorInOneTagsAttribute
    {
    }

    [Tags("BE", "UK")]
    [Tags("Staging", "Production")]
    public class TaggedWithBeAndUkAndProductionAndStagingInTwoTagsAttributes
    {
    }

    [Tags(TagBehavior.RequireAny, "BE", "UK")]
    [Tags(TagBehavior.RequireAny, "Staging", "Production")]
    public class TaggedWithBeAndUkAndProductionAndStagingInTwoTagsAttributesWithAnyBehaviorOnBoth
    {
    }

    [Tags(TagBehavior.RequireAll,"BE", "UK", "Staging")]
    [Tags(TagBehavior.RequireAny, "Staging", "Production")]
    public class TaggedWithBeAndUkAndAllBehaviorAndProductionAndStagingAndAnyBehaviorInTwoTagsAttributes
    {
    }

    [Tags("UK")]
    public class TaggedWithUk
    {
    }

    public class InheritedFromTaggedWithUk : TaggedWithUk
    {
    }

    [Tags(TagBehavior.RequireAny, "UK")]
    public class TaggedWithUkAndAnyBehavior
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
