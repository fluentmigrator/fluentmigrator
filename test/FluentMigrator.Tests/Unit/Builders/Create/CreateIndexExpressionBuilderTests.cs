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

using System;
using System.Collections.Generic;

using FluentMigrator.Builder.Create.Index;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.MySql;
using FluentMigrator.Postgres;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
    [TestFixture]
    [Category("Builder")]
    [Category("CreateIndex")]
    public class CreateIndexExpressionBuilderTests
    {
        [Test]
        public void CallingOnTableSetsTableNameToSpecifiedValue()
        {
            var indexMock = new Mock<IndexDefinition>();


            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
            builder.OnTable("Bacon");

            indexMock.VerifySet(x => x.TableName = "Bacon");
            expressionMock.VerifyGet(e => e.Index);
        }

        [Test]
        public void CallingOnColumnAddsNewColumnToExpression()
        {
            var collectionMock = new Mock<IList<IndexColumnDefinition>>();

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.Columns).Returns(collectionMock.Object);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
            builder.OnColumn("BaconId");

            collectionMock.Verify(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("BaconId"))));
            indexMock.VerifyGet(x => x.Columns);
            expressionMock.VerifyGet(e => e.Index);
        }

        [Test]
        public void CallingAscendingSetsDirectionToAscending()
        {
            var columnMock = new Mock<IndexColumnDefinition>();
            var expressionMock = new Mock<CreateIndexExpression>();

            var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
            builder.CurrentColumn = columnMock.Object;

            builder.Ascending();

            columnMock.VerifySet(c => c.Direction = Direction.Ascending);
        }

        [Test]
        public void CallingDescendingSetsDirectionToDescending()
        {
            var columnMock = new Mock<IndexColumnDefinition>();
            var expressionMock = new Mock<CreateIndexExpression>();

            var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
            builder.CurrentColumn = columnMock.Object;

            builder.Descending();

            columnMock.VerifySet(c => c.Direction = Direction.Descending);
        }

        [Test]
        public void CallingIncludeAddsNewIncludeToExpression()
        {
            var collectionMock = new Mock<IList<IndexIncludeDefinition>>();

            var additionalFeatures = new Dictionary<string, object>()
            {
                [SqlServer.SqlServerExtensions.IncludesList] = collectionMock.Object
            };

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);
            SqlServer.SqlServerExtensions.Include(builder, "BaconId");

            collectionMock.Verify(x => x.Add(It.Is<IndexIncludeDefinition>(c => c.Name.Equals("BaconId"))));
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);
        }

        [Test]
        public void CallingIncludeAddsNewIncludeToExpressionInPostgres()
        {
            var collectionMock = new Mock<IList<PostgresIndexIncludeDefinition>>();

            var additionalFeatures = new Dictionary<string, object>()
            {
                [PostgresExtensions.IncludesList] = collectionMock.Object
            };

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);
            PostgresExtensions.Include(builder, "BaconId");

            collectionMock.Verify(x => x.Add(It.Is<PostgresIndexIncludeDefinition>(c => c.Name.Equals("BaconId"))));
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);
        }

        [TestCase(Algorithm.Brin)]
        [TestCase(Algorithm.BTree)]
        [TestCase(Algorithm.Hash)]
        [TestCase(Algorithm.Gin)]
        [TestCase(Algorithm.Gist)]
        [TestCase(Algorithm.Spgist)]
        public void CallingUsingIndexAlgorithmToExpressionInPostgres(Algorithm algorithm)
        {
            var collectionMock = new Mock<PostgresIndexAlgorithmDefinition>();

            var additionalFeatures = new Dictionary<string, object>()
            {
                [PostgresExtensions.IndexAlgorithm] = collectionMock.Object
            };

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);

            switch (algorithm)
            {
                case Algorithm.BTree:
                    PostgresExtensions.UsingBTree(builder.WithOptions());
                    break;
                case Algorithm.Hash:
                    PostgresExtensions.UsingHash(builder.WithOptions());
                    break;
                case Algorithm.Gist:
                    builder.WithOptions().UsingGist();
                    break;
                case Algorithm.Spgist:
                    builder.WithOptions().UsingSpgist();
                    break;
                case Algorithm.Gin:
                    builder.WithOptions().UsingGin();
                    break;
                case Algorithm.Brin:
                    builder.WithOptions().UsingBrin();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
            }

            collectionMock.VerifySet(x => x.Algorithm = algorithm);
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);
        }

        [Test]
        public void CallingFilterExpressingInPostgres()
        {
            var additionalFeatures = new Dictionary<string, object>()
            {
                [PostgresExtensions.IndexFilter] = ""
            };

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);

            builder.WithOptions().Filter("someColumn = 'test'");
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);

            Assert.That(additionalFeatures[PostgresExtensions.IndexFilter], Is.EqualTo("someColumn = 'test'"));
        }

        [TestCase(arguments: true)]
        [TestCase(arguments: false)]
        [TestCase(arguments: null)]
        public void CallingUsingAsConcurrentlyToExpressionInPostgres(bool? isConcurrently)
        {
            var collectionMock = new Mock<PostgresIndexConcurrentlyDefinition>();

            var additionalFeatures = new Dictionary<string, object>()
            {
                [PostgresExtensions.Concurrently] = collectionMock.Object
            };


            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);
            if (isConcurrently == null)
            {
                builder.WithOptions().AsConcurrently();
            }
            else
            {
                builder.WithOptions().AsConcurrently(isConcurrently.Value);
            }

            collectionMock.VerifySet(x => x.IsConcurrently = isConcurrently ?? true);
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);
        }

        [TestCase(arguments: true)]
        [TestCase(arguments: false)]
        [TestCase(arguments: null)]
        public void CallingUsingAsOnlyToExpressionInPostgres(bool? isOnly)
        {
            var collectionMock = new Mock<PostgresIndexOnlyDefinition>();

            var additionalFeatures = new Dictionary<string, object>()
            {
                [PostgresExtensions.Only] = collectionMock.Object
            };

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);

            if (isOnly == null)
            {
                builder.WithOptions().AsOnly();
            }
            else
            {
                builder.WithOptions().AsOnly(isOnly.Value);
            }

            collectionMock.VerifySet(x => x.IsOnly = isOnly ?? true);
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);
        }

        [TestCase(NullSort.First)]
        [TestCase(NullSort.Last)]
        public void CallingNullsFirstOrLastToExpressionInPostgres(NullSort sort)
        {
            var collectionMock = new Mock<PostgresIndexNullsSort>();

            var additionalFeatures = new Dictionary<string, object>()
            {
                [PostgresExtensions.NullsSort] = collectionMock.Object
            };

            var indexMock = new Mock<IndexDefinition>();
            var indexCurrentColumnMock = new Mock<IndexColumnDefinition>();
            indexCurrentColumnMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            var builder = new CreateIndexExpressionBuilder(expressionMock.Object)
            {
                CurrentColumn = indexCurrentColumnMock.Object
            };


            switch (sort)
            {
                case NullSort.First:
                    builder.NullsFirst();
                    break;
                case NullSort.Last:
                    builder.NullsLast();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sort), sort, null);
            }

            collectionMock.VerifySet(x => x.Sort = sort);
            indexCurrentColumnMock.VerifyGet(x => x.AdditionalFeatures);
        }

        [TestCase(NullSort.First)]
        [TestCase(NullSort.Last)]
        public void CallingNullsToExpressionInPostgres(NullSort sort)
        {
            var collectionMock = new Mock<PostgresIndexNullsSort>();

            var additionalFeatures = new Dictionary<string, object>()
            {
                [PostgresExtensions.NullsSort] = collectionMock.Object
            };

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            var builder = new CreateIndexExpressionBuilder(expressionMock.Object);

            builder.Nulls(sort);

            collectionMock.VerifySet(x => x.Sort = sort);
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);
        }

        [Test]
        public void CallingWithFillfactorInPostgres()
        {
            var additionalFeatures = new Dictionary<string, object>();

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);

            builder.WithOptions().Fillfactor(90);
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);

            Assert.That(additionalFeatures[PostgresExtensions.IndexFillFactor], Is.EqualTo(90));
        }

        [Test]
        public void CallingWithVacuumCleanupIndexScaleFactorInPostgres()
        {
            var additionalFeatures = new Dictionary<string, object>();

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);

            PostgresExtensions.UsingBTree(builder.WithOptions()).VacuumCleanupIndexScaleFactor(90);
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);

            Assert.That(additionalFeatures[PostgresExtensions.IndexVacuumCleanupIndexScaleFactor], Is.EqualTo(90));
        }

        [TestCase(GistBuffering.Auto)]
        [TestCase(GistBuffering.On)]
        [TestCase(GistBuffering.Off)]
        public void CallingWithBufferingInPostgres(GistBuffering buffering)
        {
            var additionalFeatures = new Dictionary<string, object>();

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);

            builder.WithOptions().UsingGist().Buffering(buffering);
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);

            Assert.That(additionalFeatures[PostgresExtensions.IndexBuffering], Is.EqualTo(buffering));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CallingWithFastUpdateInPostgres(bool fastUpdate)
        {
            var additionalFeatures = new Dictionary<string, object>();

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);

            builder.WithOptions().UsingGin().FastUpdate(fastUpdate);
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);

            Assert.That(additionalFeatures[PostgresExtensions.IndexFastUpdate], Is.EqualTo(fastUpdate));
        }

        [Test]
        public void CallingWithGinPendingListLimitInPostgres()
        {
            var additionalFeatures = new Dictionary<string, object>();

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);

            builder.WithOptions().UsingGin().PendingListLimit(90);
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);

            Assert.That(additionalFeatures[PostgresExtensions.IndexGinPendingListLimit], Is.EqualTo(90));
        }

        [Test]
        public void CallingWithPagesPerRangeInPostgres()
        {
            var additionalFeatures = new Dictionary<string, object>();

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);

            builder.WithOptions().UsingBrin().PagesPerRange(90);
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);

            Assert.That(additionalFeatures[PostgresExtensions.IndexPagesPerRange], Is.EqualTo(90));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CallingWithAutosummarizeInPostgres(bool autosummarize)
        {
            var additionalFeatures = new Dictionary<string, object>();

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);

            builder.WithOptions().UsingBrin().Autosummarize(autosummarize);
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);

            Assert.That(additionalFeatures[PostgresExtensions.IndexAutosummarize], Is.EqualTo(autosummarize));
        }

        [Test]
        public void CallingTablespaceExpressingInPostgres()
        {
            var additionalFeatures = new Dictionary<string, object>()
            {
                [PostgresExtensions.IndexTablespace] = ""
            };

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);

            builder.WithOptions().Tablespace("indexspace");
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);

            Assert.That(additionalFeatures[PostgresExtensions.IndexTablespace], Is.EqualTo("indexspace"));
        }

        [TestCase(IndexType.BTree)]
        [TestCase(IndexType.Hash)]
        public void CallingUsingIndexTypeToExpressionInMySql(IndexType indexType)
        {
            var collectionMock = new Mock<MySqlIndexTypeDefinition>();

            var additionalFeatures = new Dictionary<string, object>()
            {
                [MySqlExtensions.IndexType] = collectionMock.Object
            };

            var indexMock = new Mock<IndexDefinition>();
            indexMock.Setup(x => x.AdditionalFeatures).Returns(additionalFeatures);

            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            ICreateIndexOnColumnOrInSchemaSyntax builder = new CreateIndexExpressionBuilder(expressionMock.Object);

            switch (indexType)
            {
                case IndexType.BTree:
                    MySqlExtensions.UsingBTree(builder.WithOptions());
                    break;
                case IndexType.Hash:
                    MySqlExtensions.UsingHash(builder.WithOptions());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(indexType), indexType, null);
            }

            collectionMock.VerifySet(x => x.IndexType = indexType);
            indexMock.VerifyGet(x => x.AdditionalFeatures);
            expressionMock.VerifyGet(e => e.Index);
        }
    }
}
