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
using FluentMigrator.Runner.Generators.DB2;
using FluentMigrator.Runner.Generators.DB2.iSeries;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.DB2;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Db2;

[TestFixture]
[Category("Generator")]
[Category("Db2")]
public class Db2CheckConstraintGeneratorTests
{
    [Test]
    public void ConnectionlessAndConnectedProcessingHonourTheReplacementGenerator()
    {
        var generator = new DecliningGenerator();
        var options = Mock.Of<IOptionsSnapshot<ProcessorOptions>>(o => o.Value == new ProcessorOptions());
        var preview = new ConnectionlessProcessor(Mock.Of<IGeneratorAccessor>(a => a.Generator == generator),
            NullLogger.Instance, options, Array.Empty<string>());
        using var processor = new Db2Processor(null, generator, new Db2Quoter(), NullLogger<Db2Processor>.Instance,
            options, Mock.Of<IMigrationConnectionFactory>());
        var expression = new AlterCheckConstraintExpression { Action = CheckConstraintAction.ChangeCondition, Constraint = NewConstraint() };

        Assert.Multiple(() =>
        {
            Assert.That(() => preview.Process(expression), Throws.TypeOf<DatabaseOperationNotSupportedException>().With.Message.EqualTo("Replacement declined"));
            Assert.That(() => processor.Process(expression), Throws.TypeOf<DatabaseOperationNotSupportedException>().With.Message.EqualTo("Replacement declined"));
        });
    }

    private sealed class DecliningGenerator : Db2Generator
    {
        protected internal override IReadOnlyList<string> GenerateCheckConstraintReplacement(AlterCheckConstraintExpression expression)
        {
            throw new DatabaseOperationNotSupportedException("Replacement declined");
        }
    }

    [TestCase(false, "\"Good-Check\"")]
    [TestCase(true, "\"App-Schema\".\"Good-Check\"")]
    public void CreateQuotesTheConstraintUsingTheProviderNamingRules(bool iSeries, string constraintName)
    {
        var generator = iSeries ? new Db2ISeriesGenerator() : new Db2Generator();
        var expression = new CreateCheckConstraintExpression { Constraint = NewConstraint() };

        Assert.That(generator.Generate(expression), Is.EqualTo(
            $"ALTER TABLE \"App-Schema\".ITEMS ADD CONSTRAINT {constraintName} CHECK (QUANTITY > 0);"));
    }

    [TestCase(false, "\"Good-Check\"")]
    [TestCase(true, "\"App-Schema\".\"Good-Check\"")]
    public void DropUsesTheCheckSpecificOperation(bool iSeries, string constraintName)
    {
        var generator = iSeries ? new Db2ISeriesGenerator() : new Db2Generator();
        var expression = new DeleteCheckConstraintExpression { Constraint = NewConstraint() };

        Assert.That(generator.Generate(expression), Is.EqualTo($"ALTER TABLE \"App-Schema\".ITEMS DROP CHECK {constraintName};"));
    }

    [TestCase(false, "\"Good-Check\"")]
    [TestCase(true, "\"App-Schema\".\"Good-Check\"")]
    public void ReplacementPreviewHasTwoTerminatedStatements(bool iSeries, string constraintName)
    {
        var generator = iSeries ? new Db2ISeriesGenerator() : new Db2Generator();
        var expression = new AlterCheckConstraintExpression { Action = CheckConstraintAction.ChangeCondition, Constraint = NewConstraint() };

        Assert.That(generator.Generate(expression), Is.EqualTo($"ALTER TABLE \"App-Schema\".ITEMS DROP CHECK {constraintName};"
            + Environment.NewLine + $"ALTER TABLE \"App-Schema\".ITEMS ADD CONSTRAINT {constraintName} CHECK (QUANTITY > 0);"));
    }

    [Test]
    public void AddingADisabledConstraintAlsoDisablesQueryOptimisation()
    {
        var generator = new Db2Generator { CompatibilityMode = CompatibilityMode.STRICT };
        var expression = new CreateCheckConstraintExpression { Constraint = NewConstraint() };
        expression.Constraint.Enabled = false;

        Assert.That(generator.Generate(expression), Is.EqualTo(
            "ALTER TABLE \"App-Schema\".ITEMS ADD CONSTRAINT \"Good-Check\" CHECK (QUANTITY > 0) NOT ENFORCED DISABLE QUERY OPTIMIZATION;"));
    }

    [TestCase(CheckConstraintAction.Disable, "NOT ENFORCED DISABLE QUERY OPTIMIZATION")]
    [TestCase(CheckConstraintAction.Enable, "ENFORCED ENABLE QUERY OPTIMIZATION")]
    public void ChangingEnforcementAlsoChangesQueryOptimisation(CheckConstraintAction action, string enforcement)
    {
        var generator = new Db2Generator { CompatibilityMode = CompatibilityMode.STRICT };
        var expression = new AlterCheckConstraintExpression { Action = action, Constraint = NewConstraint() };
        expression.Constraint.Condition = null;

        Assert.That(generator.Generate(expression), Is.EqualTo($"ALTER TABLE \"App-Schema\".ITEMS ALTER CHECK \"Good-Check\" {enforcement};"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void EnablingWithoutValidatingExistingRowsIsUnsupported(bool iSeries)
    {
        var generator = iSeries ? new Db2ISeriesGenerator() : new Db2Generator();
        generator.CompatibilityMode = CompatibilityMode.STRICT;
        var expression = new AlterCheckConstraintExpression { Action = CheckConstraintAction.Enable, Constraint = NewConstraint() };
        expression.Constraint.Condition = null;
        expression.Constraint.NoCheck = true;

        Assert.That(() => generator.Generate(expression), Throws.TypeOf<DatabaseOperationNotSupportedException>());
    }

    [TestCase(CheckConstraintAction.Enable)]
    [TestCase(CheckConstraintAction.Disable)]
    public void Db2ForIRejectsEnforcementChanges(CheckConstraintAction action)
    {
        var generator = new Db2ISeriesGenerator { CompatibilityMode = CompatibilityMode.STRICT };
        var expression = new AlterCheckConstraintExpression { Action = action, Constraint = NewConstraint() };
        expression.Constraint.Condition = null;

        Assert.That(() => generator.Generate(expression), Throws.TypeOf<DatabaseOperationNotSupportedException>());
    }

    [Test]
    public void Db2ForIRejectsAReplacementWithADisabledConstraintInLooseMode()
    {
        var generator = new Db2ISeriesGenerator { CompatibilityMode = CompatibilityMode.LOOSE };
        var expression = new AlterCheckConstraintExpression { Action = CheckConstraintAction.ChangeCondition, Constraint = NewConstraint() };
        expression.Constraint.Enabled = false;

        Assert.Throws<DatabaseOperationNotSupportedException>(() => generator.Generate(expression));
    }

    private static CheckConstraintDefinition NewConstraint()
    {
        return new CheckConstraintDefinition
        {
            TableName = "ITEMS",
            SchemaName = "App-Schema",
            ConstraintName = "Good-Check",
            Condition = "QUANTITY > 0",
        };
    }
}
