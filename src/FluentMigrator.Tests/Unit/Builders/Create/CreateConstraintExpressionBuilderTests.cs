using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Expressions;
using NUnit.Should;
using FluentMigrator.Model;
using FluentMigrator.Builders.Create.Constraint;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
    [TestFixture]
    public class CreateConstraintExpressionBuilderTests
    {

        [Test]
        public void SettingTheTableNameSetsTheTableName()
        {
            CreateConstraintExpression expression = new CreateConstraintExpression(ConstraintType.Unique);

            CreateConstraintExpressionBuilder builder = new CreateConstraintExpressionBuilder(expression);
            builder.OnTable("FOO");

            expression.Constraint.TableName.ShouldBe("FOO");
        }

        [Test]
        public void AddingASingleColumnShouldAddItToToColumnList()
        {
            CreateConstraintExpression expression = new CreateConstraintExpression(ConstraintType.Unique);

            CreateConstraintExpressionBuilder builder = new CreateConstraintExpressionBuilder(expression);
            builder.OnTable("FOO").Column("BAR");

            expression.Constraint.Columns.First().ShouldBe("BAR");

        }

        [Test]
        public void AddingMultipleColumnShouldAddThenToToColumnList()
        {
            CreateConstraintExpression expression = new CreateConstraintExpression(ConstraintType.Unique);

            CreateConstraintExpressionBuilder builder = new CreateConstraintExpressionBuilder(expression);
            builder.OnTable("FOO").Columns(new string[]{"BAR","BAZ" });

            expression.Constraint.Columns.First().ShouldBe("BAR");
            expression.Constraint.Columns.ElementAt(1).ShouldBe("BAZ");

        }

        [Test]
        public void ATableShouldBeAllowedToSpecifyASchema()
        {
            CreateConstraintExpression expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);

            CreateConstraintExpressionBuilder builder = new CreateConstraintExpressionBuilder(expression);
            builder.OnTable("FOO").WithSchema("BAR").Column("BAZ");

            expression.Constraint.SchemaName.ShouldBe("BAR");
            expression.Constraint.TableName.ShouldBe("FOO");
            expression.Constraint.Columns.First().ShouldBe("BAZ");

        }


    }
}
