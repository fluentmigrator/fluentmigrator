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

using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders
{
    [TestFixture]
    [Category("Builder")]
    public class CheckConstraintBuilderTests
    {
        private IMigrationContext _context;

        [SetUp]
        public void SetUp()
        {
            var context = new Mock<IMigrationContext>();
            context.SetupGet(x => x.Expressions).Returns(new List<IMigrationExpression>());
            _context = context.Object;
        }

        [Test]
        public void CreateUnnamedCheckRecordsColumnsAndCondition()
        {
            ICreateExpressionRoot create = new CreateExpressionRoot(_context);
            create.Check().OnTable("Bookings").InSchema("inventory")
                .Columns("StartDate", "EndDate").Condition("EndDate > StartDate");

            var constraint = _context.Expressions.Single().ShouldBeOfType<CreateCheckConstraintExpression>().Constraint;
            constraint.Columns.ShouldBe(new[] { "StartDate", "EndDate" });
            constraint.SchemaName.ShouldBe("inventory");
            constraint.Condition.ShouldBe("EndDate > StartDate");
        }

        [Test]
        public void AlterUnnamedCheckSelectsAnActionAfterTheColumn()
        {
            IAlterExpressionRoot alter = new AlterExpressionRoot(_context);
            alter.Check().OnTable("Items").InSchema("inventory").Column("Quantity").Disabled();

            var expression = _context.Expressions.Single().ShouldBeOfType<AlterCheckConstraintExpression>();
            expression.Constraint.Columns.ShouldBe(new[] { "Quantity" });
            expression.Constraint.SchemaName.ShouldBe("inventory");
            expression.Action.ShouldBe(CheckConstraintAction.Disable);
        }

        [Test]
        public void DeleteUnnamedCheckRecordsTheColumnsInOrder()
        {
            IDeleteExpressionRoot delete = new DeleteExpressionRoot(_context);
            delete.Check().FromTable("Bookings").InSchema("inventory").Columns("StartDate", "EndDate");

            var constraint = _context.Expressions.Single().ShouldBeOfType<DeleteCheckConstraintExpression>().Constraint;
            constraint.Columns.ShouldBe(new[] { "StartDate", "EndDate" });
            constraint.SchemaName.ShouldBe("inventory");
        }

        [Test]
        public void CreateCheckAddsNamedConstraintWithTableSchemaAndCondition()
        {
            ICreateExpressionRoot create = new CreateExpressionRoot(_context);

            create.Check("CK_Items_Quantity")
                .OnTable("Items").InSchema("inventory")
                .Condition("Quantity > 0");

            var expression = _context.Expressions.Single().ShouldBeOfType<CreateCheckConstraintExpression>();
            Assert.Multiple(() =>
            {
                expression.Constraint.Name.ShouldBe("CK_Items_Quantity");
                expression.Constraint.TableName.ShouldBe("Items");
                expression.Constraint.SchemaName.ShouldBe("inventory");
                expression.Constraint.Condition.ShouldBe("Quantity > 0");
                expression.Constraint.Enabled.ShouldBeTrue();
                expression.Constraint.NoCheck.ShouldBeFalse();
            });
        }

        [Test]
        public void CreateCheckWithNoCheckSkipsValidationOfExistingRows()
        {
            ICreateExpressionRoot create = new CreateExpressionRoot(_context);

            create.Check("CK_Items_Quantity").OnTable("Items").Condition("Quantity > 0").WithNoCheck();

            var expression = _context.Expressions.Single().ShouldBeOfType<CreateCheckConstraintExpression>();
            Assert.Multiple(() =>
            {
                expression.Constraint.NoCheck.ShouldBeTrue();
                expression.Constraint.Enabled.ShouldBeTrue();
            });
        }

        [Test]
        public void CreateCheckDisabledCreatesAnUnenforcedConstraint()
        {
            ICreateExpressionRoot create = new CreateExpressionRoot(_context);

            create.Check("CK_Items_Quantity").OnTable("Items").Condition("Quantity > 0").Disabled();

            var expression = _context.Expressions.Single().ShouldBeOfType<CreateCheckConstraintExpression>();
            expression.Constraint.Enabled.ShouldBeFalse();
        }

        [Test]
        public void AlterCheckWithConditionChangesTheCondition()
        {
            IAlterExpressionRoot alter = new AlterExpressionRoot(_context);

            alter.Check("CK_Items_Quantity")
                .OnTable("Items").InSchema("inventory")
                .Condition("Quantity >= 0");

            var expression = _context.Expressions.Single().ShouldBeOfType<AlterCheckConstraintExpression>();
            Assert.Multiple(() =>
            {
                expression.Action.ShouldBe(CheckConstraintAction.ChangeCondition);
                expression.Constraint.Name.ShouldBe("CK_Items_Quantity");
                expression.Constraint.TableName.ShouldBe("Items");
                expression.Constraint.SchemaName.ShouldBe("inventory");
                expression.Constraint.Condition.ShouldBe("Quantity >= 0");
                expression.Constraint.Enabled.ShouldBeTrue();
                expression.Constraint.NoCheck.ShouldBeFalse();
            });
        }

        [Test]
        public void AlterCheckWithConditionAndNoCheckSkipsValidationOfExistingRows()
        {
            IAlterExpressionRoot alter = new AlterExpressionRoot(_context);

            alter.Check("CK_Items_Quantity").OnTable("Items").Condition("Quantity >= 0").WithNoCheck();

            var expression = _context.Expressions.Single().ShouldBeOfType<AlterCheckConstraintExpression>();
            Assert.Multiple(() =>
            {
                expression.Action.ShouldBe(CheckConstraintAction.ChangeCondition);
                expression.Constraint.NoCheck.ShouldBeTrue();
            });
        }

        [Test]
        public void AlterCheckWithConditionAndDisabledReplacesTheConstraintWithoutEnforcement()
        {
            IAlterExpressionRoot alter = new AlterExpressionRoot(_context);

            alter.Check("CK_Items_Quantity").OnTable("Items").Condition("Quantity >= 0").Disabled();

            var expression = _context.Expressions.Single().ShouldBeOfType<AlterCheckConstraintExpression>();
            Assert.Multiple(() =>
            {
                expression.Action.ShouldBe(CheckConstraintAction.ChangeCondition);
                expression.Constraint.Enabled.ShouldBeFalse();
            });
        }

        [Test]
        public void AlterCheckEnabledEnablesTheExistingConstraint()
        {
            IAlterExpressionRoot alter = new AlterExpressionRoot(_context);

            alter.Check("CK_Items_Quantity").OnTable("Items").Enabled();

            var expression = _context.Expressions.Single().ShouldBeOfType<AlterCheckConstraintExpression>();
            Assert.Multiple(() =>
            {
                expression.Action.ShouldBe(CheckConstraintAction.Enable);
                expression.Constraint.NoCheck.ShouldBeFalse();
            });
        }

        [Test]
        public void AlterCheckEnabledWithNoCheckEnablesTheExistingConstraintWithoutValidation()
        {
            IAlterExpressionRoot alter = new AlterExpressionRoot(_context);

            alter.Check("CK_Items_Quantity").OnTable("Items").Enabled().WithNoCheck();

            var expression = _context.Expressions.Single().ShouldBeOfType<AlterCheckConstraintExpression>();
            Assert.Multiple(() =>
            {
                expression.Action.ShouldBe(CheckConstraintAction.Enable);
                expression.Constraint.NoCheck.ShouldBeTrue();
            });
        }

        [Test]
        public void AlterCheckDisabledDisablesTheExistingConstraint()
        {
            IAlterExpressionRoot alter = new AlterExpressionRoot(_context);

            alter.Check("CK_Items_Quantity").OnTable("Items").Disabled();

            var expression = _context.Expressions.Single().ShouldBeOfType<AlterCheckConstraintExpression>();
            Assert.Multiple(() =>
            {
                expression.Action.ShouldBe(CheckConstraintAction.Disable);
                expression.Constraint.Enabled.ShouldBeFalse();
            });
        }

        [Test]
        public void DeleteCheckDropsTheNamedConstraintFromTheSpecifiedTable()
        {
            IDeleteExpressionRoot delete = new DeleteExpressionRoot(_context);

            delete.Check("CK_Items_Quantity").FromTable("Items").InSchema("inventory");

            var expression = _context.Expressions.Single().ShouldBeOfType<DeleteCheckConstraintExpression>();
            Assert.Multiple(() =>
            {
                expression.Constraint.Name.ShouldBe("CK_Items_Quantity");
                expression.Constraint.TableName.ShouldBe("Items");
                expression.Constraint.SchemaName.ShouldBe("inventory");
            });
        }

        [Test]
        public void CreateCheckExtensionsPassOptionsToTheExpression()
        {
            ICreateExpressionRoot create = new CreateExpressionRoot(_context);
            var options = create.Check("CK_Items_Quantity").OnTable("Items").Condition("Quantity > 0");

            options.ShouldBeAssignableTo<ISupportAdditionalFeatures>().SetAdditionalFeature("Example:NoInherit", true);

            var expression = _context.Expressions.Single().ShouldBeAssignableTo<ISupportAdditionalFeatures>();
            expression.GetAdditionalFeature<bool>("Example:NoInherit").ShouldBeTrue();
        }

        [Test]
        public void AlterCheckExtensionsPassOptionsToTheExpression()
        {
            IAlterExpressionRoot alter = new AlterExpressionRoot(_context);
            var options = alter.Check("CK_Items_Quantity").OnTable("Items").Condition("Quantity >= 0");

            options.ShouldBeAssignableTo<ISupportAdditionalFeatures>().SetAdditionalFeature("Example:NoInherit", true);

            var expression = _context.Expressions.Single().ShouldBeAssignableTo<ISupportAdditionalFeatures>();
            expression.GetAdditionalFeature<bool>("Example:NoInherit").ShouldBeTrue();
        }

        [Test]
        public void DeleteCheckExtensionsPassOptionsToTheExpression()
        {
            IDeleteExpressionRoot delete = new DeleteExpressionRoot(_context);
            var options = delete.Check("CK_Items_Quantity").FromTable("Items");

            options.ShouldBeAssignableTo<ISupportAdditionalFeatures>().SetAdditionalFeature("Example:Cascade", true);

            var expression = _context.Expressions.Single().ShouldBeAssignableTo<ISupportAdditionalFeatures>();
            expression.GetAdditionalFeature<bool>("Example:Cascade").ShouldBeTrue();
        }
    }
}
