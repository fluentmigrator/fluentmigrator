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

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Exceptions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Infrastructure;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Tests.Integration.Migrations;
using FluentMigrator.Tests.Unit.TaggingTestFakes;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Shouldly;


namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class DefaultMigrationInformationLoaderTests
    {
        [Test]
        public void CanFindMigrationsInAssembly()
        {
            var loader = ServiceCollectionExtensions.CreateServices()
                .WithMigrationsIn("FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1")
                .BuildServiceProvider()
                .GetRequiredService<IMigrationInformationLoader>();

            var migrationList = loader.LoadMigrations();

            var count = migrationList.Count;

            count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void CanFindMigrationsInNamespace()
        {
            var loader = ServiceCollectionExtensions.CreateServices()
                .WithMigrationsIn("FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1")
                .BuildServiceProvider()
                .GetRequiredService<IMigrationInformationLoader>();

            var migrationList = loader.LoadMigrations();
            migrationList.Select(x => x.Value.Migration.GetType()).ShouldNotContain(typeof(VersionedMigration));
            migrationList.Count().ShouldBeGreaterThan(0);
        }

        [Test]
        public void DefaultBehaviorIsToNotLoadNestedNamespaces()
        {
            var loader = ServiceCollectionExtensions.CreateServices()
                .WithMigrationsIn("FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1")
                .BuildServiceProvider()
                .GetRequiredService<IMigrationInformationLoader>();

            Assert.IsInstanceOf<DefaultMigrationInformationLoader>(loader);

            var defaultLoader = (DefaultMigrationInformationLoader)loader;

            defaultLoader.LoadNestedNamespaces.ShouldBe(false);
        }

        [Test]
        public void FindsMigrationsInNestedNamespaceWhenLoadNestedNamespacesEnabled()
        {
            var loader = ServiceCollectionExtensions.CreateServices()
                .WithMigrationsIn("FluentMigrator.Tests.Integration.Migrations.Nested", true)
                .BuildServiceProvider()
                .GetRequiredService<IMigrationInformationLoader>();

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
            var loader = ServiceCollectionExtensions.CreateServices()
                .WithMigrationsIn("FluentMigrator.Tests.Integration.Migrations.Nested")
                .BuildServiceProvider()
                .GetRequiredService<IMigrationInformationLoader>();

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
            var migrationType = typeof(TaggedMigration);
            var tagsToMatch = new[] { "UK", "Production" };

            var conventionsMock = new Mock<IMigrationRunnerConventions>();
            conventionsMock.SetupGet(m => m.GetMigrationInfoForMigration).Returns(DefaultMigrationRunnerConventions.Instance.GetMigrationInfoForMigration);
            conventionsMock.SetupGet(m => m.TypeIsMigration).Returns(t => true);
            conventionsMock.SetupGet(m => m.TypeHasTags).Returns(t => migrationType == t);
            conventionsMock.SetupGet(m => m.TypeHasMatchingTags).Returns((type, tags) => (migrationType == type && tagsToMatch == tags));

            var loader = ServiceCollectionExtensions.CreateServices()
                .WithMigrationsIn(migrationType.Namespace)
                .Configure<RunnerOptions>(opt => opt.Tags = tagsToMatch)
                .ConfigureRunner(builder => builder.WithRunnerConventions(conventionsMock.Object))
                .BuildServiceProvider()
                .GetRequiredService<IMigrationInformationLoader>();

            var expected = new List<Type> { typeof(UntaggedMigration), migrationType };

            var actual = loader.LoadMigrations().Select(m => m.Value.Migration.GetType()).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void DoesNotFindMigrationsThatDoNotHaveMatchingTags()
        {
            var migrationType = typeof(TaggedMigration);
            var tagsToMatch = new[] { "UK", "Production" };

            var conventionsMock = new Mock<IMigrationRunnerConventions>();
            conventionsMock.SetupGet(m => m.GetMigrationInfoForMigration).Returns(DefaultMigrationRunnerConventions.Instance.GetMigrationInfoForMigration);
            conventionsMock.SetupGet(m => m.TypeIsMigration).Returns(t => true);
            conventionsMock.SetupGet(m => m.TypeHasTags).Returns(t => migrationType == t);
            conventionsMock.SetupGet(m => m.TypeHasMatchingTags).Returns((type, tags) => false);

            var loader = ServiceCollectionExtensions.CreateServices()
                .WithMigrationsIn(migrationType.Namespace)
                .Configure<RunnerOptions>(opt => opt.Tags = tagsToMatch)
                .ConfigureRunner(builder => builder.WithRunnerConventions(conventionsMock.Object))
                .BuildServiceProvider()
                .GetRequiredService<IMigrationInformationLoader>();

            var expected = new List<Type> { typeof(UntaggedMigration) };

            var actual = loader.LoadMigrations().Select(m => m.Value.Migration.GetType()).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void HandlesNotFindingMigrations()
        {
            var loader = ServiceCollectionExtensions.CreateServices()
                .WithMigrationsIn("FluentMigrator.Tests.Unit.EmptyNamespace")
                .BuildServiceProvider()
                .GetRequiredService<IMigrationInformationLoader>();
            Assert.Throws<MissingMigrationsException>(() => loader.LoadMigrations());
        }

        [Test]
        public void ShouldThrowExceptionIfDuplicateVersionNumbersAreLoaded()
        {
            var loader = ServiceCollectionExtensions.CreateServices()
                .WithMigrationsIn("FluentMigrator.Tests.Unit.DuplicateVersionNumbers")
                .BuildServiceProvider()
                .GetRequiredService<IMigrationInformationLoader>();
            Assert.Throws<DuplicateMigrationException>(() => loader.LoadMigrations());
        }

        [Test]
        public void HandlesMigrationThatDoesNotInheritFromMigrationBaseClass()
        {
            var loader = ServiceCollectionExtensions.CreateServices()
                .WithMigrationsIn("FluentMigrator.Tests.Unit.DoesNotInheritFromBaseClass")
                .BuildServiceProvider()
                .GetRequiredService<IMigrationInformationLoader>();
            Assert.That(loader.LoadMigrations().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldHandleTransactionlessMigrations()
        {
            var loader = ServiceCollectionExtensions.CreateServices()
                .WithMigrationsIn("FluentMigrator.Tests.Unit.DoesHandleTransactionLessMigrations")
                .BuildServiceProvider()
                .GetRequiredService<IMigrationInformationLoader>();

            var list = loader.LoadMigrations().ToList();

            list.Count().ShouldBe(2);

            list[0].Value.Migration.GetType().ShouldBe(typeof(DoesHandleTransactionLessMigrations.MigrationThatIsTransactionLess));
            list[0].Value.TransactionBehavior.ShouldBe(TransactionBehavior.None);
            list[0].Value.Version.ShouldBe(1);

            list[1].Value.Migration.GetType().ShouldBe(typeof(DoesHandleTransactionLessMigrations.MigrationThatIsNotTransactionLess));
            list[1].Value.TransactionBehavior.ShouldBe(TransactionBehavior.Default);
            list[1].Value.Version.ShouldBe(2);
        }

        [Test]
        [Obsolete]
        public void ObsoleteCanFindMigrationsInAssembly()
        {
            var conventions = new MigrationRunnerConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1", null);

            SortedList<long, IMigrationInfo> migrationList = loader.LoadMigrations();

            //if this works, there will be at least one migration class because i've included on in this code file
            int count = migrationList.Count();
            count.ShouldBeGreaterThan(0);
        }

        [Test]
        [Obsolete]
        public void ObsoleteCanFindMigrationsInNamespace()
        {
            var conventions = new MigrationRunnerConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1", null);

            var migrationList = loader.LoadMigrations();
            migrationList.Select(x => x.Value.Migration.GetType()).ShouldNotContain(typeof(VersionedMigration));
            migrationList.Count().ShouldBeGreaterThan(0);
        }

        [Test]
        [Obsolete]
        public void ObsoleteDefaultBehaviorIsToNotLoadNestedNamespaces()
        {
            var conventions = new MigrationRunnerConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Integration.Migrations.Nested", null);

            loader.LoadNestedNamespaces.ShouldBe(false);
        }

        [Test]
        [Obsolete]
        public void ObsoleteFindsMigrationsInNestedNamespaceWhenLoadNestedNamespacesEnabled()
        {
            var conventions = new MigrationRunnerConventions();
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
        [Obsolete]
        public void ObsoleteDoesNotFindsMigrationsInNestedNamespaceWhenLoadNestedNamespacesDisabled()
        {
            var conventions = new MigrationRunnerConventions();
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
        [Obsolete]
        public void ObsoleteDoesFindMigrationsThatHaveMatchingTags()
        {
            var asm = Assembly.GetExecutingAssembly();
            var migrationType = typeof(TaggedMigration);
            var tagsToMatch = new[] { "UK", "Production" };

            var conventionsMock = new Mock<IMigrationRunnerConventions>();
            conventionsMock.SetupGet(m => m.GetMigrationInfoForMigration).Returns(DefaultMigrationRunnerConventions.Instance.GetMigrationInfoForMigration);
            conventionsMock.SetupGet(m => m.TypeIsMigration).Returns(t => true);
            conventionsMock.SetupGet(m => m.TypeHasTags).Returns(t => migrationType == t);
            conventionsMock.SetupGet(m => m.TypeHasMatchingTags).Returns((type, tags) => (migrationType == type && tagsToMatch == tags));

            var loader = new DefaultMigrationInformationLoader(conventionsMock.Object, asm, migrationType.Namespace, tagsToMatch);

            var expected = new List<Type> { typeof(UntaggedMigration), migrationType };

            var actual = loader.LoadMigrations().Select(m => m.Value.Migration.GetType()).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        [Obsolete]
        public void ObsoleteDoesNotFindMigrationsThatDoNotHaveMatchingTags()
        {
            var asm = Assembly.GetExecutingAssembly();
            var migrationType = typeof(TaggedMigration);
            var tagsToMatch = new[] { "UK", "Production" };

            var conventionsMock = new Mock<IMigrationRunnerConventions>();
            conventionsMock.SetupGet(m => m.GetMigrationInfoForMigration).Returns(DefaultMigrationRunnerConventions.Instance.GetMigrationInfoForMigration);
            conventionsMock.SetupGet(m => m.TypeIsMigration).Returns(t => true);
            conventionsMock.SetupGet(m => m.TypeHasTags).Returns(t => migrationType == t);
            conventionsMock.SetupGet(m => m.TypeHasMatchingTags).Returns((type, tags) => false);

            var loader = new DefaultMigrationInformationLoader(conventionsMock.Object, asm, migrationType.Namespace, tagsToMatch);

            var expected = new List<Type> { typeof(UntaggedMigration) };

            var actual = loader.LoadMigrations().Select(m => m.Value.Migration.GetType()).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        [Obsolete]
        public void ObsoleteHandlesNotFindingMigrations()
        {
            var conventions = new MigrationRunnerConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Unit.EmptyNamespace", null);
            Assert.Throws<MissingMigrationsException>(() => loader.LoadMigrations());
        }

        [Test]
        [Obsolete]
        public void ObsoleteShouldThrowExceptionIfDuplicateVersionNumbersAreLoaded()
        {
            var conventions = new MigrationRunnerConventions();
            var asm = Assembly.GetExecutingAssembly();
            var migrationLoader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Unit.DuplicateVersionNumbers", null);
            Assert.Throws<DuplicateMigrationException>(() => migrationLoader.LoadMigrations());
        }

        [Test]
        [Obsolete]
        public void ObsoleteHandlesMigrationThatDoesNotInheritFromMigrationBaseClass()
        {
            var conventions = new MigrationRunnerConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new DefaultMigrationInformationLoader(conventions, asm, "FluentMigrator.Tests.Unit.DoesNotInheritFromBaseClass", null);

            Assert.That(loader.LoadMigrations().Count(), Is.EqualTo(1));
        }

        [Test]
        [Obsolete]
        public void ObsoleteShouldHandleTransactionlessMigrations()
        {
            var conventions = new MigrationRunnerConventions();
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

    // ReSharper disable once EmptyNamespace
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

            public string ConnectionString { get; } = null;

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
        public class TaggedMigration : Migration
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
