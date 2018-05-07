#region License
// 
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Alter.Column;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Alter
{
    [TestFixture]
    public class AlterExpressionRootTests
    {
        [Test]
        public void CallingTableAddsAlterTableExpressionToContextWithSpecifiedNameSet()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();


            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);


            var root = new AlterExpressionRoot(contextMock.Object);
            root.Table("Bacon");

            collectionMock.Verify(x => x.Add(It.Is<AlterTableExpression>(e => e.TableName.Equals("Bacon"))));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingTableReturnsAlterTableExpressionBuilder()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var root = new AlterExpressionRoot(contextMock.Object);
            var builder = root.Table("Bacon");

            builder.ShouldBeOfType<AlterTableExpressionBuilder>();

            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingColumnAddsAlterColumnExpressionToContextWithSpecifiedNameSet()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();


            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);


            var root = new AlterExpressionRoot(contextMock.Object);
            root.Column("Bacon");

            collectionMock.Verify(x => x.Add(It.Is<AlterColumnExpression>(e => e.Column.Name.Equals("Bacon"))));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingColumnReturnsAlterColumnExpression()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);


            var root = new AlterExpressionRoot(contextMock.Object);
            var builder = root.Column("Bacon");

            builder.ShouldBeOfType<AlterColumnExpressionBuilder>();
            contextMock.VerifyGet(x => x.Expressions);
        }
    }
}