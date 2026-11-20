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
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors.Firebird;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Firebird;

[TestFixture]
[Category("Generator")]
[Category("Firebird")]
public class FirebirdCheckConstraintGeneratorTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void CreateUsesTheSameSqlWithOrWithoutAnExplicitRequestToSkipValidation(bool noCheck)
    {
        var options = FirebirdOptions.AutoCommitBehaviour();
        options.ForceQuote = true;
        var generator = new FirebirdGenerator(options) { CompatibilityMode = CompatibilityMode.STRICT };
        var expression = new CreateCheckConstraintExpression
        {
            Constraint =
            {
                TableName = "Items",
                Name = "CheckItems",
                Condition = "quantity > 0",
                NoCheck = noCheck,
            },
        };

        Assert.That(generator.Generate(expression), Is.EqualTo("ALTER TABLE \"Items\" ADD CONSTRAINT \"CheckItems\" CHECK (quantity > 0);"));
    }

    [Test]
    public void ReplacementPreviewUsesTheSameTruncatedNamesForBothStatements()
    {
        var options = FirebirdOptions.AutoCommitBehaviour();
        options.ForceQuote = true;
        var generator = new FirebirdGenerator(options) { CompatibilityMode = CompatibilityMode.STRICT };
        var expression = new AlterCheckConstraintExpression
        {
            Action = CheckConstraintAction.ChangeCondition,
            Constraint =
            {
                TableName = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789",
                Name = "abcdefghijklmnopqrstuvwxyz123456789",
                Condition = "quantity > 0",
            },
        };

        Assert.That(generator.Generate(expression), Is.EqualTo("ALTER TABLE \"ABCDEFGHIJKLMNOPQRSTUVWXYZ12345\" DROP CONSTRAINT \"abcdefghijklmnopqrstuvwxyz12345\";"
            + Environment.NewLine + "ALTER TABLE \"ABCDEFGHIJKLMNOPQRSTUVWXYZ12345\" ADD CONSTRAINT \"abcdefghijklmnopqrstuvwxyz12345\" CHECK (quantity > 0);"));
    }

    [Test]
    public void DisablingAnExistingConstraintIsUnsupported()
    {
        var generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour()) { CompatibilityMode = CompatibilityMode.STRICT };
        var expression = new AlterCheckConstraintExpression { Action = CheckConstraintAction.Disable };

        Assert.That(() => generator.Generate(expression), Throws.TypeOf<DatabaseOperationNotSupportedException>());
    }
}
