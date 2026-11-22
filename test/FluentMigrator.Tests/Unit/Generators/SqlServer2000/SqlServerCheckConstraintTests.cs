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

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.SqlServer;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2000;

[TestFixture]
public class SqlServerCheckConstraintTests
{
    private readonly SqlServer2000Generator _generator = new SqlServer2000Generator();

    [Test]
    public void SkipsExistingRowsWhenAddingAnUnvalidatedCheck()
    {
        var expression = new CreateCheckConstraintExpression
        {
            Constraint = { ConstraintName = "ck", TableName = "items", Condition = "qty > 0", NoCheck = true }
        };
        Assert.That(_generator.Generate(expression), Is.EqualTo("ALTER TABLE [items] WITH NOCHECK ADD CONSTRAINT [ck] CHECK (qty > 0);"));
    }

    [Test]
    public void AddsWithoutCheckingExistingRowsBeforeDisabling()
    {
        var expression = new CreateCheckConstraintExpression
        {
            Constraint = { ConstraintName = "ck", TableName = "items", Condition = "qty > 0", Enabled = false }
        };
        Assert.That(_generator.Generate(expression), Is.EqualTo("ALTER TABLE [items] WITH NOCHECK ADD CONSTRAINT [ck] CHECK (qty > 0);"
            + Environment.NewLine + "ALTER TABLE [items] NOCHECK CONSTRAINT [ck];"));
    }

    [TestCase(CheckConstraintAction.Disable, false, "NOCHECK CONSTRAINT")]
    [TestCase(CheckConstraintAction.Enable, false, "WITH CHECK CHECK CONSTRAINT")]
    [TestCase(CheckConstraintAction.Enable, true, "CHECK CONSTRAINT")]
    public void AltersEnforcement(CheckConstraintAction action, bool noCheck, string operation)
    {
        var expression = new AlterCheckConstraintExpression
        {
            Action = action,
            Constraint = { ConstraintName = "ck", TableName = "items", NoCheck = noCheck }
        };
        Assert.That(_generator.Generate(expression), Is.EqualTo($"ALTER TABLE [items] {operation} [ck];"));
    }
}
