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
using FluentMigrator.Runner.Generators.MySql;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.MySql8;

[TestFixture]
public class MySqlCheckConstraintTests
{
    private readonly MySql8Generator _generator = new MySql8Generator();

    [TestCase(true, "")]
    [TestCase(false, " NOT ENFORCED")]
    public void ReplacesThePredicateInOneStatement(bool enabled, string enforcement)
    {
        var alter = new AlterCheckConstraintExpression
        {
            Action = CheckConstraintAction.ChangeCondition,
            Constraint = { Name = "ck", TableName = "items", Condition = "qty >= 1", Enabled = enabled }
        };
        Assert.That(_generator.Generate(alter), Is.EqualTo("ALTER TABLE `items` DROP CONSTRAINT `ck`, ADD CONSTRAINT `ck` CHECK (qty >= 1)" + enforcement + ";"));
    }

    [TestCase(CheckConstraintAction.Enable, "ENFORCED")]
    [TestCase(CheckConstraintAction.Disable, "NOT ENFORCED")]
    public void ChangesEnforcement(CheckConstraintAction action, string enforcement)
    {
        var alter = new AlterCheckConstraintExpression
        {
            Action = action,
            Constraint = { Name = "ck", TableName = "items" }
        };
        Assert.That(_generator.Generate(alter), Is.EqualTo("ALTER TABLE `items` ALTER CHECK `ck` " + enforcement + ";"));
    }

    [Test]
    public void CanCreateAnUnenforcedCheck()
    {
        var create = new CreateCheckConstraintExpression
        {
            Constraint = { Name = "ck", TableName = "items", Condition = "qty > 0", Enabled = false }
        };
        Assert.That(_generator.Generate(create), Is.EqualTo("ALTER TABLE `items` ADD CONSTRAINT `ck` CHECK (qty > 0) NOT ENFORCED;"));
    }

    [Test]
    public void StrictModeExplainsWhyEnablingWithoutValidationIsUnsupported()
    {
        var generator = new MySql8Generator { CompatibilityMode = CompatibilityMode.STRICT };
        var alter = new AlterCheckConstraintExpression
        {
            Action = CheckConstraintAction.Enable,
            Constraint = { Name = "ck", TableName = "items", NoCheck = true }
        };

        var error = Assert.Throws<DatabaseOperationNotSupportedException>(() => generator.Generate(alter));
        Assert.That(error.Message, Does.Contain("without validating existing rows"));
    }

    [Test]
    public void UnsupportedReplacementOptionLeavesTheOriginalCheckAlone()
    {
        var generator = new MySql8Generator { CompatibilityMode = CompatibilityMode.LOOSE };
        var alter = new AlterCheckConstraintExpression
        {
            Action = CheckConstraintAction.ChangeCondition,
            Constraint = { Name = "ck", TableName = "items", Condition = "qty > 0", NoCheck = true }
        };
        Assert.That(generator.Generate(alter), Is.Empty);
    }
}
