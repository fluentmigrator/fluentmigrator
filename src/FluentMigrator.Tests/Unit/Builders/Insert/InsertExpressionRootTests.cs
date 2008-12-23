using System.Collections.Generic;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Moq;
using Xunit;

namespace FluentMigrator.Tests.Unit.Builders.Insert
{
    public class InsertExpressionRootTests
    {
        [Fact]
        public void CallingIntoTableSetsTableName()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Expect(x => x.Add(It.Is<InsertDataExpression>(e => e.TableName.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new InsertExpressionRoot(contextMock.Object);
            root.IntoTable("Bacon");			

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
        }
    }
}