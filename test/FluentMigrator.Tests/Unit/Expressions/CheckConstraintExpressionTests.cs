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
using FluentMigrator.Tests.Helpers;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Expressions;

[TestFixture]
public class CheckConstraintExpressionTests
{
    [Test]
    public void ReverseOfCreateOnlyCarriesTheConstraintIdentity()
    {
        var create = new CreateCheckConstraintExpression
        {
            Constraint = { Name = "ck_quantity", TableName = "items", SchemaName = "stock", Condition = "quantity > 0", NoCheck = true }
        };

        var delete = (DeleteCheckConstraintExpression)create.Reverse();

        Assert.That(delete.Constraint, Is.EqualTo(new CheckConstraintDefinition
        {
            Name = "ck_quantity",
            TableName = "items",
            SchemaName = "stock"
        }).UsingPropertiesComparer());
    }

    [TestCase(CheckConstraintAction.None, null, false)]
    [TestCase(CheckConstraintAction.ChangeCondition, " ", false)]
    [TestCase(CheckConstraintAction.ChangeCondition, "quantity > 0", true)]
    [TestCase(CheckConstraintAction.Enable, null, true)]
    [TestCase(CheckConstraintAction.Disable, null, true)]
    [TestCase(CheckConstraintAction.Enable, "quantity > 0", false)]
    [TestCase(CheckConstraintAction.Disable, "quantity > 0", false)]
    public void AlterValidatesTheSelectedAction(CheckConstraintAction action, string condition, bool valid)
    {
        var alter = new AlterCheckConstraintExpression
        {
            Action = action,
            Constraint = { Name = "ck_quantity", TableName = "items", Condition = condition }
        };

        Assert.That(ValidationHelper.CollectErrors(alter).Count == 0, Is.EqualTo(valid));
    }

    [Test]
    public void DeleteCannotRestoreAnUnknownCondition()
    {
        Assert.Throws<NotSupportedException>(() => new DeleteCheckConstraintExpression().Reverse());
    }
}
