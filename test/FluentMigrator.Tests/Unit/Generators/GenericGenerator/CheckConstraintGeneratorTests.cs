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
using System.Collections.Generic;

using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Generic;

using Microsoft.Extensions.Options;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.GenericGenerator
{
    [TestFixture]
    [Category("Generator")]
    public class CheckConstraintGeneratorTests
    {
        [Test]
        public void CreateQuotesIdentifiersAndPreservesTheCondition()
        {
            var generator = new AnsiGenerator(CompatibilityMode.STRICT);
            var expression = new CreateCheckConstraintExpression { Constraint = NewConstraint() };

            Assert.That(
                generator.Generate(expression),
                Is.EqualTo("ALTER TABLE \"inventory\".\"Order Items\" ADD CONSTRAINT \"Positive Quantity\" CHECK (quantity > 0);"));
        }

        [Test]
        public void DeleteQuotesIdentifiers()
        {
            var generator = new AnsiGenerator(CompatibilityMode.STRICT);
            var expression = new DeleteCheckConstraintExpression { Constraint = NewConstraint() };

            Assert.That(
                generator.Generate(expression),
                Is.EqualTo("ALTER TABLE \"inventory\".\"Order Items\" DROP CONSTRAINT \"Positive Quantity\";"));
        }

        [Test]
        public void AlterChangesTheConditionByDroppingAndRecreatingTheConstraint()
        {
            var generator = new AnsiGenerator(CompatibilityMode.STRICT);
            var expression = new AlterCheckConstraintExpression
            {
                Action = CheckConstraintAction.ChangeCondition,
                Constraint = NewConstraint(),
            };

            Assert.That(
                generator.Generate(expression),
                Is.EqualTo("ALTER TABLE \"inventory\".\"Order Items\" DROP CONSTRAINT \"Positive Quantity\";" + Environment.NewLine
                    + "ALTER TABLE \"inventory\".\"Order Items\" ADD CONSTRAINT \"Positive Quantity\" CHECK (quantity > 0);"));
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void CreateRejectsUnsupportedOptionsInStrictMode(bool noCheck, bool enabled)
        {
            var generator = new AnsiGenerator(CompatibilityMode.STRICT);
            var expression = new CreateCheckConstraintExpression { Constraint = NewConstraint() };
            expression.Constraint.NoCheck = noCheck;
            expression.Constraint.Enabled = enabled;

            Assert.That(() => generator.Generate(expression), Throws.TypeOf<DatabaseOperationNotSupportedException>());
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void AlterSkipsTheWholeReplacementWhenTheNewConstraintIsUnsupported(bool noCheck, bool enabled)
        {
            var generator = new AnsiGenerator(CompatibilityMode.LOOSE);
            var expression = new AlterCheckConstraintExpression
            {
                Action = CheckConstraintAction.ChangeCondition,
                Constraint = NewConstraint(),
            };
            expression.Constraint.NoCheck = noCheck;
            expression.Constraint.Enabled = enabled;

            Assert.That(generator.Generate(expression), Is.Empty);
        }

        [Test]
        public void AlterSkipsTheWholeReplacementWhenTheExistingConstraintCannotBeDropped()
        {
            var generator = new CreateOnlyGenerator();
            var expression = new AlterCheckConstraintExpression
            {
                Action = CheckConstraintAction.ChangeCondition,
                Constraint = NewConstraint(),
            };

            Assert.That(generator.Generate(expression), Is.Empty);
        }

        [TestCase(CheckConstraintAction.Enable)]
        [TestCase(CheckConstraintAction.Disable)]
        public void AlterRejectsUnsupportedActionsInStrictMode(CheckConstraintAction action)
        {
            var generator = new AnsiGenerator(CompatibilityMode.STRICT);
            var expression = new AlterCheckConstraintExpression
            {
                Action = action,
                Constraint = NewConstraint(),
            };
            expression.Constraint.Condition = null;

            Assert.That(() => generator.Generate(expression), Throws.TypeOf<DatabaseOperationNotSupportedException>());
        }

        private static CheckConstraintDefinition NewConstraint()
        {
            return new CheckConstraintDefinition
            {
                Name = "Positive Quantity",
                TableName = "Order Items",
                SchemaName = "inventory",
                Condition = "quantity > 0",
            };
        }

        private class AnsiGenerator : Runner.Generators.Generic.GenericGenerator
        {
            public AnsiGenerator(CompatibilityMode mode)
                : base(null, new GenericQuoter(), null, Options.Create(new GeneratorOptions { CompatibilityMode = mode }))
            {
            }

            public override string GeneratorId => "ANSI";

            public override List<string> GeneratorIdAliases { get; } = new List<string>();

            public override string Generate(AlterDefaultConstraintExpression expression)
            {
                throw new NotSupportedException();
            }

            public override string Generate(DeleteDefaultConstraintExpression expression)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class CreateOnlyGenerator : AnsiGenerator
        {
            public CreateOnlyGenerator()
                : base(CompatibilityMode.LOOSE)
            {
            }

            public override string Generate(DeleteCheckConstraintExpression expression)
            {
                return CompatibilityMode.HandleCompatibility("Dropping check constraints is not supported");
            }
        }
    }
}
