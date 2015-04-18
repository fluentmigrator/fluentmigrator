#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Runner.Extensions;
using Moq;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Builders;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
    [TestFixture]
    public class CreateColumnExpressionBuilderTests
    {
        [Test]
        public void CallingOnTableSetsTableName()
        {
            var expressionMock = new Mock<CreateColumnExpression>();

            var contextMock = new Mock<IMigrationContext>();

            var builder = new CreateColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            builder.OnTable("Bacon");

            expressionMock.VerifySet(x => x.TableName = "Bacon");
        }

        [Test]
        public void CallingAsAnsiStringSetsColumnDbTypeToAnsiString()
        {
            VerifyColumnDbType(DbType.AnsiString, b => b.AsAnsiString());
        }

        [Test]
        public void CallingAsAnsiStringWithSizeSetsColumnDbTypeToAnsiString()
        {
            VerifyColumnDbType(DbType.AnsiString, b => b.AsAnsiString(42));
        }

        [Test]
        public void CallingAsAnsiStringWithSizeSetsColumnSizeToSpecifiedValue()
        {
            VerifyColumnSize(42, b => b.AsAnsiString(42));
        }

        [Test]
        public void CallingAsBinarySetsColumnDbTypeToBinary()
        {
            VerifyColumnDbType(DbType.Binary, b => b.AsBinary());
        }

        [Test]
        public void CallingAsBinaryWithSizeSetsColumnDbTypeToBinary()
        {
            VerifyColumnDbType(DbType.Binary, b => b.AsBinary(42));
        }

        [Test]
        public void CallingAsBinaryWithSizeSetsColumnSizeToSpecifiedValue()
        {
            VerifyColumnSize(42, b => b.AsBinary(42));
        }

        [Test]
        public void CallingAsBooleanSetsColumnDbTypeToBoolean()
        {
            VerifyColumnDbType(DbType.Boolean, b => b.AsBoolean());
        }

        [Test]
        public void CallingAsByteSetsColumnDbTypeToByte()
        {
            VerifyColumnDbType(DbType.Byte, b => b.AsByte());
        }

        [Test]
        public void CallingAsCurrencySetsColumnDbTypeToCurrency()
        {
            VerifyColumnDbType(DbType.Currency, b => b.AsCurrency());
        }

        [Test]
        public void CallingAsDateSetsColumnDbTypeToDate()
        {
            VerifyColumnDbType(DbType.Date, b => b.AsDate());
        }

        [Test]
        public void CallingAsDateTimeSetsColumnDbTypeToDateTime()
        {
            VerifyColumnDbType(DbType.DateTime, b => b.AsDateTime());
        }

        [Test]
        public void CallingAsDateTimeOffsetSetsColumnDbTypeToDateTimeOffset()
        {
            VerifyColumnDbType(DbType.DateTimeOffset, b => b.AsDateTimeOffset());
        }

        [Test]
        public void CallingAsDecimalSetsColumnDbTypeToDecimal()
        {
            VerifyColumnDbType(DbType.Decimal, b => b.AsDecimal());
        }

        [Test]
        public void CallingAsDecimalWithSizeAndPrecisionSetsColumnDbTypeToDecimal()
        {
            VerifyColumnDbType(DbType.Decimal, b => b.AsDecimal(1, 2));
        }

        [Test]
        public void CallingAsDecimalStringSetsColumnSizeToSpecifiedValue()
        {
            VerifyColumnSize(1, b => b.AsDecimal(1, 2));
        }

        [Test]
        public void CallingAsDecimalStringSetsColumnPrecisionToSpecifiedValue()
        {
            VerifyColumnPrecision(2, b => b.AsDecimal(1, 2));
        }

        [Test]
        public void CallingAsDoubleSetsColumnDbTypeToDouble()
        {
            VerifyColumnDbType(DbType.Double, b => b.AsDouble());
        }

        [Test]
        public void CallingAsGuidSetsColumnDbTypeToGuid()
        {
            VerifyColumnDbType(DbType.Guid, b => b.AsGuid());
        }

        [Test]
        public void CallingAsFixedLengthStringSetsColumnDbTypeToStringFixedLength()
        {
            VerifyColumnDbType(DbType.StringFixedLength, e => e.AsFixedLengthString(255));
        }

        [Test]
        public void CallingAsFixedLengthStringSetsColumnSizeToSpecifiedValue()
        {
            VerifyColumnSize(255, b => b.AsFixedLengthString(255));
        }

        [Test]
        public void CallingAsFixedLengthAnsiStringSetsColumnDbTypeToAnsiStringFixedLength()
        {
            VerifyColumnDbType(DbType.AnsiStringFixedLength, b => b.AsFixedLengthAnsiString(255));
        }

        [Test]
        public void CallingAsFixedLengthAnsiStringSetsColumnSizeToSpecifiedValue()
        {
            VerifyColumnSize(255, b => b.AsFixedLengthAnsiString(255));
        }

        [Test]
        public void CallingAsFloatSetsColumnDbTypeToSingle()
        {
            VerifyColumnDbType(DbType.Single, b => b.AsFloat());
        }

        [Test]
        public void CallingAsInt16SetsColumnDbTypeToInt16()
        {
            VerifyColumnDbType(DbType.Int16, b => b.AsInt16());
        }

        [Test]
        public void CallingAsInt32SetsColumnDbTypeToInt32()
        {
            VerifyColumnDbType(DbType.Int32, b => b.AsInt32());
        }

        [Test]
        public void CallingAsInt64SetsColumnDbTypeToInt64()
        {
            VerifyColumnDbType(DbType.Int64, b => b.AsInt64());
        }

        [Test]
        public void CallingAsStringSetsColumnDbTypeToString()
        {
            VerifyColumnDbType(DbType.String, b => b.AsString());
        }

        [Test]
        public void CallingAsStringWithLengthSetsColumnDbTypeToString()
        {
            VerifyColumnDbType(DbType.String, b => b.AsString(255));
        }

        [Test]
        public void CallingAsStringSetsColumnSizeToSpecifiedValue()
        {
            VerifyColumnSize(255, b => b.AsFixedLengthAnsiString(255));
        }

        [Test]
        public void CallingAsTimeSetsColumnDbTypeToTime()
        {
            VerifyColumnDbType(DbType.Time, b => b.AsTime());
        }

        [Test]
        public void CallingAsXmlSetsColumnDbTypeToXml()
        {
            VerifyColumnDbType(DbType.Xml, b => b.AsXml());
        }

        [Test]
        public void CallingAsXmlWithSizeSetsColumnDbTypeToXml()
        {
            VerifyColumnDbType(DbType.Xml, b => b.AsXml(255));
        }

        [Test]
        public void CallingAsXmlSetsColumnSizeToSpecifiedValue()
        {
            VerifyColumnSize(255, b => b.AsXml(255));
        }

        [Test]
        public void CallingAsCustomSetsTypeToNullAndSetsCustomType()
        {
            VerifyColumnProperty(c => c.Type = null, b => b.AsCustom("Test"));
            VerifyColumnProperty(c => c.CustomType = "Test", b => b.AsCustom("Test"));
        }

        [Test]
        public void CallingWithDefaultValueSetsDefaultValue()
        {
            const int value = 42;

            var columnMock = new Mock<ColumnDefinition>();

            var expressionMock = new Mock<CreateColumnExpression>();
            expressionMock.SetupProperty(e => e.Column);

            var expression = expressionMock.Object;
            expression.Column = columnMock.Object;

            var contextMock = new Mock<IMigrationContext>();

            var builder = new CreateColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            builder.WithDefaultValue(value);

            columnMock.VerifySet(c => c.DefaultValue = value);
        }

        [Test]
        public void CallingWithDefaultSetsDefaultValue()
        {
            var columnMock = new Mock<ColumnDefinition>();

            var expressionMock = new Mock<CreateColumnExpression>();
            expressionMock.SetupProperty(e => e.Column);

            var expression = expressionMock.Object;
            expression.Column = columnMock.Object;

            var contextMock = new Mock<IMigrationContext>();

            var builder = new CreateColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            builder.WithDefault(SystemMethods.NewGuid);

            columnMock.VerifySet(c => c.DefaultValue = SystemMethods.NewGuid);
        }

        [Test]
        public void CallingForeignKeySetsIsForeignKeyToTrue()
        {
            VerifyColumnProperty(c => c.IsForeignKey = true, b => b.ForeignKey());
        }

        [Test]
        public void CallingIdentitySetsIsIdentityToTrue()
        {
            VerifyColumnProperty(c => c.IsIdentity = true, b => b.Identity());
        }

        [Test]
        public void CallingSeededIdentitySetsAdditionalProperties() 
        {
            var contextMock = new Mock<IMigrationContext>();

            var columnMock = new Mock<ColumnDefinition>();
            columnMock.SetupGet(x => x.Name).Returns("BaconId");

            var expressionMock = new Mock<CreateColumnExpression>();
            expressionMock.SetupGet(x => x.Column).Returns(columnMock.Object);

            var builder = new CreateColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            builder.Identity(23, 44);

            columnMock.Object.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(SqlServerExtensions.IdentitySeed, 23));
            columnMock.Object.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(SqlServerExtensions.IdentityIncrement, 44));
        }

        [Test]
        public void CallingIndexedCallsHelperWithNullIndexName()
        {
            VerifyColumnHelperCall(c => c.Indexed(), h => h.Indexed(null));
        }

        [Test]
        public void CallingIndexedNamedCallsHelperWithName()
        {
            VerifyColumnHelperCall(c => c.Indexed("MyIndexName"), h => h.Indexed("MyIndexName"));
        }
        
        [Test]
        public void CallingPrimaryKeySetsIsPrimaryKeyToTrue()
        {
            VerifyColumnProperty(c => c.IsPrimaryKey = true, b => b.PrimaryKey());
        }

        [Test]
        public void CallingReferencesAddsNewForeignKeyExpressionToContext()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var columnMock = new Mock<ColumnDefinition>();
            columnMock.SetupGet(x => x.Name).Returns("BaconId");

            var expressionMock = new Mock<CreateColumnExpression>();
            expressionMock.SetupGet(x => x.TableName).Returns("Bacon");
            expressionMock.SetupGet(x => x.Column).Returns(columnMock.Object);

            var builder = new CreateColumnExpressionBuilder(expressionMock.Object, contextMock.Object);

            builder.References("fk_foo", "FooTable", new[] { "BarColumn" });

            collectionMock.Verify(x => x.Add(It.Is<CreateForeignKeyExpression>(
                fk => fk.ForeignKey.Name == "fk_foo" &&
                        fk.ForeignKey.ForeignTable == "FooTable" &&
                        fk.ForeignKey.ForeignColumns.Contains("BarColumn") &&
                        fk.ForeignKey.ForeignColumns.Count == 1 &&
                        fk.ForeignKey.PrimaryTable == "Bacon" &&
                        fk.ForeignKey.PrimaryColumns.Contains("BaconId") &&
                        fk.ForeignKey.PrimaryColumns.Count == 1
                                                )));

            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingReferencedByAddsNewForeignKeyExpressionToContext()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var columnMock = new Mock<ColumnDefinition>();
            columnMock.SetupGet(x => x.Name).Returns("BaconId");

            var expressionMock = new Mock<CreateColumnExpression>();
            expressionMock.SetupGet(x => x.TableName).Returns("Bacon");
            expressionMock.SetupGet(x => x.Column).Returns(columnMock.Object);

            var builder = new CreateColumnExpressionBuilder(expressionMock.Object, contextMock.Object);

            builder.ReferencedBy("fk_foo", "FooTable", "BarColumn");

            collectionMock.Verify(x => x.Add(It.Is<CreateForeignKeyExpression>(
                fk => fk.ForeignKey.Name == "fk_foo" &&
                        fk.ForeignKey.ForeignTable == "FooTable" &&
                        fk.ForeignKey.ForeignColumns.Contains("BarColumn") &&
                        fk.ForeignKey.ForeignColumns.Count == 1 &&
                        fk.ForeignKey.PrimaryTable == "Bacon" &&
                        fk.ForeignKey.PrimaryColumns.Contains("BaconId") &&
                        fk.ForeignKey.PrimaryColumns.Count == 1
                                                )));

            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingForeignKeyAddsNewForeignKeyExpressionToContext()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var columnMock = new Mock<ColumnDefinition>();
            columnMock.SetupGet(x => x.Name).Returns("BaconId");

            var expressionMock = new Mock<CreateColumnExpression>();
            expressionMock.SetupGet(x => x.TableName).Returns("Bacon");
            expressionMock.SetupGet(x => x.Column).Returns(columnMock.Object);

            var builder = new CreateColumnExpressionBuilder(expressionMock.Object, contextMock.Object);

            builder.ForeignKey("fk_foo", "FooTable", "BarColumn");

            contextMock.VerifyGet(x => x.Expressions);
            collectionMock.Verify(x => x.Add(It.Is<CreateForeignKeyExpression>(
                fk => fk.ForeignKey.Name == "fk_foo" &&
                        fk.ForeignKey.PrimaryTable == "FooTable" &&
                        fk.ForeignKey.PrimaryColumns.Contains("BarColumn") &&
                        fk.ForeignKey.PrimaryColumns.Count == 1 &&
                        fk.ForeignKey.ForeignTable == "Bacon" &&
                        fk.ForeignKey.ForeignColumns.Contains("BaconId") &&
                        fk.ForeignKey.ForeignColumns.Count == 1
                                                )));
        }

        [TestCase(Rule.Cascade), TestCase(Rule.SetDefault), TestCase(Rule.SetNull), TestCase(Rule.None)]
        public void CallingOnUpdateSetsOnUpdateOnForeignKeyExpression(Rule rule) 
        {
            var builder = new CreateColumnExpressionBuilder(null, null) {CurrentForeignKey = new ForeignKeyDefinition()};
            builder.OnUpdate(rule);
            Assert.That(builder.CurrentForeignKey.OnUpdate, Is.EqualTo(rule));
            Assert.That(builder.CurrentForeignKey.OnDelete, Is.EqualTo(Rule.None));
        }

        [TestCase(Rule.Cascade), TestCase(Rule.SetDefault), TestCase(Rule.SetNull), TestCase(Rule.None)]
        public void CallingOnDeleteSetsOnDeleteOnForeignKeyExpression(Rule rule) 
        {
            var builder = new CreateColumnExpressionBuilder(null, null) { CurrentForeignKey = new ForeignKeyDefinition() };
            builder.OnDelete(rule);
            Assert.That(builder.CurrentForeignKey.OnUpdate, Is.EqualTo(Rule.None));
            Assert.That(builder.CurrentForeignKey.OnDelete, Is.EqualTo(rule));
        }

        [TestCase(Rule.Cascade), TestCase(Rule.SetDefault), TestCase(Rule.SetNull), TestCase(Rule.None)]
        public void CallingOnDeleteOrUpdateSetsOnUpdateAndOnDeleteOnForeignKeyExpression(Rule rule) 
        {
            var builder = new CreateColumnExpressionBuilder(null, null) { CurrentForeignKey = new ForeignKeyDefinition() };
            builder.OnDeleteOrUpdate(rule);
            Assert.That(builder.CurrentForeignKey.OnUpdate, Is.EqualTo(rule));
            Assert.That(builder.CurrentForeignKey.OnDelete, Is.EqualTo(rule));
        }

        [Test]
        public void ColumnHelperSetOnCreation()
        {
            var expressionMock = new Mock<CreateColumnExpression>();
            var contextMock = new Mock<IMigrationContext>();

            var builder = new CreateColumnExpressionBuilder(expressionMock.Object, contextMock.Object);

            Assert.IsNotNull(builder.ColumnHelper);
        }

        [Test]
        public void IColumnExpressionBuilder_UsesExpressionColumnSchemaAndTableName()
        {
            var expressionMock = new Mock<CreateColumnExpression>();
            var contextMock = new Mock<IMigrationContext>();
            expressionMock.SetupGet(n => n.SchemaName).Returns("Fred");
            expressionMock.SetupGet(n => n.TableName).Returns("Flinstone");

            var builder = new CreateColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            var builderAsInterface = (IColumnExpressionBuilder)builder;

            Assert.AreEqual("Fred", builderAsInterface.SchemaName);
            Assert.AreEqual("Flinstone", builderAsInterface.TableName);
        }

        [Test]
        public void IColumnExpressionBuilder_UsesExpressionColumn()
        {
            var expressionMock = new Mock<CreateColumnExpression>();
            var contextMock = new Mock<IMigrationContext>();
            var curColumn = new Mock<ColumnDefinition>().Object;
            expressionMock.SetupGet(n => n.Column).Returns(curColumn);

            var builder = new CreateColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            var builderAsInterface = (IColumnExpressionBuilder)builder;

            Assert.AreSame(curColumn, builderAsInterface.Column);
        }

        [Test]
        public void NullableUsesHelper()
        {
            VerifyColumnHelperCall(c => c.Nullable(), h => h.SetNullable(true));
        }

        [Test]
        public void NotNullableUsesHelper()
        {
            VerifyColumnHelperCall(c => c.NotNullable(), h => h.SetNullable(false));
        }

        [Test]
        public void UniqueUsesHelper()
        {
            VerifyColumnHelperCall(c => c.Unique(), h => h.Unique(null));
        }

        [Test]
        public void NamedUniqueUsesHelper()
        {
            VerifyColumnHelperCall(c => c.Unique("asdf"), h => h.Unique("asdf"));
        }

        [Test]
        public void SetExistingRowsUsesHelper()
        {
            VerifyColumnHelperCall(c => c.SetExistingRowsTo("test"), h => h.SetExistingRowsTo("test"));
        }

        private void VerifyColumnHelperCall(Action<CreateColumnExpressionBuilder> callToTest, System.Linq.Expressions.Expression<Action<ColumnExpressionBuilderHelper>> expectedHelperAction)
        {
            var expressionMock = new Mock<CreateColumnExpression>();
            var contextMock = new Mock<IMigrationContext>();
            var helperMock = new Mock<ColumnExpressionBuilderHelper>();

            var builder = new CreateColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            builder.ColumnHelper = helperMock.Object;

            callToTest(builder);

            helperMock.Verify(expectedHelperAction);
        }

        private void VerifyColumnProperty(Action<ColumnDefinition> columnExpression, Action<CreateColumnExpressionBuilder> callToTest)
        {
            var columnMock = new Mock<ColumnDefinition>();

            var expressionMock = new Mock<CreateColumnExpression>();
            expressionMock.SetupProperty(e => e.Column);

            var expression = expressionMock.Object;
            expression.Column = columnMock.Object;

            var contextMock = new Mock<IMigrationContext>();
            contextMock.SetupGet(mc => mc.Expressions).Returns(new Collection<IMigrationExpression>());

            callToTest(new CreateColumnExpressionBuilder(expression, contextMock.Object));

            columnMock.VerifySet(columnExpression);
        }

        private void VerifyColumnDbType(DbType expected, Action<CreateColumnExpressionBuilder> callToTest)
        {
            var columnMock = new Mock<ColumnDefinition>();

            var expressionMock = new Mock<CreateColumnExpression>();
            expressionMock.SetupProperty(e => e.Column);

            var expression = expressionMock.Object;
            expression.Column = columnMock.Object;

            var contextMock = new Mock<IMigrationContext>();

            callToTest(new CreateColumnExpressionBuilder(expression, contextMock.Object));

            columnMock.VerifySet(c => c.Type = expected);
        }

        private void VerifyColumnSize(int expected, Action<CreateColumnExpressionBuilder> callToTest)
        {
            var columnMock = new Mock<ColumnDefinition>();

            var expressionMock = new Mock<CreateColumnExpression>();
            expressionMock.SetupProperty(e => e.Column);

            var expression = expressionMock.Object;
            expression.Column = columnMock.Object;

            var contextMock = new Mock<IMigrationContext>();

            callToTest(new CreateColumnExpressionBuilder(expression, contextMock.Object));

            columnMock.VerifySet(c => c.Size = expected);
        }

        private void VerifyColumnPrecision(int expected, Action<CreateColumnExpressionBuilder> callToTest)
        {
            var columnMock = new Mock<ColumnDefinition>();

            var expressionMock = new Mock<CreateColumnExpression>();
            expressionMock.SetupProperty(e => e.Column);

            var expression = expressionMock.Object;
            expression.Column = columnMock.Object;

            var contextMock = new Mock<IMigrationContext>();

            callToTest(new CreateColumnExpressionBuilder(expression, contextMock.Object));

            columnMock.VerifySet(c => c.Precision = expected);
        }
    }
}
