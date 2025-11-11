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

namespace FluentMigrator.Tests.Unit.Versioning
{
    [TestFixture]
    [Category("Versioning")]
    [Category("Issue2051")]
    public class VersionUniqueMigrationTests
    {
        [Test]
        public void UpShouldCreateClusteredUniqueIndexWhenCreateWithPrimaryKeyIsFalse()
        {
            // Arrange
            var versionTableMetaData = new TestVersionTableMetaData { CreateWithPrimaryKey = false };
            var migration = new VersionUniqueMigration(versionTableMetaData);
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = CreateMigrationContext(collectionMock);

            // Act
            migration.GetUpExpressions(contextMock.Object);

            // Assert
            // Should create clustered unique index
            collectionMock.Verify(x => x.Add(It.Is<CreateIndexExpression>(e => 
                e.Index.Name == versionTableMetaData.UniqueIndexName &&
                e.Index.IsUnique &&
                e.Index.IsClustered)), Times.Once);
            
            // Should add AppliedOn column
            collectionMock.Verify(x => x.Add(It.Is<CreateColumnExpression>(e => 
                e.Column.Name == versionTableMetaData.AppliedOnColumnName)), Times.Once);
            
            collectionMock.Verify(x => x.Add(It.IsAny<IMigrationExpression>()), Times.Exactly(3));
        }

        [Test]
        public void UpShouldCreateNonClusteredUniqueIndexWhenCreateWithPrimaryKeyIsTrue()
        {
            // Arrange
            var versionTableMetaData = new TestVersionTableMetaData { CreateWithPrimaryKey = true };
            var migration = new VersionUniqueMigration(versionTableMetaData);
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = CreateMigrationContext(collectionMock);

            // Act
            migration.GetUpExpressions(contextMock.Object);

            // Assert
            // Should create non-clustered unique index
            collectionMock.Verify(x => x.Add(It.Is<CreateIndexExpression>(e => 
                e.Index.Name == versionTableMetaData.UniqueIndexName &&
                e.Index.IsUnique &&
                !e.Index.IsClustered)), Times.Once);
            
            // Should add AppliedOn column
            collectionMock.Verify(x => x.Add(It.Is<CreateColumnExpression>(e => 
                e.Column.Name == versionTableMetaData.AppliedOnColumnName)), Times.Once);
            
            collectionMock.Verify(x => x.Add(It.IsAny<IMigrationExpression>()), Times.Exactly(3));
        }

        private Mock<IMigrationContext> CreateMigrationContext(Mock<ICollection<IMigrationExpression>> collectionMock)
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider(validateScopes: false);

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);
            contextMock.Setup(x => x.ServiceProvider).Returns(serviceProvider);
            
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