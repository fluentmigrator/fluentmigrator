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
using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Insert
{
    [TestFixture]
    [Category("Builder")]
    [Category("RootInsert")]
    public class InsertExpressionRootTests
    {
        [Test]
        public void CallingIntoTableSetsTableName()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new InsertExpressionRoot(contextMock.Object);
            root.IntoTable("Bacon");

            collectionMock.Verify(x => x.Add(It.Is<InsertDataExpression>(e => e.TableName.Equals("Bacon"))));
            contextMock.VerifyGet(x => x.Expressions);
        }
    }
}