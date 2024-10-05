#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using System.Collections.ObjectModel;
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

using NUnit.Framework;

using Moq;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    [Category("Migration")]
    public class AutoReversingMigrationTests
    {
        private Mock<IMigrationContext> _context;

        [SetUp]
        public void SetUp()
        {
            _context = new Mock<IMigrationContext>();
            _context.SetupAllProperties();
        }

        [Test]
        public void CreateTableUpAutoReversingMigrationGivesDeleteTableDown()
        {
            var autoReversibleMigration = new TestAutoReversingMigration();
            _context.Object.Expressions = new Collection<IMigrationExpression>();
            autoReversibleMigration.GetDownExpressions(_context.Object);

            Assert.That(_context.Object.Expressions.Any(me => me is DeleteTableExpression expression && expression.TableName == "Foo"));
        }

        [Test]
        public void DownMigrationsAreInReverseOrderOfUpMigrations()
        {
            var autoReversibleMigration = new TestAutoReversingMigration();
            _context.Object.Expressions = new Collection<IMigrationExpression>();
            autoReversibleMigration.GetDownExpressions(_context.Object);

            Assert.That(_context.Object.Expressions.ToList()[0], Is.AssignableFrom(typeof(RenameTableExpression)));
            Assert.That(_context.Object.Expressions.ToList()[1], Is.AssignableFrom(typeof(DeleteTableExpression)));
        }

    }

    internal class TestAutoReversingMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Foo");
            Rename.Table("Foo").InSchema("FooSchema").To("Bar");
        }
    }
}
