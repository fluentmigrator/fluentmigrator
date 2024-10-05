#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using NUnit.Framework;
using FluentMigrator.Expressions;
using Moq;
using FluentMigrator.Infrastructure;
using FluentMigrator.Builders.Delete;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
    [TestFixture]
    [Category("Builder")]
    [Category("DeleteConstraint")]
    public class DeleteConstraintTest
    {
        [Test]
        public void CallingDeletePrimaryKeyCreatesADeleteConstraintExpression()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new DeleteExpressionRoot(contextMock.Object);
            root.PrimaryKey("TestKey");

            collectionMock.Verify(x => x.Add(It.Is<DeleteConstraintExpression>(e => e.Constraint.IsPrimaryKeyConstraint)));
            collectionMock.Verify(x => x.Add(It.Is<DeleteConstraintExpression>(e => e.Constraint.ConstraintName == "TestKey")));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingDeleteUniqueConstraintCreatesADeleteConstraintExpression()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new DeleteExpressionRoot(contextMock.Object);
            root.UniqueConstraint("TestUniqueConstraintName");

            collectionMock.Verify(x => x.Add(It.Is<DeleteConstraintExpression>(e => e.Constraint.IsUniqueConstraint)));
            collectionMock.Verify(x => x.Add(It.Is<DeleteConstraintExpression>(e => e.Constraint.ConstraintName == "TestUniqueConstraintName")));
            contextMock.VerifyGet(x => x.Expressions);
        }
    }
}
