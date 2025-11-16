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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

using FluentMigrator.Builders;
using FluentMigrator.Builders.Alter.Column;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.SqlServer;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Alter
{
    [TestFixture]
    [Category("Builder")]
    [Category("AlterColumn")]
    public class AlterColumnExpressionBuilderTests
    {
        private void VerifyColumnProperty(Action<ColumnDefinition> columnExpression, Action<AlterColumnExpressionBuilder> callToTest)
        {
            var columnMock = new Mock<ColumnDefinition>();

            var expressionMock = new Mock<AlterColumnExpression>();
            expressionMock.SetupProperty(e => e.Column);

            var expression = expressionMock.Object;
            expression.Column = columnMock.Object;

            var contextMock = new Mock<IMigrationContext>();
            contextMock.SetupGet(mc => mc.Expressions).Returns(new Collection<IMigrationExpression>());

            callToTest(new AlterColumnExpressionBuilder(expression, contextMock.Object));

            columnMock.VerifySet(columnExpression);
        }

        private void VerifyColumnDbType(DbType expected, Action<AlterColumnExpressionBuilder> callToTest)
        {
            var columnMock = new Mock<ColumnDefinition>();

            var expressionMock = new Mock<AlterColumnExpression>();
            expressionMock.SetupProperty(e => e.Column);

            var expression = expressionMock.Object;
            expression.Column = columnMock.Object;

            var contextMock = new Mock<IMigrationContext>();

            callToTest(new AlterColumnExpressionBuilder(expression, contextMock.Object));

            columnMock.VerifySet(c => c.Type = expected);
        }

        private void VerifyColumnSize(int expected, Action<AlterColumnExpressionBuilder> callToTest)
        {
            var columnMock = new Mock<ColumnDefinition>();

            var expressionMock = new Mock<AlterColumnExpression>();
            expressionMock.SetupProperty(e => e.Column);

            var expression = expressionMock.Object;
            expression.Column = columnMock.Object;

            var contextMock = new Mock<IMigrationContext>();

            callToTest(new AlterColumnExpressionBuilder(expression, contextMock.Object));

            columnMock.VerifySet(c => c.Size = expected);
        }

        private void VerifyColumnPrecision(int expected, Action<AlterColumnExpressionBuilder> callToTest)
        {
            var columnMock = new Mock<ColumnDefinition>();

            var expressionMock = new Mock<AlterColumnExpression>();
            expressionMock.SetupProperty(e => e.Column);

            var expression = expressionMock.Object;
            expression.Column = columnMock.Object;

            var contextMock = new Mock<IMigrationContext>();

            callToTest(new AlterColumnExpressionBuilder(expression, contextMock.Object));

            columnMock.VerifySet(c => c.Precision = expected);
        }

        private void VerifyColumnCollation(string expected, Action<AlterColumnExpressionBuilder> callToTest)
        {
            var columnMock = new Mock<ColumnDefinition>();

            var expressionMock = new Mock<AlterColumnExpression>();
            expressionMock.SetupProperty(e => e.Column);

            var expression = expressionMock.Object;
            expression.Column = columnMock.Object;

            var contextMock = new Mock<IMigrationContext>();

            callToTest(new AlterColumnExpressionBuilder(expression, contextMock.Object));

            columnMock.VerifySet(c => c.CollationName = expected);
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
        public void CallingAsCustomSetsTypeToNullAndSetsCustomType()
        {
            VerifyColumnProperty(c => c.Type = null, b => b.AsCustom("Test"));
            VerifyColumnProperty(c => c.CustomType = "Test", b => b.AsCustom("Test"));
        }

        [Test]
        public void CallingAsColumnDataTypeWithNonNullCustomTypeSetsTypeToNullAndSetsCustomType()
        {
            VerifyColumnProperty(c => c.Type = null, b => b.AsColumnDataType(new ColumnDataType { CustomType = "Test"}));
            VerifyColumnProperty(c => c.CustomType = "Test", b => b.AsColumnDataType(new ColumnDataType { CustomType = "Test" }));
        }

        [Test]
        public void CallingAsColumnDataTypeSetsOnlyProvidedValue()
        {
            // Only provide Type
            Action<AlterColumnExpressionBuilder> provideType = b => b.AsColumnDataType(new ColumnDataType { Type = DbType.Boolean });
            VerifyColumnProperty(c => c.CustomType = null, provideType);
            VerifyColumnProperty(c => c.Type = DbType.Boolean, provideType);
            VerifyColumnProperty(c => c.CollationName = null, provideType);
            VerifyColumnProperty(c => c.Size = null, provideType);
            VerifyColumnProperty(c => c.Precision = null, provideType);
            // Provide type and size
            Action<AlterColumnExpressionBuilder> provideTypeSize = b => b.AsColumnDataType(new ColumnDataType { Type = DbType.String, Size = 50 });
            VerifyColumnProperty(c => c.CustomType = null, provideTypeSize);
            VerifyColumnProperty(c => c.Type = DbType.String, provideTypeSize);
            VerifyColumnProperty(c => c.CollationName = null, provideTypeSize);
            VerifyColumnProperty(c => c.Size = 50, provideTypeSize);
            VerifyColumnProperty(c => c.Precision = null, provideTypeSize);
            // Provide type and size with collation
            Action<AlterColumnExpressionBuilder> provideTypeSizeCollation = b => b.AsColumnDataType(new ColumnDataType { Type = DbType.AnsiString, Size = 50, CollationName = "test" });
            VerifyColumnProperty(c => c.CustomType = null, provideTypeSizeCollation);
            VerifyColumnProperty(c => c.Type = DbType.AnsiString, provideTypeSizeCollation);
            VerifyColumnProperty(c => c.CollationName = "test", provideTypeSizeCollation);
            VerifyColumnProperty(c => c.Size = 50, provideTypeSizeCollation);
            VerifyColumnProperty(c => c.Precision = null, provideTypeSizeCollation);
            // Provide type and size/precision
            Action<AlterColumnExpressionBuilder> provideTypeSizePrecision = b => b.AsColumnDataType(new ColumnDataType { Type = DbType.Decimal, Size = 28, Precision = 10});
            VerifyColumnProperty(c => c.CustomType = null, provideTypeSizePrecision);
            VerifyColumnProperty(c => c.Type = DbType.Decimal, provideTypeSizePrecision);
            VerifyColumnProperty(c => c.CollationName = null, provideTypeSizePrecision);
            VerifyColumnProperty(c => c.Size = 28, provideTypeSizePrecision);
            VerifyColumnProperty(c => c.Precision = 10, provideTypeSizePrecision);
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
        public void CallingAsDateTime2SetsColumnDbTypeToDateTime2()
        {
            VerifyColumnDbType(DbType.DateTime2, b => b.AsDateTime2());
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
        public void CallingAsDecimalStringSetsColumnPrecisionToSpecifiedValue()
        {
            VerifyColumnPrecision(2, b => b.AsDecimal(1, 2));
        }

        [Test]
        public void CallingAsDecimalStringSetsColumnSizeToSpecifiedValue()
        {
            VerifyColumnSize(1, b => b.AsDecimal(1, 2));
        }

        [Test]
        public void CallingAsDecimalWithSizeAndPrecisionSetsColumnDbTypeToDecimal()
        {
            VerifyColumnDbType(DbType.Decimal, b => b.AsDecimal(1, 2));
        }

        [Test]
        public void CallingAsDoubleSetsColumnDbTypeToDouble()
        {
            VerifyColumnDbType(DbType.Double, b => b.AsDouble());
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
        public void CallingAsFloatSetsColumnDbTypeToSingle()
        {
            VerifyColumnDbType(DbType.Single, b => b.AsFloat());
        }

        [Test]
        public void CallingAsGuidSetsColumnDbTypeToGuid()
        {
            VerifyColumnDbType(DbType.Guid, b => b.AsGuid());
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
        public void CallingAsStringSetsColumnSizeToSpecifiedValue()
        {
            VerifyColumnSize(255, b => b.AsFixedLengthAnsiString(255));
        }

        [Test]
        public void CallingAsStringWithLengthSetsColumnDbTypeToString()
        {
            VerifyColumnDbType(DbType.String, b => b.AsString(255));
        }

        [Test]
        public void CallingAsAnsiStringWithCollation()
        {
            VerifyColumnCollation(Generators.GeneratorTestHelper.TestColumnCollationName, b => b.AsAnsiString(Generators.GeneratorTestHelper.TestColumnCollationName));
        }

        [Test]
        public void CallingAsFixedLengthAnsiStringWithCollation()
        {
            VerifyColumnCollation(Generators.GeneratorTestHelper.TestColumnCollationName, b => b.AsFixedLengthAnsiString(255, Generators.GeneratorTestHelper.TestColumnCollationName));
        }

        [Test]
        public void CallingAsFixedLengthStringWithCollation()
        {
            VerifyColumnCollation(Generators.GeneratorTestHelper.TestColumnCollationName, b => b.AsFixedLengthString(255, Generators.GeneratorTestHelper.TestColumnCollationName));
        }

        [Test]
        public void CallingAsStringWithCollation()
        {
            VerifyColumnCollation(Generators.GeneratorTestHelper.TestColumnCollationName, b => b.AsString(Generators.GeneratorTestHelper.TestColumnCollationName));
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
        public void CallingAsXmlSetsColumnSizeToSpecifiedValue()
        {
            VerifyColumnSize(255, b => b.AsXml(255));
        }

        [Test]
        public void CallingAsXmlWithSizeSetsColumnDbTypeToXml()
        {
            VerifyColumnDbType(DbType.Xml, b => b.AsXml(255));
        }

        [Test]
        public void CallingForeignKeyAddsNewForeignKeyExpressionToContext()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var columnMock = new Mock<ColumnDefinition>();
            columnMock.SetupGet(x => x.Name).Returns("BaconId");

            var expressionMock = new Mock<AlterColumnExpression>();
            expressionMock.SetupGet(x => x.TableName).Returns("Bacon");
            expressionMock.SetupGet(x => x.Column).Returns(columnMock.Object);

            var builder = new AlterColumnExpressionBuilder(expressionMock.Object, contextMock.Object);

            builder.ForeignKey("fk_foo", "FooTable", "BarColumn");

            collectionMock.Verify(x => x.Add(It.Is<CreateForeignKeyExpression>(
                fk => fk.ForeignKey.Name == "fk_foo" &&
                      fk.ForeignKey.PrimaryTable == "FooTable" &&
                      fk.ForeignKey.PrimaryColumns.Contains("BarColumn") &&
                      fk.ForeignKey.PrimaryColumns.Count == 1 &&
                      fk.ForeignKey.ForeignTable == "Bacon" &&
                      fk.ForeignKey.ForeignColumns.Contains("BaconId") &&
                      fk.ForeignKey.ForeignColumns.Count == 1
                                                 )));

            contextMock.VerifyGet(x => x.Expressions);
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

            var expressionMock = new Mock<AlterColumnExpression>();
            expressionMock.SetupGet(x => x.Column).Returns(columnMock.Object);

            var builder = new AlterColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            builder.Identity(23, 44);

            columnMock.Object.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(SqlServerExtensions.IdentitySeed, 23));
            columnMock.Object.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(SqlServerExtensions.IdentityIncrement, 44));
        }

        [Test]
        public void CallingSeededLongIdentitySetsAdditionalProperties()
        {
            var contextMock = new Mock<IMigrationContext>();

            var columnMock = new Mock<ColumnDefinition>();
            columnMock.SetupGet(x => x.Name).Returns("BaconId");

            var expressionMock = new Mock<AlterColumnExpression>();
            expressionMock.SetupGet(x => x.Column).Returns(columnMock.Object);

            var builder = new AlterColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            builder.Identity(long.MaxValue, 44);

            columnMock.Object.AdditionalFeatures.ShouldContain(
                new KeyValuePair<string, object>(SqlServerExtensions.IdentitySeed, long.MaxValue));
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
        public void CallingOnTableSetsTableName()
        {
            var expressionMock = new Mock<AlterColumnExpression>();

            var contextMock = new Mock<IMigrationContext>();

            var builder = new AlterColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            builder.OnTable("Bacon");

            expressionMock.VerifySet(x => x.TableName = "Bacon");
        }

        [Test]
        public void CallingPrimaryKeySetsIsPrimaryKeyToTrue()
        {
            VerifyColumnProperty(c => c.IsPrimaryKey = true, b => b.PrimaryKey());
        }

        [Test]
        public void CallingReferencedByAddsNewForeignKeyExpressionToContext()
        {
            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var columnMock = new Mock<ColumnDefinition>();
            columnMock.SetupGet(x => x.Name).Returns("BaconId");

            var expressionMock = new Mock<AlterColumnExpression>();
            expressionMock.SetupGet(x => x.TableName).Returns("Bacon");
            expressionMock.SetupGet(x => x.Column).Returns(columnMock.Object);

            var builder = new AlterColumnExpressionBuilder(expressionMock.Object, contextMock.Object);

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

        [TestCase(Rule.Cascade), TestCase(Rule.SetDefault), TestCase(Rule.SetNull), TestCase(Rule.None)]
        public void CallingOnUpdateSetsOnUpdateOnForeignKeyExpression(Rule rule)
        {
            var builder = new AlterColumnExpressionBuilder(null, null) { CurrentForeignKey = new ForeignKeyDefinition() };
            builder.OnUpdate(rule);
            Assert.Multiple(() =>
            {
                Assert.That(builder.CurrentForeignKey.OnUpdate, Is.EqualTo(rule));
                Assert.That(builder.CurrentForeignKey.OnDelete, Is.EqualTo(Rule.None));
            });
        }

        [TestCase(Rule.Cascade), TestCase(Rule.SetDefault), TestCase(Rule.SetNull), TestCase(Rule.None)]
        public void CallingOnDeleteSetsOnDeleteOnForeignKeyExpression(Rule rule)
        {
            var builder = new AlterColumnExpressionBuilder(null, null) { CurrentForeignKey = new ForeignKeyDefinition() };
            builder.OnDelete(rule);
            Assert.Multiple(() =>
            {
                Assert.That(builder.CurrentForeignKey.OnUpdate, Is.EqualTo(Rule.None));
                Assert.That(builder.CurrentForeignKey.OnDelete, Is.EqualTo(rule));
            });
        }

        [TestCase(Rule.Cascade), TestCase(Rule.SetDefault), TestCase(Rule.SetNull), TestCase(Rule.None)]
        public void CallingOnDeleteOrUpdateSetsOnUpdateAndOnDeleteOnForeignKeyExpression(Rule rule)
        {
            var builder = new AlterColumnExpressionBuilder(null, null) { CurrentForeignKey = new ForeignKeyDefinition() };
            builder.OnDeleteOrUpdate(rule);
            Assert.Multiple(() =>
            {
                Assert.That(builder.CurrentForeignKey.OnUpdate, Is.EqualTo(rule));
                Assert.That(builder.CurrentForeignKey.OnDelete, Is.EqualTo(rule));
            });
        }

        [Test]
        public void CallingWithDefaultValueAddsAlterDefaultConstraintExpression()
        {
            const int value = 42;

            var columnMock = new Mock<ColumnDefinition>();

            var expressionMock = new Mock<AlterColumnExpression>();
            expressionMock.SetupProperty(e => e.Column);

            var expression = expressionMock.Object;
            expression.Column = columnMock.Object;

            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var builder = new AlterColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            builder.WithDefaultValue(value);

            columnMock.VerifySet(c => c.DefaultValue = value);
            collectionMock.Verify(x => x.Add(It.Is<AlterDefaultConstraintExpression>(e => e.DefaultValue.Equals(value))));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void CallingWithDefaultAddsAlterDefaultConstraintExpression()
        {
            var columnMock = new Mock<ColumnDefinition>();

            var expressionMock = new Mock<AlterColumnExpression>();
            expressionMock.SetupProperty(e => e.Column);

            var expression = expressionMock.Object;
            expression.Column = columnMock.Object;

            var collectionMock = new Mock<ICollection<IMigrationExpression>>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.Setup(x => x.Expressions).Returns(collectionMock.Object);

            var builder = new AlterColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            builder.WithDefault(SystemMethods.NewGuid);

            columnMock.VerifySet(c => c.DefaultValue = SystemMethods.NewGuid);
            collectionMock.Verify(x => x.Add(It.Is<AlterDefaultConstraintExpression>(e => e.DefaultValue.Equals(SystemMethods.NewGuid))));
            contextMock.VerifyGet(x => x.Expressions);
        }

        [Test]
        public void ColumnHelperSetOnCreation()
        {
            var expressionMock = new Mock<AlterColumnExpression>();
            var contextMock = new Mock<IMigrationContext>();

            var builder = new AlterColumnExpressionBuilder(expressionMock.Object, contextMock.Object);

            Assert.That(builder.ColumnHelper, Is.Not.Null);
        }

        [Test]
        public void ColumnExpressionBuilderUsesExpressionSchemaAndTableName()
        {
            var expressionMock = new Mock<AlterColumnExpression>();
            var contextMock = new Mock<IMigrationContext>();
            expressionMock.SetupGet(n => n.SchemaName).Returns("Fred");
            expressionMock.SetupGet(n => n.TableName).Returns("Flinstone");

            var builder = new AlterColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            var builderAsInterface = (IColumnExpressionBuilder)builder;

            Assert.Multiple(() =>
            {
                Assert.That(builderAsInterface.SchemaName, Is.EqualTo("Fred"));
                Assert.That(builderAsInterface.TableName, Is.EqualTo("Flinstone"));
            });
        }

        [Test]
        public void ColumnExpressionBuilderUsesExpressionColumn()
        {
            var expressionMock = new Mock<AlterColumnExpression>();
            var contextMock = new Mock<IMigrationContext>();
            var curColumn = new Mock<ColumnDefinition>().Object;
            expressionMock.SetupGet(n => n.Column).Returns(curColumn);

            var builder = new AlterColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            var builderAsInterface = (IColumnExpressionBuilder)builder;

            Assert.That(builderAsInterface.Column, Is.SameAs(curColumn));
        }

        [Test]
        public void CallingComputedSetsColumnExpression()
        {
            VerifyColumnProperty(c => c.Expression = "Price * Quantity", b => b.Computed("Price * Quantity"));
        }

        [Test]
        public void CallingComputedSetsColumnExpressionStoredToFalseByDefault()
        {
            VerifyColumnProperty(c => c.ExpressionStored = false, b => b.Computed("Price * Quantity"));
        }

        [Test]
        public void CallingComputedWithStoredTrueSetsColumnExpressionStoredToTrue()
        {
            VerifyColumnProperty(c => c.ExpressionStored = true, b => b.Computed("Price * Quantity", true));
        }

        private void VerifyColumnHelperCall(Action<AlterColumnExpressionBuilder> callToTest, System.Linq.Expressions.Expression<Action<ColumnExpressionBuilderHelper>> expectedHelperAction)
        {
            var expressionMock = new Mock<AlterColumnExpression>();
            var contextMock = new Mock<IMigrationContext>();
            var helperMock = new Mock<ColumnExpressionBuilderHelper>();

            var builder = new AlterColumnExpressionBuilder(expressionMock.Object, contextMock.Object);
            builder.ColumnHelper = helperMock.Object;

            callToTest(builder);

            helperMock.Verify(expectedHelperAction);
        }
    }
}
