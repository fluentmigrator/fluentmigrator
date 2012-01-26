#region License

// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class MigrationWithMetaDataAdapterTests
    {
        private MigrationWithMetaDataAdapter adapter;
        private IMigration migration;
        private IMigrationMetadata migrationMetadata;

        [SetUp]
        public void Setup()
        {
            migration = new Mock<IMigration>(MockBehavior.Strict).Object;
            migrationMetadata = new Mock<IMigrationMetadata>(MockBehavior.Strict).Object;
            adapter = new MigrationWithMetaDataAdapter(migration, migrationMetadata);
        }

        [Test]
        public void Should_retain_migration()
        {
            Assert.AreSame(migration, adapter.Migration);
        }

        [Test]
        public void Should_retain_migration_metadata()
        {
            Assert.AreSame(migrationMetadata, adapter.MetaData);
        }

        [Test]
        public void Should_throw_argumentnullexception_if_migrationMetaData_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new MigrationWithMetaDataAdapter(migration, null));
        }

        [Test]
        public void Should_throw_argumentnullexception_if_migration_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new MigrationWithMetaDataAdapter(null, migrationMetadata));
        }

        [Test]
        public void Should_delegate_version_to_metadata()
        {
            Mock.Get(migrationMetadata).SetupGet(m => m.Version).Returns(1);
            Assert.AreEqual(1, adapter.Version);
        }

        [Test]
        public void Should_delegate_type_to_metadata()
        {
            Mock.Get(migrationMetadata).SetupGet(m => m.Type).Returns(typeof (string));
            Assert.AreEqual(typeof (string), adapter.Type);
        }

        [Test]
        public void Should_delegate_hastrait_to_metadata()
        {
            Mock.Get(migrationMetadata).Setup(m => m.HasTrait("Boeh")).Returns(false);
            Assert.AreEqual(false, adapter.HasTrait("Boeh"));
        }

        [Test]
        public void Should_delegate_trait_to_metadata()
        {
            Mock.Get(migrationMetadata).Setup(m => m.Trait("Boeh")).Returns(false);
            Assert.AreEqual(false, adapter.Trait("Boeh"));
        }

        [Test]
        public void Should_delegate_applicationcontext_to_migration()
        {
            Mock.Get(migration).SetupGet(m => m.ApplicationContext).Returns(false);
            Assert.AreEqual(false, adapter.ApplicationContext);
        }

        [Test]
        public void Should_delegate_getupexpressions_to_migration()
        {
            var migrationContext = new Mock<IMigrationContext>();

            Mock.Get(migration).Setup(m => m.GetUpExpressions(migrationContext.Object));
            adapter.GetUpExpressions(migrationContext.Object);
        }

        [Test]
        public void Should_delegate_getdownexpressions_to_migration()
        {
            var migrationContext = new Mock<IMigrationContext>();

            Mock.Get(migration).Setup(m => m.GetDownExpressions(migrationContext.Object));
            adapter.GetDownExpressions(migrationContext.Object);
        }
    }
}