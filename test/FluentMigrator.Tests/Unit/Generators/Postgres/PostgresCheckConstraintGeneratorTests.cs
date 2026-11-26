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

using System;

using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresCheckConstraintGeneratorTests
    {
        private const string QualifiedTable = "\"inventory\".\"Items\"";
        private const string QuotedConstraint = "\"CK_Quantity\"";
        private const string Condition = "quantity > 0";

        private PostgresGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _generator = new PostgresGenerator(new PostgresQuoter(new PostgresOptions()));
        }

        [Test]
        public void CreatesCheckConstraintWithoutValidatingExistingRows()
        {
            var expression = new CreateCheckConstraintExpression { Constraint = CreateDefinition(Condition) };
            expression.Constraint.NoCheck = true;
            _generator.CompatibilityMode = CompatibilityMode.STRICT;

            _generator.Generate(expression).ShouldBe($"ALTER TABLE {QualifiedTable} ADD CONSTRAINT {QuotedConstraint} CHECK ({Condition}) NOT VALID;");
        }

        [Test]
        public void EnablingCheckConstraintValidatesExistingRows()
        {
            var expression = new AlterCheckConstraintExpression
            {
                Constraint = CreateDefinition(),
                Action = CheckConstraintAction.Enable,
            };
            _generator.CompatibilityMode = CompatibilityMode.STRICT;

            _generator.Generate(expression).ShouldBe($"ALTER TABLE {QualifiedTable} VALIDATE CONSTRAINT {QuotedConstraint};");
        }

        [Test]
        public void EnablingCheckConstraintWithoutValidationGeneratesNoSql()
        {
            var expression = new AlterCheckConstraintExpression
            {
                Constraint = CreateDefinition(),
                Action = CheckConstraintAction.Enable,
            };
            expression.Constraint.NoCheck = true;
            _generator.CompatibilityMode = CompatibilityMode.STRICT;

            _generator.Generate(expression).ShouldBeEmpty();
        }

        [Test]
        public void ChangesCheckConstraintConditionWithoutValidatingExistingRows()
        {
            var expression = new AlterCheckConstraintExpression
            {
                Constraint = CreateDefinition(Condition),
                Action = CheckConstraintAction.ChangeCondition,
            };
            expression.Constraint.NoCheck = true;
            _generator.CompatibilityMode = CompatibilityMode.STRICT;

            _generator.Generate(expression).ShouldBe(
                $"ALTER TABLE {QualifiedTable} DROP CONSTRAINT {QuotedConstraint};"
                + Environment.NewLine
                + $"ALTER TABLE {QualifiedTable} ADD CONSTRAINT {QuotedConstraint} CHECK ({Condition}) NOT VALID;");
        }

        [Test]
        public void CreatingDisabledCheckConstraintFailsInBothModes([Values] CompatibilityMode compatibilityMode)
        {
            var expression = new CreateCheckConstraintExpression { Constraint = CreateDefinition(Condition) };
            expression.Constraint.Enabled = false;
            _generator.CompatibilityMode = compatibilityMode;

            Should.Throw<DatabaseOperationNotSupportedException>(() => _generator.Generate(expression));
        }

        [Test]
        public void DisablingExistingCheckConstraintUsesCompatibilityMode([Values] CompatibilityMode compatibilityMode)
        {
            var expression = new AlterCheckConstraintExpression
            {
                Constraint = CreateDefinition(),
                Action = CheckConstraintAction.Disable,
            };
            _generator.CompatibilityMode = compatibilityMode;

            AssertUnsupported(() => _generator.Generate(expression), compatibilityMode);
        }

        [Test]
        public void ReplacingCheckConstraintWithDisabledConstraintFailsInBothModes([Values] CompatibilityMode compatibilityMode)
        {
            var expression = new AlterCheckConstraintExpression
            {
                Constraint = CreateDefinition(Condition),
                Action = CheckConstraintAction.ChangeCondition,
            };
            expression.Constraint.Enabled = false;
            _generator.CompatibilityMode = compatibilityMode;

            Should.Throw<DatabaseOperationNotSupportedException>(() => _generator.Generate(expression));
        }

        private static CheckConstraintDefinition CreateDefinition(string condition = null) => new()
        {
            ConstraintName = "CK_Quantity",
            TableName = "Items",
            SchemaName = "inventory",
            Condition = condition,
        };

        private static void AssertUnsupported(Func<string> generate, CompatibilityMode compatibilityMode)
        {
            if (compatibilityMode == CompatibilityMode.STRICT)
            {
                Should.Throw<DatabaseOperationNotSupportedException>(() => generate());
            }
            else
            {
                generate().ShouldBeEmpty();
            }
        }
    }
}
