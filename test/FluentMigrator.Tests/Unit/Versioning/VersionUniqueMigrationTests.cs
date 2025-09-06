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
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace FluentMigrator.Tests.Unit.Versioning
{
    [TestFixture]
    [Category("Versioning")]
    [Category("Issue2051")]
    public class VersionUniqueMigrationTests
    {
        [Test]
        public void UpShouldCreateUniqueIndexWhenCreateWithPrimaryKeyIsFalse()
        {
            // Arrange
            var versionTableMetaData = new TestVersionTableMetaData { CreateWithPrimaryKey = false };
            var migration = new VersionUniqueMigration(versionTableMetaData);
            var contextMock = CreateMigrationContext();

            // Act
            migration.GetUpExpressions(contextMock.Object);

            // Assert
            var expressions = contextMock.Object.Expressions;
            expressions.Count.ShouldBe(2);
            
            // Should create unique index
            var createIndexExpression = expressions[0] as CreateIndexExpression;
            createIndexExpression.ShouldNotBeNull();
            createIndexExpression.Index.Name.ShouldBe(versionTableMetaData.UniqueIndexName);
            createIndexExpression.Index.IsUnique.ShouldBeTrue();
            createIndexExpression.Index.IsClustered.ShouldBeTrue();
            
            // Should add AppliedOn column
            var alterTableExpression = expressions[1] as AlterTableExpression;
            alterTableExpression.ShouldNotBeNull();
            alterTableExpression.Columns.Count.ShouldBe(1);
            alterTableExpression.Columns[0].Name.ShouldBe(versionTableMetaData.AppliedOnColumnName);
        }

        [Test]
        public void UpShouldNotCreateUniqueIndexWhenCreateWithPrimaryKeyIsTrue()
        {
            // Arrange
            var versionTableMetaData = new TestVersionTableMetaData { CreateWithPrimaryKey = true };
            var migration = new VersionUniqueMigration(versionTableMetaData);
            var contextMock = CreateMigrationContext();

            // Act
            migration.GetUpExpressions(contextMock.Object);

            // Assert
            var expressions = contextMock.Object.Expressions;
            expressions.Count.ShouldBe(1);
            
            // Should only add AppliedOn column, no index creation
            var alterTableExpression = expressions[0] as AlterTableExpression;
            alterTableExpression.ShouldNotBeNull();
            alterTableExpression.Columns.Count.ShouldBe(1);
            alterTableExpression.Columns[0].Name.ShouldBe(versionTableMetaData.AppliedOnColumnName);
        }

        private Mock<IMigrationContext> CreateMigrationContext()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider(validateScopes: false);

            var contextMock = new Mock<IMigrationContext>();
            contextMock.SetupGet(x => x.Expressions).Returns(new List<IMigrationExpression>());
            contextMock.SetupGet(x => x.ServiceProvider).Returns(serviceProvider);
            
            return contextMock;
        }

        private class TestVersionTableMetaData : IVersionTableMetaData
        {
            public bool OwnsSchema => false;
            public string SchemaName => "dbo";
            public string TableName => "VersionInfo";
            public string ColumnName => "Version";
            public string DescriptionColumnName => "Description";
            public string UniqueIndexName => "UC_Version";
            public string AppliedOnColumnName => "AppliedOn";
            public bool CreateWithPrimaryKey { get; set; }
        }
    }
}