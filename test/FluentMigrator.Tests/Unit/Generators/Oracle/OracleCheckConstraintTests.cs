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

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Oracle;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    [TestFixture]
    public class OracleCheckConstraintTests
    {
        [TestCase(true, false, "")]
        [TestCase(true, true, " ENABLE NOVALIDATE")]
        [TestCase(false, false, " DISABLE")]
        public void CreatesCheckConstraintWithRequestedEnforcement(bool enabled, bool noCheck, string state)
        {
            var expression = new CreateCheckConstraintExpression
            {
                Constraint = { SchemaName = "Inventory", TableName = "Items", Name = "CK_Items", Condition = "Quantity > 0", Enabled = enabled, NoCheck = noCheck },
            };

            new OracleGenerator().Generate(expression).ShouldBe(
                $"ALTER TABLE Inventory.Items ADD CONSTRAINT CK_Items CHECK (Quantity > 0){state};");
        }

        [TestCase(CheckConstraintAction.Enable, false, "ENABLE VALIDATE")]
        [TestCase(CheckConstraintAction.Enable, true, "ENABLE NOVALIDATE")]
        [TestCase(CheckConstraintAction.Disable, false, "DISABLE")]
        public void ChangesCheckConstraintEnforcement(CheckConstraintAction action, bool noCheck, string state)
        {
            var expression = new AlterCheckConstraintExpression
            {
                Action = action,
                Constraint = { SchemaName = "Inventory", TableName = "Items", Name = "CK_Items", NoCheck = noCheck },
            };

            new OracleGenerator().Generate(expression).ShouldBe(
                $"ALTER TABLE Inventory.Items MODIFY CONSTRAINT CK_Items {state};");
        }

        [Test]
        public void DropsCheckConstraintWithOracleIdentifierQuoting()
        {
            var expression = new DeleteCheckConstraintExpression
            {
                Constraint = { SchemaName = "Inventory", TableName = "Items", Name = "CK Items" },
            };

            new OracleGenerator(useQuotedIdentifiers: true).Generate(expression).ShouldBe(
                "ALTER TABLE \"Inventory\".\"Items\" DROP CONSTRAINT \"CK Items\";");
        }

        [Test]
        public void ReplacesCheckConstraintInOneBlockAndEscapesSqlLiterals()
        {
            var expression = new AlterCheckConstraintExpression
            {
                Action = CheckConstraintAction.ChangeCondition,
                Constraint = { SchemaName = "Inventory", TableName = "Items", Name = "Item's check", Condition = "\"Code\" <> 'Don''t;stop'", NoCheck = true },
            };

            new OracleGenerator(useQuotedIdentifiers: true).Generate(expression).ShouldBe(
                "BEGIN EXECUTE IMMEDIATE 'ALTER TABLE \"Inventory\".\"Items\" DROP CONSTRAINT \"Item''s check\"'; " +
                "EXECUTE IMMEDIATE 'ALTER TABLE \"Inventory\".\"Items\" ADD CONSTRAINT \"Item''s check\" CHECK (\"Code\" <> ''Don''''t;stop'') ENABLE NOVALIDATE'; END;");
        }
    }
}
