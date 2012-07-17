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

using System;
using System.Reflection;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Tests.Integration.Migrations;
using FluentMigrator.Tests.Unit.TaggingTestFakes;
using Moq;
using NUnit.Framework;
using NUnit.Should;
using System.Linq;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class MigrationLoaderTests
    {
        [Test]
        public void CanFindMigrationsInAssembly()
        {
            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new MigrationLoader( conventions, asm, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1", null);
            
            SortedList<long, IMigration> migrationList = loader.Migrations;

            //if this works, there will be at least one migration class because i've included on in this code file
            var en = migrationList.GetEnumerator();
            int count = 0;
            while (en.MoveNext())
                count++;

            count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void CanFindMigrationsInNamespace()
        {
            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new MigrationLoader(conventions, asm, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1", null);

            var migrationList = loader.FindMigrations();
            migrationList.Select(x => x.Type).ShouldNotContain(typeof(VersionedMigration));
            migrationList.Count().ShouldBeGreaterThan(0);
        }

        [Test]
        public void DefaultBehaviorIsToNotLoadNestedNamespaces()
        {
            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new MigrationLoader(conventions, asm, "FluentMigrator.Tests.Integration.Migrations.Nested", null);

            loader.LoadNestedNamespaces.ShouldBe(false);
        }

        [Test]
        public void FindsMigrationsInNestedNamepsaceWhenLoadNestedNamespacesEnabled()
        {
            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new MigrationLoader(conventions, asm, "FluentMigrator.Tests.Integration.Migrations.Nested", true);

            List<Type> expected = new List<Type>
                                      {
                typeof(Integration.Migrations.Nested.NotGrouped),
                typeof(Integration.Migrations.Nested.Group1.FromGroup1),
                typeof(Integration.Migrations.Nested.Group1.AnotherFromGroup1),
                typeof(Integration.Migrations.Nested.Group2.FromGroup2),
            };

            var migrationList = loader.FindMigrations();
            List<Type> actual = migrationList.Select(m => m.Type).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void DoesNotFindsMigrationsInNestedNamepsaceWhenLoadNestedNamespacesDisabled()
        {
            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new MigrationLoader(conventions, asm, "FluentMigrator.Tests.Integration.Migrations.Nested", false);

            List<Type> expected = new List<Type>
                                      {
                typeof(Integration.Migrations.Nested.NotGrouped),
            };

            var migrationList = loader.FindMigrations();
            List<Type> actual = migrationList.Select(m => m.Type).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void DoesFindMigrationsThatHaveMatchingTags()
        {
            var asm = Assembly.GetExecutingAssembly();
            var migrationType = typeof(TaggedMigraion);
            var tagsToMatch = new[] { "UK", "Production" };

            var conventionsMock = new Mock<IMigrationConventions>();
            conventionsMock.SetupGet(m => m.GetMetadataForMigration).Returns(DefaultMigrationConventions.GetMetadataForMigration);
            conventionsMock.SetupGet(m => m.TypeIsMigration).Returns(t => true);
            conventionsMock.SetupGet(m => m.TypeHasTags).Returns(t => migrationType == t);
            conventionsMock.SetupGet(m => m.TypeHasMatchingTags).Returns((type, tags) => (migrationType == type && tagsToMatch == tags));

            var loader = new MigrationLoader(conventionsMock.Object, asm, migrationType.Namespace, tagsToMatch);

            var expected = new List<Type> { typeof(UntaggedMigration), migrationType };

            var actual = loader.FindMigrations().Select(m => m.Type).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void DoesNotFindMigrationsThatDoNotHaveMatchingTags()
        {
            var asm = Assembly.GetExecutingAssembly();
            var migrationType = typeof(TaggedMigraion);
            var tagsToMatch = new[] { "UK", "Production" };

            var conventionsMock = new Mock<IMigrationConventions>();
            conventionsMock.SetupGet(m => m.GetMetadataForMigration).Returns(DefaultMigrationConventions.GetMetadataForMigration);
            conventionsMock.SetupGet(m => m.TypeIsMigration).Returns(t => true);
            conventionsMock.SetupGet(m => m.TypeHasTags).Returns(t => migrationType == t);
            conventionsMock.SetupGet(m => m.TypeHasMatchingTags).Returns((type, tags) => false);

            var loader = new MigrationLoader(conventionsMock.Object, asm, migrationType.Namespace, tagsToMatch);

            var expected = new List<Type> { typeof(UntaggedMigration) };

            var actual = loader.FindMigrations().Select(m => m.Type).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }
    }
    
    namespace TaggingTestFakes
    {
        [Tags("UK", "IE", "QA", "Production")]
        [Migration(123)]
        public class TaggedMigraion : Migration
        {
            public override void Up() { }

            public override void Down() { }
        }    

        [Migration(567)]
        public class UntaggedMigration : Migration
        {
            public override void Up() { }

            public override void Down() { }
        }
    }
}

