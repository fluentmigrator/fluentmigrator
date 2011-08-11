#region License
// 
// Copyright (c) 2011, Grant Archibald
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

using FluentMigrator.Builders.Execute;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Jet;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Builders.Execute
{
    [TestFixture]
    public class ExecuteExpressionRootTests
    {
        [Test]
        public void NullExpressionIfDatabaseTypeNotMatch()
        {
            // Arrange
            var context = new Mock<IMigrationContext>();
            var expression = new ExecuteExpressionRoot(context.Object);

            // Act
            var newExpression = expression.WithDatabaseType(DatabaseType.Unknown);

            // Assert
            newExpression.GetType().ShouldBe(typeof (NullExecuteExpression));
        }

        [Test]
        public void ExpressionIfDatabaseTypeJet()
        {
            // Arrange
            var context = new Mock<IMigrationContext>();
            var expression = new ExecuteExpressionRoot(context.Object);

            context.Setup(m => m.QuerySchema).Returns(new JetProcessor(null, null, new NullAnnouncer(), new ProcessorOptions()));

            // Act
            var newExpression = expression.WithDatabaseType(DatabaseType.Jet);

            // Assert
            newExpression.ShouldBe(expression);
        }

        [Test]
        public void ExpressionIfDatabaseTypeJetOrSqlServer()
        {
            // Arrange
            var context = new Mock<IMigrationContext>();
            var expression = new ExecuteExpressionRoot(context.Object);

            context.Setup(m => m.QuerySchema).Returns(new JetProcessor(null, null, new NullAnnouncer(), new ProcessorOptions()));

            // Act
            var newExpression = expression.WithDatabaseType(DatabaseType.Jet | DatabaseType.SqlServer);

            // Assert
            newExpression.ShouldBe(expression);
        }
    }
}
