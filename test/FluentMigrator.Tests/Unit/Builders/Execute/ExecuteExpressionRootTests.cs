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
using System.Data;
using System.Linq;

using FluentMigrator.Builders.Execute;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Execute
{
    [TestFixture]
    [Category("Execute")]
    [Category("ExecuteExpressionRoot")]
    public class ExecuteExpressionRootTests
    {
        private Mock<IMigrationContext> _contextMock;
        private List<IMigrationExpression> _expressions;
        private ExecuteExpressionRoot _root;

        [SetUp]
        public void SetUp()
        {
            _expressions = new List<IMigrationExpression>();
            _contextMock = new Mock<IMigrationContext>();
            _contextMock.SetupGet(x => x.Expressions).Returns(_expressions);
            _root = new ExecuteExpressionRoot(_contextMock.Object);
        }

        [Test]
        public void WithConnectionCreatesPerformDBOperationExpression()
        {
            // Arrange
            void TestOperation(IDbConnection connection, IDbTransaction transaction) { }

            // Act
            _root.WithConnection(TestOperation);

            // Assert
            _expressions.Count.ShouldBe(1);
            var expression = _expressions.First() as PerformDBOperationExpression;
            expression.ShouldNotBeNull();
            expression.Operation.ShouldBe(TestOperation);
            expression.Description.ShouldBeNull();
        }

        [Test]
        public void WithConnectionWithDescriptionCreatesPerformDBOperationExpressionWithDescription()
        {
            // Arrange
            void TestOperation(IDbConnection connection, IDbTransaction transaction) { }
            const string testDescription = "Test database operation";

            // Act
            _root.WithConnection(TestOperation, testDescription);

            // Assert
            _expressions.Count.ShouldBe(1);
            var expression = _expressions.First() as PerformDBOperationExpression;
            expression.ShouldNotBeNull();
            expression.Operation.ShouldBe(TestOperation);
            expression.Description.ShouldBe(testDescription);
        }

        [Test]
        public void WithConnectionWithNullDescriptionCreatesPerformDBOperationExpressionWithNullDescription()
        {
            // Arrange
            void TestOperation(IDbConnection connection, IDbTransaction transaction) { }

            // Act
            _root.WithConnection(TestOperation, null);

            // Assert
            _expressions.Count.ShouldBe(1);
            var expression = _expressions.First() as PerformDBOperationExpression;
            expression.ShouldNotBeNull();
            expression.Operation.ShouldBe(TestOperation);
            expression.Description.ShouldBeNull();
        }

        [Test]
        public void WithConnectionWithEmptyDescriptionCreatesPerformDBOperationExpressionWithEmptyDescription()
        {
            // Arrange
            void TestOperation(IDbConnection connection, IDbTransaction transaction) { }
            const string emptyDescription = "";

            // Act
            _root.WithConnection(TestOperation, emptyDescription);

            // Assert
            _expressions.Count.ShouldBe(1);
            var expression = _expressions.First() as PerformDBOperationExpression;
            expression.ShouldNotBeNull();
            expression.Operation.ShouldBe(TestOperation);
            expression.Description.ShouldBe(emptyDescription);
        }
        [Test]
        public void SqlResolvesSqlScriptTokenProvidersFromServiceProvider()
        {
            var tokenProvider = new Mock<ISqlScriptTokenProvider>();
            var providers = new List<ISqlScriptTokenProvider> { tokenProvider.Object };

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IEnumerable<ISqlScriptTokenProvider>)))
                .Returns(providers);
            _contextMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            _root.Sql("INSERT INTO BLAH");

            _expressions.Count.ShouldBe(1);
            var expression = _expressions.First() as ExecuteSqlStatementExpression;
            expression.ShouldNotBeNull();
            expression.SqlScriptTokenProviders.ShouldBe(providers);
        }

        [Test]
        public void ScriptResolvesSqlScriptTokenProvidersFromServiceProvider()
        {
            var tokenProvider = new Mock<ISqlScriptTokenProvider>();
            var providers = new List<ISqlScriptTokenProvider> { tokenProvider.Object };

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IEnumerable<ISqlScriptTokenProvider>)))
                .Returns(providers);
            _contextMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            _root.Script("script.sql");

            _expressions.Count.ShouldBe(1);
            var expression = _expressions.First() as ExecuteSqlScriptExpression;
            expression.ShouldNotBeNull();
            expression.SqlScriptTokenProviders.ShouldBe(providers);
        }

        [Test]
        public void EmbeddedScriptResolvesSqlScriptTokenProvidersFromServiceProvider()
        {
            var tokenProvider = new Mock<ISqlScriptTokenProvider>();
            var providers = new List<ISqlScriptTokenProvider> { tokenProvider.Object };
            var embeddedResourceProviders = new List<IEmbeddedResourceProvider>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IEnumerable<ISqlScriptTokenProvider>)))
                .Returns(providers);
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IEnumerable<IEmbeddedResourceProvider>)))
                .Returns(embeddedResourceProviders);
            _contextMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            _root.EmbeddedScript("script.sql");

            _expressions.Count.ShouldBe(1);
            var expression = _expressions.First() as ExecuteEmbeddedSqlScriptExpression;
            expression.ShouldNotBeNull();
            expression.SqlScriptTokenProviders.ShouldBe(providers);
        }
    }
}