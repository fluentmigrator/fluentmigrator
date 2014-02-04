#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

using System;
using System.Linq;
using FluentMigrator.Builders.InDatabase;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.InDatabase
{
    [TestFixture]
    public class InDatabaseExpressionRootTests
    {
        [Test]
        public void WillAssignCorrectDatabaseKeyToExpression()
        {
            var mock = new Mock<IMultiDatabaseMigrationProcessor>();
            mock.Setup(x => x.HasDatabaseKey(It.IsAny<string>()))
                .Returns(true);

            var context = ExecuteTestMigration(mock.Object, "TestKey");

            var expression = context.Expressions.Single();
            mock.Verify(m => m.AssignDatabaseKey(expression, "TestKey"));
        }

        [Test]
        public void WillThrowArgumentExceptionIfDatabaseKeyIsNotFoundInProcessors() {
            var mock = new Mock<IMultiDatabaseMigrationProcessor>();
            mock.Setup(x => x.HasDatabaseKey("TestKey"))
                .Returns(false);

            Assert.Throws<ArgumentException>(() => ExecuteTestMigration(mock.Object, "TestKey"));
        }
        
        private MigrationContext ExecuteTestMigration(IMultiDatabaseMigrationProcessor processor, string databaseKey = null)
        {
            var context = new MigrationContext(new MigrationConventions(), processor, GetType().Assembly, null, "");
            var expression = new InDatabaseExpressionRoot(context, databaseKey);

            expression.Create.Table("Foo").WithColumn("Id").AsInt16();

            return context;
        }
    }

}
