#region License

// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

using Shouldly;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class MigrationInfoTests
    {
        [SetUp]
        public void Setup()
        {
            _expectedVersion = new Random().Next();
            _migration = Mock.Of<IMigration>();
        }

        private IMigration _migration;
        private long _expectedVersion;

        private MigrationInfo Create(TransactionBehavior behavior = TransactionBehavior.Default)
        {
            return new MigrationInfo(_expectedVersion, behavior, _migration);
        }

        [Test]
        public void ConstructingShouldRetainMigration()
        {
            MigrationInfo migrationinfo = Create();
            migrationinfo.Migration.ShouldBeSameAs(_migration);
        }

        [Test]
        public void ConstructingShouldRetainTransactionBehaviorDefault()
        {
            MigrationInfo migrationinfo = Create();
            migrationinfo.TransactionBehavior.ShouldBe(TransactionBehavior.Default);
        }

        [Test]
        public void ConstructingShouldRetainTransactionBehaviorNone()
        {
            MigrationInfo migrationinfo = Create(TransactionBehavior.None);
            migrationinfo.TransactionBehavior.ShouldBe(TransactionBehavior.None);
        }

        [Test]
        public void ConstructingShouldRetainValueOfVersion()
        {
            MigrationInfo migrationinfo = Create();
            migrationinfo.Version.ShouldBe(_expectedVersion);
        }

        [Test]
        public void HasTraitReturnsFalseWhenTraitIsNotDefined()
        {
            MigrationInfo migrationinfo = Create();
            migrationinfo.HasTrait("foo").ShouldBeFalse();
        }

        [Test]
        public void HasTraitReturnsTrueWhenTraitIsDefined()
        {
            MigrationInfo migrationinfo = Create();
            migrationinfo.AddTrait("foo", 42);
            migrationinfo.HasTrait("foo").ShouldBeTrue();
        }

        [Test]
        public void TraitMethodReturnsNullForNonExistentTrait()
        {
            MigrationInfo migrationinfo = Create();
            migrationinfo.Trait("foo").ShouldBeNull();
        }

        [Test]
        public void TraitMethodReturnsTraitValue()
        {
            MigrationInfo migrationinfo = Create();
            const string value = "bar";
            migrationinfo.AddTrait("foo", value);
            migrationinfo.Trait("foo").ShouldBeSameAs(value);
        }
    }
}
