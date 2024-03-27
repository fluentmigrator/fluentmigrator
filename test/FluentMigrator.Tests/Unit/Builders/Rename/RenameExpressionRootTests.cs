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
using FluentMigrator.Builders.Rename;
using FluentMigrator.Builders.Rename.Column;
using FluentMigrator.Builders.Rename.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Rename
{
    [TestFixture]
    [Category("Builder")]
    [Category("RootRename")]
    public class RenameExpressionRootTests
    {
        [Test]
        public void CallingTableAddsRenameTableExpressionToContextWithSpecifiedOldName()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object);

            var root = new RenameExpressionRoot(contextMock.Object);
            root.Table("Bacon");

            collectionMock.Verify(x => x.Add(It.Is<RenameTableExpression>(e => e.OldName.Equals("Bacon"))));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingTableReturnsRenameTableExpressionBuilder()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object);

            var root = new RenameExpressionRoot(contextMock.Object);
            var builder = root.Table("Bacon");

            builder.ShouldBeOfType<RenameTableExpressionBuilder>();
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingColumnAddsRenameColumnExpressionToContextWithSpecifiedOldName()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object);

            var root = new RenameExpressionRoot(contextMock.Object);
            root.Column("Bacon");

            collectionMock.Verify(x => x.Add(It.Is<RenameColumnExpression>(e => e.OldName.Equals("Bacon"))));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingColumnReturnsRenameColumnExpressionBuilder()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var contextMock = new Mock<IMigrationContext>();
            contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object);

            var root = new RenameExpressionRoot(contextMock.Object);
            var builder = root.Column("Bacon");

            builder.ShouldBeOfType<RenameColumnExpressionBuilder>();
            contextMock.VerifyGet(x => x.Expressions);
        }
    }
}