#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Hana;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Hana
{
    [TestFixture]
    [Category("Hana")]
    public class HanaCheckConstraintTests
    {
        [Test]
        public void CreatesCheckConstraintInCurrentSchema()
        {
            var expression = new CreateCheckConstraintExpression
            {
                Constraint = { SchemaName = "Inventory", TableName = "Items", ConstraintName = "CK Items", Condition = "\"Quantity\" > 0" },
            };

            new HanaGenerator().Generate(expression).ShouldBe(
                "ALTER TABLE \"Items\" ADD CONSTRAINT \"CK Items\" CHECK (\"Quantity\" > 0);");
        }

        [Test]
        public void DropsCheckConstraintInCurrentSchema()
        {
            var expression = new DeleteCheckConstraintExpression
            {
                Constraint = { SchemaName = "Inventory", TableName = "Items", ConstraintName = "CK Items" },
            };

            new HanaGenerator().Generate(expression).ShouldBe("ALTER TABLE \"Items\" DROP CONSTRAINT \"CK Items\";");
        }

        [Test]
        public void ReplacesCheckConstraintInOneAnonymousBlockAndEscapesSqlLiterals()
        {
            var expression = new AlterCheckConstraintExpression
            {
                Action = CheckConstraintAction.ChangeCondition,
                Constraint = { TableName = "Items", ConstraintName = "Item's check", Condition = "\"Code\" <> 'Don''t;stop'" },
            };

            new HanaGenerator().Generate(expression).ShouldBe(
                "DO BEGIN EXECUTE IMMEDIATE 'ALTER TABLE \"Items\" DROP CONSTRAINT \"Item''s check\"'; " +
                "EXECUTE IMMEDIATE 'ALTER TABLE \"Items\" ADD CONSTRAINT \"Item''s check\" CHECK (\"Code\" <> ''Don''''t;stop'')'; END;");
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void RejectsCheckConstraintCreationWithUnverifiedOptions(bool enabled, bool noCheck)
        {
            var expression = new CreateCheckConstraintExpression
            {
                Constraint = { TableName = "Items", ConstraintName = "CK Items", Condition = "\"Quantity\" > 0", Enabled = enabled, NoCheck = noCheck },
            };

            var generator = new HanaGenerator { CompatibilityMode = CompatibilityMode.STRICT };
            Assert.Throws<DatabaseOperationNotSupportedException>(() => generator.Generate(expression));
        }

        [TestCase(CheckConstraintAction.Enable, false)]
        [TestCase(CheckConstraintAction.Enable, true)]
        [TestCase(CheckConstraintAction.Disable, false)]
        public void RejectsUnverifiedChangesToEnforcement(CheckConstraintAction action, bool noCheck)
        {
            var expression = new AlterCheckConstraintExpression
            {
                Action = action,
                Constraint = { TableName = "Items", ConstraintName = "CK Items", NoCheck = noCheck },
            };

            var generator = new HanaGenerator { CompatibilityMode = CompatibilityMode.STRICT };
            Assert.Throws<DatabaseOperationNotSupportedException>(() => generator.Generate(expression));
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void LooseModeRejectsTheReplacementWhenAnOptionIsUnsupported(bool enabled, bool noCheck)
        {
            var expression = new AlterCheckConstraintExpression
            {
                Action = CheckConstraintAction.ChangeCondition,
                Constraint = { TableName = "Items", ConstraintName = "CK Items", Condition = "\"Quantity\" > 0", Enabled = enabled, NoCheck = noCheck },
            };

            var generator = new HanaGenerator { CompatibilityMode = CompatibilityMode.LOOSE };
            Assert.Throws<DatabaseOperationNotSupportedException>(() => generator.Generate(expression));
        }
    }
}
