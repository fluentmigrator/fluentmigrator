using FluentMigrator.Builders;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace FluentMigrator.Tests.Unit.Builders
{
    [TestFixture]
    public class ColumnExpressionBuilderHelperTests
    {
        [Test]
        public void SetNotNullable_SetsColumnIfNotSettingExistingRowValues()
        {
            var builderMock = new Mock<IColumnExpressionBuilder>();
            var contextMock = new Mock<IMigrationContext>();
            var columnMock = new Mock<ColumnDefinition>();
            builderMock.SetupGet(n => n.Column).Returns(columnMock.Object);
            columnMock.SetupGet(n => n.ModificationType).Returns(ColumnModificationType.Create);

            var helper = new ColumnExpressionBuilderHelper(builderMock.Object, contextMock.Object);
            helper.SetNullable(false);

            columnMock.VerifySet(n => n.IsNullable = false);
        }

        [Test]
        public void SetNotNullable_DoesntSetIfSettingExistingRowValues()
        {
            var builderMock = new Mock<IColumnExpressionBuilder>();
            var contextMock = new Mock<IMigrationContext>();
            var columnMock = new Mock<ColumnDefinition>();
            var columnCloneMock = new Mock<ColumnDefinition>();
            builderMock.SetupGet(n => n.Column).Returns(columnMock.Object);
            columnMock.SetupGet(n => n.ModificationType).Returns(ColumnModificationType.Create);
            columnMock.Setup(n => n.Clone()).Returns(columnCloneMock.Object);
            contextMock.Setup(n => n.Expressions.Add(It.IsAny<IMigrationExpression>()));

            var helper = new ColumnExpressionBuilderHelper(builderMock.Object, contextMock.Object);

            helper.SetExistingRowsTo("test");
            helper.SetNullable(false);

            //The column IsNullable should NOT be set to false, instead an internal clone
            //should have been created, which will have IsNullable set to false.
            columnMock.VerifySet(n => n.IsNullable = false, Times.Never());
            columnCloneMock.VerifySet(n => n.IsNullable = false, Times.Once());
        }

        [Test]
        public void SetExistingRows_AddsAllRowsExpression()
        {
            var builderMock = new Mock<IColumnExpressionBuilder>();
            var contextMock = new Mock<IMigrationContext>();
            IMigrationExpression addedExpression = null;
            contextMock
                .Setup(n => n.Expressions.Add(It.IsAny<IMigrationExpression>()))
                .Callback((IMigrationExpression ex) => addedExpression = ex);

            builderMock.SetupGet(n => n.SchemaName).Returns("Fred");
            builderMock.SetupGet(n => n.TableName).Returns("Flinstone");
            builderMock.SetupGet(n => n.Column.Name).Returns("ColName");
            builderMock.SetupGet(n => n.Column.ModificationType).Returns(ColumnModificationType.Create);

            var helper = new ColumnExpressionBuilderHelper(builderMock.Object, contextMock.Object);

            helper.SetExistingRowsTo(5);

            contextMock.Verify(n => n.Expressions.Add(It.IsAny<IMigrationExpression>()), Times.Once());

            //Check that the update data expression was added as expected.  Maybe there's a cleaner way to do this?
            Assert.IsInstanceOf<UpdateDataExpression>(addedExpression);
            UpdateDataExpression updateDataExpr = (UpdateDataExpression)addedExpression;
            Assert.IsNotNull(updateDataExpr);
            Assert.AreEqual("Fred", updateDataExpr.SchemaName);
            Assert.AreEqual("Flinstone", updateDataExpr.TableName);
            Assert.AreEqual(true, updateDataExpr.IsAllRows);
            Assert.AreEqual(1, updateDataExpr.Set.Count);
            Assert.AreEqual("ColName", updateDataExpr.Set[0].Key);
            Assert.AreEqual(5, updateDataExpr.Set[0].Value);
        }

        [Test]
        public void SetExistingRows_IgnoredIfAlterColumn()
        {
            var builderMock = new Mock<IColumnExpressionBuilder>();
            var contextMock = new Mock<IMigrationContext>();
            builderMock.SetupGet(n => n.Column.ModificationType).Returns(ColumnModificationType.Alter);
            contextMock.Setup(n => n.Expressions.Add(It.IsAny<IMigrationExpression>()));

            var helper = new ColumnExpressionBuilderHelper(builderMock.Object, contextMock.Object);

            helper.SetExistingRowsTo("test");

            contextMock.Verify(n => n.Expressions.Add(It.IsAny<IMigrationExpression>()), Times.Never());
        }

        [Test]
        public void SetExistingRows_AfterNotNullableAddsAlterColumnExpression()
        {
            var builderMock = new Mock<IColumnExpressionBuilder>();
            var contextMock = new Mock<IMigrationContext>();
            List<IMigrationExpression> addedExpressions = new List<IMigrationExpression>();
            contextMock.SetupGet(n => n.Expressions).Returns(addedExpressions);

            builderMock.SetupGet(n => n.SchemaName).Returns("Fred");
            builderMock.SetupGet(n => n.TableName).Returns("Flinstone");

            var createColColumn = new ColumnDefinition
            {
                ModificationType = ColumnModificationType.Create,
                Name = "ColName",
                Type = System.Data.DbType.String,
                CustomType = "CustomType",
                Size = 12,
                Precision = 2
            };

            builderMock.SetupGet(n => n.Column).Returns(createColColumn);

            var helper = new ColumnExpressionBuilderHelper(builderMock.Object, contextMock.Object);

            helper.SetNullable(false);
            helper.SetExistingRowsTo(5);

            Assert.AreEqual(2, addedExpressions.Count);
            Assert.IsInstanceOf<UpdateDataExpression>(addedExpressions[0]);
            Assert.IsInstanceOf<AlterColumnExpression>(addedExpressions[1]);

            //TODO: refactor to use same method of checking as "CallingUniqueAddsIndexExpressionToContext" test does.
            AlterColumnExpression alterColExpr = (AlterColumnExpression)addedExpressions[1];
            Assert.AreNotSame(builderMock.Object.Column, alterColExpr.Column);
            Assert.AreEqual("Fred", alterColExpr.SchemaName);
            Assert.AreEqual("Flinstone", alterColExpr.TableName);

            //Check that the the 'alter' expression column definition is not the same instance as the
            //create column definition.
            Assert.IsNotNull(alterColExpr.Column);
            var alterColColumn = alterColExpr.Column;
            Assert.AreNotSame(createColColumn, alterColColumn);

            //Check that all properties on the alter expression column have been cloned.
            //Could also test this by mocking .clone method to return another mock etc, just doing
            //it here tho by comparing values.
            Assert.AreEqual(ColumnModificationType.Alter, alterColColumn.ModificationType);
            Assert.AreEqual("ColName", alterColColumn.Name);
            Assert.AreEqual(System.Data.DbType.String, alterColColumn.Type);
            Assert.AreEqual("CustomType", alterColColumn.CustomType);
            Assert.AreEqual(false, alterColColumn.IsNullable);
            Assert.AreEqual(12, alterColColumn.Size);
            Assert.AreEqual(2, alterColColumn.Precision);
        }

        [Test]
        public void SetExistingRows_AfterNotNullableSetsOriginalColumnNullable()
        {
            var builderMock = new Mock<IColumnExpressionBuilder>();
            var contextMock = new Mock<IMigrationContext>();
            List<IMigrationExpression> addedExpressions = new List<IMigrationExpression>();
            contextMock.SetupGet(n => n.Expressions).Returns(addedExpressions);
            builderMock.SetupGet(n => n.Column.ModificationType).Returns(ColumnModificationType.Create);
            builderMock.Setup(n => n.Column.Clone()).Returns(new ColumnDefinition());

            var helper = new ColumnExpressionBuilderHelper(builderMock.Object, contextMock.Object);

            helper.SetNullable(false);
            helper.SetExistingRowsTo(5);

            //Check that column is nullable.  This is because a later alter column statement will mark it non nullable.
            builderMock.VerifySet(n => n.Column.IsNullable = true);
        }

/*
        //Will this ever happen?  It should handle it, but need to test that if users goes
        // .Nullable().SetExistingRowsTo(5).NotNullable() it will be handled.
        public void SetExistingRows_SettingNullableRemovesAlterColumn()
        {
            throw new NotImplementedException();
        }
*/

        [Test]
        public void SetNullable_ToTrue()
        {
            VerifyColumnModification(h => h.SetNullable(true), c => c.IsNullable = true);
        }

        [Test]
        public void SetNullable_ToFalse()
        {
            VerifyColumnModification(h => h.SetNullable(false), c => c.IsNullable = false);
        }

        [Test]
        public void CallingUniqueSetsIsUniqueToTrue()
        {
            VerifyColumnModification(h => h.Unique(null), c => c.IsUnique = true);
        }

        [Test]
        public void CallingUniqueAddsIndexExpressionToContext()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var builderMock = new Mock<IColumnExpressionBuilder>();
            var contextMock = new Mock<IMigrationContext>();
            builderMock.SetupGet(n => n.Column.ModificationType).Returns(ColumnModificationType.Create);
            builderMock.SetupGet(n => n.Column.Name).Returns("BaconId");
            builderMock.SetupGet(n => n.SchemaName).Returns("Eggs");
            builderMock.SetupGet(n => n.TableName).Returns("Bacon");
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var helper = new ColumnExpressionBuilderHelper(builderMock.Object, contextMock.Object);
            helper.Unique("IX_Bacon_BaconId");

            collectionMock.Verify(x => x.Add(It.Is<CreateIndexExpression>(
                ix => ix.Index.Name == "IX_Bacon_BaconId"
                      && ix.Index.TableName == "Bacon"
                      && ix.Index.SchemaName == "Eggs"
                      && ix.Index.IsUnique
                      && !ix.Index.IsClustered
                      && ix.Index.Columns.All(c => c.Name == "BaconId")
                                                 )));

            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingIndexedNamedAddsIndexExpressionToContext()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();
            var builderMock = new Mock<IColumnExpressionBuilder>();
            var contextMock = new Mock<IMigrationContext>();
            builderMock.SetupGet(n => n.Column.ModificationType).Returns(ColumnModificationType.Create);
            builderMock.SetupGet(n => n.Column.Name).Returns("BaconId");
            builderMock.SetupGet(n => n.SchemaName).Returns("Eggs");
            builderMock.SetupGet(n => n.TableName).Returns("Bacon");
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var helper = new ColumnExpressionBuilderHelper(builderMock.Object, contextMock.Object);
            helper.Indexed("IX_Bacon_BaconId");

            collectionMock.Verify(x => x.Add(It.Is<CreateIndexExpression>(
                ix => ix.Index.Name == "IX_Bacon_BaconId"
                      && ix.Index.TableName == "Bacon"
                      && ix.Index.SchemaName == "Eggs"
                      && !ix.Index.IsUnique
                      && !ix.Index.IsClustered
                      && ix.Index.Columns.All(c => c.Name == "BaconId")
                                                 )));

            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingIndexedSetsIsIndexedToTrue()
        {
            VerifyColumnModification(h => h.Indexed(null), c => c.IsIndexed = true);
        }

        private void VerifyColumnModification(
            [NotNull] Action<ColumnExpressionBuilderHelper> helperCall,
            [NotNull] Action<ColumnDefinition> expectedAction)
        {
            var builderMock = new Mock<IColumnExpressionBuilder>();
            var contextMock = new Mock<IMigrationContext>();
            builderMock.SetupGet(n => n.Column.ModificationType).Returns(ColumnModificationType.Create);
            contextMock.SetupProperty(c => c.Expressions, new List<IMigrationExpression>());

            var helper = new ColumnExpressionBuilderHelper(builderMock.Object, contextMock.Object);
            helperCall.Invoke(helper);

            builderMock.VerifySet(n => expectedAction(n.Column));
        }
    }
}
