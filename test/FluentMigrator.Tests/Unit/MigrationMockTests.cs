#region License
// Copyright (c) 2019, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit
{
    public class MigrationMockTests
    {
        [Test]
        public void TestMigrationWithMockedContext()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider(validateScopes: false);

            var contextMock = new Mock<IMigrationContext>();
            contextMock.SetupGet(x => x.Expressions).Returns(new List<IMigrationExpression>());
            contextMock.SetupGet(x => x.ServiceProvider).Returns(serviceProvider);

            var sut = new TestMigration();
            var exception = Assert.Throws<InvalidOperationException>(() => sut.GetUpExpressions(contextMock.Object));
            Assert.That(exception.Message, Is.EqualTo("Something went wrong!"));
        }

        private class TestMigration : ForwardOnlyMigration
        {
            public override void Up()
            {
                throw new InvalidOperationException("Something went wrong!");
            }
        }
    }
}
