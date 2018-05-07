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

using System;
using System.Data;

using FluentMigrator.Runner.Generators.SqlAnywhere;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlAnywhere
{
    [TestFixture]
    [Category("SqlAnywhere")]
    [Category("SqlAnywhere16")]
    [Category("Generator")]
    [Category("TypeMap")]
    public abstract class SqlAnywhere16TypeMapTests
    {
        private SqlAnywhere16TypeMap TypeMap { get; set; }

        [SetUp]
        public void Setup()
        {
            TypeMap = new SqlAnywhere16TypeMap();
        }

        [TestFixture]
        public class AnsistringTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            public void ItMapsAnsistringByDefaultToVarchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size: null, precision: null);

                template.ShouldBe("VARCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            public void ItMapsAnsistringWithSizeToVarcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size, precision: null);

                template.ShouldBe($"VARCHAR({size})");
            }

            [Test]
            [TestCase(8001)]
            [TestCase(2147483647)]
            public void ItMapsAnsistringWithSizeAbove8000ToText(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size, precision: null);

                template.ShouldBe("TEXT");
            }
        }

        [TestFixture]
        public class AnsistringFixedLengthTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            public void ItMapsAnsistringFixedLengthByDefaultToChar255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, size: null, precision: null);

                template.ShouldBe("CHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            public void ItMapsAnsistringFixedLengthWithSizeToCharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, size, precision: null);

                template.ShouldBe($"CHAR({size})");
            }

            [Test]
            public void ItThrowsIfAnsistringFixedLengthHasSizeAbove8000()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, size: 8001, precision: null));
            }
        }

        [TestFixture]
        public class StringTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            public void ItMapsStringByDefaultToNvarchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.String, size: null, precision: null);

                template.ShouldBe("NVARCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            public void ItMapsStringWithSizeToNvarcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.String, size, precision: null);

                template.ShouldBe($"NVARCHAR({size})");
            }

            [Test]
            [TestCase(4001)]
            [TestCase(1073741823)]
            public void ItMapsStringWithSizeAbove4000ToNtext(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.String, size, precision: null);

                template.ShouldBe("NTEXT");
            }

            [Test]
            public void ItMapsStringWithSizeAbove1073741823ToNtextToAllowIntMaxvalueConvention()
            {
                var template = TypeMap.GetTypeMap(DbType.String, int.MaxValue, precision: null);

                template.ShouldBe("NTEXT");
            }
        }

        [TestFixture]
        public class StringFixedLengthTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            public void ItMapsStringFixedLengthByDefaultToNchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, size: null, precision: null);

                template.ShouldBe("NCHAR(255)");
            }


            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            public void ItMapsStringFixedLengthWithSizeToNcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, size, precision: null);

                template.ShouldBe($"NCHAR({size})");
            }

            [Test]
            public void ItThrowsIfStringFixedLengthHasSizeAbove4000()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.StringFixedLength, size: 4001, precision: null));
            }
        }

        [TestFixture]
        public class BinaryTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            public void ItMapsBinaryByDefaultToVarbinary8000()
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size: null, precision: null);

                template.ShouldBe("VARBINARY(8000)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            public void ItMapsBinaryWithSizeToVarbinaryOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, precision: null);

                template.ShouldBe($"VARBINARY({size})");
            }

            [Test]
            [TestCase(8001)]
            [TestCase(int.MaxValue)]
            public void ItMapsBinaryWithSizeAbove8000ToImage(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, precision: null);

                template.ShouldBe("IMAGE");
            }
        }

        [TestFixture]
        public class NumericTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            public void ItMapsBooleanToBit()
            {
                var template = TypeMap.GetTypeMap(DbType.Boolean, size: null, precision: null);

                template.ShouldBe("BIT");
            }

            [Test]
            public void ItMapsByteToTinyint()
            {
                var template = TypeMap.GetTypeMap(DbType.Byte, size: null, precision: null);

                template.ShouldBe("TINYINT");
            }

            [Test]
            public void ItMapsInt16ToSmallint()
            {
                var template = TypeMap.GetTypeMap(DbType.Int16, size: null, precision: null);

                template.ShouldBe("SMALLINT");
            }

            [Test]
            public void ItMapsInt32ToInteger()
            {
                var template = TypeMap.GetTypeMap(DbType.Int32, size: null, precision: null);

                template.ShouldBe("INTEGER");
            }

            [Test]
            public void ItMapsInt64ToBigint()
            {
                var template = TypeMap.GetTypeMap(DbType.Int64, size: null, precision: null);

                template.ShouldBe("BIGINT");
            }

            [Test]
            public void ItMapsSingleToReal()
            {
                var template = TypeMap.GetTypeMap(DbType.Single, size: null, precision: null);

                template.ShouldBe("REAL");
            }

            [Test]
            public void ItMapsDoubleToDoublePrecision()
            {
                var template = TypeMap.GetTypeMap(DbType.Double, size: null, precision: null);

                template.ShouldBe("DOUBLE PRECISION");
            }

            [Test]
            public void ItMapsCurrencyToMoney()
            {
                var template = TypeMap.GetTypeMap(DbType.Currency, size: null, precision: null);

                template.ShouldBe("MONEY");
            }

            [Test]
            public void ItMapsDecimalByDefaultToDecimal306()
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, size: null, precision: null);

                template.ShouldBe("DECIMAL(30,6)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(20)]
            [TestCase(38)]
            public void ItMapsDecimalWithPrecisionToDecimal(int precision)
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, (int?)precision, precision: 1);

                template.ShouldBe($"DECIMAL({precision},1)");
            }

            [Test]
            public void ItThrowsIfDecimalPrecisionIsAbove127()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.Decimal, size: 128, precision: null));
            }

            [Test]
            public void ItMapsVarnumericByDefaultToNumeric195()
            {
                var template = TypeMap.GetTypeMap(DbType.VarNumeric, size: null, precision: null);

                template.ShouldBe("NUMERIC(30,6)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(20)]
            [TestCase(38)]
            public void ItMapsVarnumericWithPrecisionToNumeric(int precision)
            {
                var template = TypeMap.GetTypeMap(DbType.VarNumeric, (int?)precision, 1);

                template.ShouldBe($"NUMERIC({precision},1)");
            }

            [Test]
            public void ItThrowsIfVarnumericPrecisionIsAbove127()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.VarNumeric, size: 128, precision: null));
            }
        }

        [TestFixture]
        public class GuidTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            public void ItMapsGUIDToUniqueidentifier()
            {
                var template = TypeMap.GetTypeMap(DbType.Guid, size: null, precision: null);

                template.ShouldBe("UNIQUEIDENTIFIER");
            }
        }

        [TestFixture]
        public class DateTimeTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            public void ItMapsTimeToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Time, size: null, precision: null);

                template.ShouldBe("DATETIME");
            }

            [Test]
            public void ItMapsDateToDate()
            {
                var template = TypeMap.GetTypeMap(DbType.Date, size: null, precision: null);

                template.ShouldBe("DATE");
            }

            [Test]
            public void ItMapsDatetimeToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.DateTime, size: null, precision: null);

                template.ShouldBe("DATETIME");
            }
        }
    }
}
