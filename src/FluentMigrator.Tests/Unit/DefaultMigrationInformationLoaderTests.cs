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
using FluentMigrator.Exceptions;
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
    public class DefaultMigrationInformationLoaderTests
    {
        [Test]
        public void CanFindMigrationsInAssembly()
        {
            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new DefaultMigrationInformationLoader( conventions, asm, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1", null);
            
            SortedList<long, IMigrationInfo> migrationList = loader.LoadMigrations();

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
            var loader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1", null);

            var migrationList = loader.LoadMigrations();
            migrationList.Select(x => x.Value.Migration.GetType()).ShouldNotContain(typeof(VersionedMigration));
            migrationList.Count().ShouldBeGreaterThan(0);
        }

        [Test]
        public void DefaultBehaviorIsToNotLoadNestedNamespaces()
        {
            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Integration.Migrations.Nested", null);

            loader.LoadNestedNamespaces.ShouldBe(false);
        }

        [Test]
        public void FindsMigrationsInNestedNamespaceWhenLoadNestedNamespacesEnabled()
        {
            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Integration.Migrations.Nested", true, null);

            List<Type> expected = new List<Type>
                                      {
                typeof(Integration.Migrations.Nested.NotGrouped),
                typeof(Integration.Migrations.Nested.Group1.FromGroup1),
                typeof(Integration.Migrations.Nested.Group1.AnotherFromGroup1),
                typeof(Integration.Migrations.Nested.Group2.FromGroup2),
            };

            var migrationList = loader.LoadMigrations();
            List<Type> actual = migrationList.Select(m => m.Value.Migration.GetType()).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void DoesNotFindsMigrationsInNestedNamespaceWhenLoadNestedNamespacesDisabled()
        {
            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Integration.Migrations.Nested", false, null);

            List<Type> expected = new List<Type>
                                      {
                typeof(Integration.Migrations.Nested.NotGrouped),
            };

            var migrationList = loader.LoadMigrations();
            List<Type> actual = migrationList.Select(m => m.Value.Migration.GetType()).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void DoesFindMigrationsThatHaveMatchingTags()
        {
            var asm = Assembly.GetExecutingAssembly();
            var migrationType = typeof(TaggedMigraion);
            var tagsToMatch = new[] { "UK", "Production" };

            var conventionsMock = new Mock<IMigrationConventions>();
            conventionsMock.SetupGet(m => m.GetMigrationInfo).Returns(DefaultMigrationConventions.GetMigrationInfoFor);
            conventionsMock.SetupGet(m => m.TypeIsMigration).Returns(t => true);
            conventionsMock.SetupGet(m => m.TypeHasTags).Returns(t => migrationType == t);
            conventionsMock.SetupGet(m => m.TypeHasMatchingTags).Returns((type, tags) => (migrationType == type && tagsToMatch == tags));

            var loader = new DefaultMigrationInformationLoader(conventionsMock.Object, asm, migrationType.Namespace, tagsToMatch);

            var expected = new List<Type> { typeof(UntaggedMigration), migrationType };

            var actual = loader.LoadMigrations().Select(m => m.Value.Migration.GetType()).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void DoesNotFindMigrationsThatDoNotHaveMatchingTags()
        {
            var asm = Assembly.GetExecutingAssembly();
            var migrationType = typeof(TaggedMigraion);
            var tagsToMatch = new[] { "UK", "Production" };

            var conventionsMock = new Mock<IMigrationConventions>();
            conventionsMock.SetupGet(m => m.GetMigrationInfo).Returns(DefaultMigrationConventions.GetMigrationInfoFor);
            conventionsMock.SetupGet(m => m.TypeIsMigration).Returns(t => true);
            conventionsMock.SetupGet(m => m.TypeHasTags).Returns(t => migrationType == t);
            conventionsMock.SetupGet(m => m.TypeHasMatchingTags).Returns((type, tags) => false);

            var loader = new DefaultMigrationInformationLoader(conventionsMock.Object, asm, migrationType.Namespace, tagsToMatch);

            var expected = new List<Type> { typeof(UntaggedMigration) };

            var actual = loader.LoadMigrations().Select(m => m.Value.Migration.GetType()).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        public void HandlesNotFindingMigrations()
        {
            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Unit.EmptyNamespace", null);

            var list = loader.LoadMigrations();

            Assert.That(list, Is.Not.Null);
            Assert.That(list.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ShouldThrowExceptionIfDuplicateVersionNumbersAreLoaded()
        {
            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var migrationLoader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Unit.DuplicateVersionNumbers", null);
            Assert.Throws<DuplicateMigrationException>(() => migrationLoader.LoadMigrations());
        }

        [Test]
        public void HandlesMigrationThatDoesNotInheritFromMigrationBaseClass()
        {
            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Unit.DoesNotInheritFromBaseClass", null);

            Assert.That(loader.LoadMigrations().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldHandleTransactionlessMigrations()
        {
            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Unit.DoesHandleTransactionLessMigrations", null);

            var list = loader.LoadMigrations().ToList();

            list.Count().ShouldBe(2);
            
            list[0].Value.Migration.GetType().ShouldBe(typeof(DoesHandleTransactionLessMigrations.MigrationThatIsTransactionLess));
            list[0].Value.TransactionBehavior.ShouldBe(TransactionBehavior.None);
            list[0].Value.Version.ShouldBe(1);

            list[1].Value.Migration.GetType().ShouldBe(typeof(DoesHandleTransactionLessMigrations.MigrationThatIsNotTransactionLess));
            list[1].Value.TransactionBehavior.ShouldBe(TransactionBehavior.Default);
            list[1].Value.Version.ShouldBe(2);
        }
    }

    namespace EmptyNamespace
    {

    }

    namespace DoesHandleTransactionLessMigrations
    {
        [Migration(1, TransactionBehavior.None)]
        public class MigrationThatIsTransactionLess : Migration
        {
            public override void Up()
            {
                throw new NotImplementedException();
            }

            public override void Down()
            {
                throw new NotImplementedException();
            }
        }

        [Migration(2)]
        public class MigrationThatIsNotTransactionLess : Migration
        {
            public override void Up()
            {
                throw new NotImplementedException();
            }

            public override void Down()
            {
                throw new NotImplementedException();
            }
        }
    }

    namespace DoesNotInheritFromBaseClass
    {
        [Migration(1)]
        public class MigrationThatDoesNotInheritFromMigrationBaseClass : IMigration
        {
            /// <summary>The arbitrary application context passed to the task runner.</summary>
            public object ApplicationContext
            {
                get { throw new NotImplementedException(); }
            }

            public string ConnectionString { get; private set; }

            public void GetUpExpressions(IMigrationContext context)
            {
                throw new NotImplementedException();
            }

            public void GetDownExpressions(IMigrationContext context)
            {
                throw new NotImplementedException();
            }
        }
    }

    namespace DuplicateVersionNumbers
    {
        [Migration(1)]
        public class Duplicate1 : Migration
        {
            public override void Up() { }

            public override void Down() { }
        }

        [Migration(1)]
        public class Duplicate2 : Migration
        {
            public override void Up() { }

            public override void Down() { }
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

