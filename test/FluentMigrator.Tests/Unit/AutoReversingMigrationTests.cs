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
using Shouldly;

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

        [Test]
        public void DownMigrationsWithInsertDataGeneratesTheDeleteInInverseOrderOfUpMigrations()
        {
            var autoReversibleMigration = new TestInsertDataAutoReversingMigration();
            _context.Object.Expressions = new Collection<IMigrationExpression>();
            autoReversibleMigration.GetDownExpressions(_context.Object);

            Assert.That(_context.Object.Expressions.ToList()[0], Is.AssignableFrom(typeof(DeleteDataExpression)));

            var deleteExpression = (_context.Object.Expressions.ToList()[0] as DeleteDataExpression);

            deleteExpression.Rows.Count.ShouldBe(2);

            deleteExpression.Rows[0][0].Key.ShouldBe("Id");
            deleteExpression.Rows[0][0].Value.ShouldBe(2);

            deleteExpression.Rows[0][1].Key.ShouldBe("ParentId");
            deleteExpression.Rows[0][1].Value.ShouldBe(1);

            deleteExpression.Rows[1][0].Key.ShouldBe("Id");
            deleteExpression.Rows[1][0].Value.ShouldBe(1);
            deleteExpression.Rows[1].Count.ShouldBe(1, "Id 1 doesn't have a ParentId reference");
        }

        [Test]
        public void AlterTableAddColumnAutoReversingMigrationGivesDeleteColumnDown()
        {
            var autoReversibleMigration = new TestAlterTableAddColumnAutoReversingMigration();
            _context.Object.Expressions = new Collection<IMigrationExpression>();
            autoReversibleMigration.GetDownExpressions(_context.Object);

            var expressions = _context.Object.Expressions.ToList();

            var deleteColumnExpressions = expressions.OfType<DeleteColumnExpression>().ToList();
            deleteColumnExpressions.Count.ShouldBe(3);

            deleteColumnExpressions.ShouldContain(e => e.TableName == "Library" && e.ColumnNames.Contains("address_ar"));
            deleteColumnExpressions.ShouldContain(e => e.TableName == "Library" && e.ColumnNames.Contains("details_ar"));
            deleteColumnExpressions.ShouldContain(e => e.TableName == "Library" && e.ColumnNames.Contains("organization_details_ar"));
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

    internal class TestInsertDataAutoReversingMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Foo")
                .WithColumn("Id").AsInt32().PrimaryKey()
                .WithColumn("ParentId").AsInt32().Nullable()
                    .ForeignKey("Foo", "Id");

            Insert.IntoTable("Foo")
                .Row(new { Id = 1 })
                .Row(new { Id = 2, ParentId = 1 });
        }
    }

    internal class TestAlterTableAddColumnAutoReversingMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Alter.Table("Library")
                .AddColumn("organization_details_ar").AsString(2000).Nullable()
                .AddColumn("details_ar").AsString(2000).Nullable()
                .AddColumn("address_ar").AsString(250).Nullable();
        }
    }
}
