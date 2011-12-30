using System.Collections.Generic;
using NUnit.Framework;
using FluentMigrator.Expressions;
using Moq;
using FluentMigrator.Infrastructure;
using FluentMigrator.Builders.Delete;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
    [TestFixture]
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

            collectionMock.Verify(x => x.Add(It.Is<DeleteConstraintExpression>(e => e.Constraint.IsPrimaryKeyConstraint == true)));
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

            collectionMock.Verify(x => x.Add(It.Is<DeleteConstraintExpression>(e => e.Constraint.IsUniqueConstraint == true)));
            collectionMock.Verify(x => x.Add(It.Is<DeleteConstraintExpression>(e => e.Constraint.ConstraintName == "TestUniqueConstraintName")));
            contextMock.VerifyGet(x => x.Expressions);
        }
    }
}
