#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using System.Data;
using FluentMigrator.Builders.Create;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
    [TestFixture]
    [Category("Builder")]
    [Category("CreateTable")]
    [Category("MultiColumnForeignKey")]
    public class CreateTableWithMultiColumnForeignKeyTests
    {
        [Test]
        public void CanCreateTableWithMultiColumnForeignKeyUsingFluentSyntax()
        {
            var expressions = new List<IMigrationExpression>();
            var migrationContext = new Mock<IMigrationContext>();
            migrationContext.SetupGet(mc => mc.Expressions).Returns(expressions);

            var root = new CreateExpressionRoot(migrationContext.Object);

            // Create table with multi-column foreign key using the new fluent syntax
            root.Table("Area")
                .WithColumn("ArticleId").AsString().NotNullable()
                .WithColumn("AreaGroupIndex").AsInt32().NotNullable()
                .WithColumn("Index").AsInt32().NotNullable()
                .ForeignKey(new[] { "ArticleId", "AreaGroupIndex" }, "AreaGroup", new[] { "ArticleId", "Index" })
                .OnDelete(Rule.Cascade);

            expressions.Count.ShouldBe(2); // CreateTable + CreateForeignKey
            
            var createTableExpression = expressions[0] as CreateTableExpression;
            createTableExpression.ShouldNotBeNull();
            createTableExpression.TableName.ShouldBe("Area");
            createTableExpression.Columns.Count.ShouldBe(3);

            var createForeignKeyExpression = expressions[1] as CreateForeignKeyExpression;
            createForeignKeyExpression.ShouldNotBeNull();
            createForeignKeyExpression.ForeignKey.ForeignTable.ShouldBe("Area");
            createForeignKeyExpression.ForeignKey.PrimaryTable.ShouldBe("AreaGroup");
            createForeignKeyExpression.ForeignKey.ForeignColumns.Count.ShouldBe(2);
            createForeignKeyExpression.ForeignKey.PrimaryColumns.Count.ShouldBe(2);
            createForeignKeyExpression.ForeignKey.ForeignColumns.ShouldContain("ArticleId");
            createForeignKeyExpression.ForeignKey.ForeignColumns.ShouldContain("AreaGroupIndex");
            createForeignKeyExpression.ForeignKey.PrimaryColumns.ShouldContain("ArticleId");
            createForeignKeyExpression.ForeignKey.PrimaryColumns.ShouldContain("Index");
            createForeignKeyExpression.ForeignKey.OnDelete.ShouldBe(Rule.Cascade);
        }

        [Test]
        public void CanCreateTableWithNamedMultiColumnForeignKeyUsingFluentSyntax()
        {
            var expressions = new List<IMigrationExpression>();
            var migrationContext = new Mock<IMigrationContext>();
            migrationContext.SetupGet(mc => mc.Expressions).Returns(expressions);

            var root = new CreateExpressionRoot(migrationContext.Object);

            // Create table with named multi-column foreign key using the new fluent syntax
            root.Table("Area")
                .WithColumn("ArticleId").AsString().NotNullable()
                .WithColumn("AreaGroupIndex").AsInt32().NotNullable()
                .WithColumn("Index").AsInt32().NotNullable()
                .ForeignKey("FK_Area_AreaGroup", new[] { "ArticleId", "AreaGroupIndex" }, "AreaGroup", new[] { "ArticleId", "Index" })
                .OnDelete(Rule.Cascade);

            expressions.Count.ShouldBe(2); // CreateTable + CreateForeignKey
            
            var createForeignKeyExpression = expressions[1] as CreateForeignKeyExpression;
            createForeignKeyExpression.ShouldNotBeNull();
            createForeignKeyExpression.ForeignKey.Name.ShouldBe("FK_Area_AreaGroup");
        }

        [Test]
        public void CanCreateTableWithMultiColumnForeignKeyWithSchemaUsingFluentSyntax()
        {
            var expressions = new List<IMigrationExpression>();
            var migrationContext = new Mock<IMigrationContext>();
            migrationContext.SetupGet(mc => mc.Expressions).Returns(expressions);

            var root = new CreateExpressionRoot(migrationContext.Object);

            // Create table with multi-column foreign key with schema using the new fluent syntax
            root.Table("Area")
                .WithColumn("ArticleId").AsString().NotNullable()
                .WithColumn("AreaGroupIndex").AsInt32().NotNullable()
                .WithColumn("Index").AsInt32().NotNullable()
                .ForeignKey("FK_Area_AreaGroup", "dbo", new[] { "ArticleId", "AreaGroupIndex" }, "AreaGroup", new[] { "ArticleId", "Index" })
                .OnDelete(Rule.Cascade);

            expressions.Count.ShouldBe(2); // CreateTable + CreateForeignKey
            
            var createForeignKeyExpression = expressions[1] as CreateForeignKeyExpression;
            createForeignKeyExpression.ShouldNotBeNull();
            createForeignKeyExpression.ForeignKey.PrimaryTableSchema.ShouldBe("dbo");
        }
    }
}