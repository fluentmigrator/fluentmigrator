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
using FluentMigrator.Runner.Generators.Snowflake;

using NUnit.Framework;
using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Snowflake
{
    [TestFixture]
    [Category("Snowflake")]
    [Category("Generator")]
    [Category("TypeMap")]
    public abstract class SnowflakeTypeMapTests
    {
        private SnowflakeTypeMap TypeMap { get; set; }

        [SetUp]
        public void Setup()
        {
            TypeMap = new SnowflakeTypeMap();
        }

        [TestFixture]
        public class AnsiStringTests : SnowflakeTypeMapTests
        {
            [Test]
            public void ItMapsAnsiStringByDefaultToVarchar()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size: null, precision: null);

                template.ShouldBe("VARCHAR");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            [TestCase(8001)]
            [TestCase(4194304)]
            [TestCase(134217728)]
            public void ItMapsAnsiStringWithSizeToVarcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size, precision: null);

                template.ShouldBe($"VARCHAR({size})");
            }

            [Test]
            public void ItThrowsIfAnsiStringHasSizeAbove134217728()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.AnsiString, 134217729, precision: null));
            }
        }

        [TestFixture]
        public class AnsiStringFixedLengthTests : SnowflakeTypeMapTests
        {
            [Test]
            public void ItMapsAnsiStringFixedLengthByDefaultToVarchar()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, size: null, precision: null);

                template.ShouldBe("VARCHAR");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            [TestCase(8001)]
            [TestCase(4194304)]
            [TestCase(134217728)]
            public void ItMapsAnsiStringFixedLengthWithSizeToCharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, size, precision: null);

                template.ShouldBe($"VARCHAR({size})");
            }

            [Test]
            public void ItThrowsIfAnsiStringHasSizeAbove134217728()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, 134217729, precision: null));
            }
        }

        [TestFixture]
        public class StringTests : SnowflakeTypeMapTests
        {
            [Test]
            public void ItMapsStringByDefaultToVarchar()
            {
                var template = TypeMap.GetTypeMap(DbType.String, size: null, precision: null);

                template.ShouldBe("VARCHAR");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(4001)]
            [TestCase(4194304)]
            [TestCase(134217728)]
            public void ItMapsStringWithSizeToNvarcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.String, size, precision: null);

                template.ShouldBe($"VARCHAR({size})");
            }

            [Test]
            public void ItThrowsIfStringHasSizeAbove134217728()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.String, 134217729, precision: null));
            }
        }

        [TestFixture]
        public class StringFixedLengthTests : SnowflakeTypeMapTests
        {
            [Test]
            public void ItMapsStringFixedLengthByDefaultToVarchar()
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, size: null, precision: null);

                template.ShouldBe("VARCHAR");
            }


            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(4001)]
            public void ItMapsStringFixedLengthWithSizeToNcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, size, precision: null);

                template.ShouldBe($"VARCHAR({size})");
            }
        }

        [TestFixture]
        public class BinaryTests : SnowflakeTypeMapTests
        {
            [Test]
            public void ItMapsBinaryByDefaultToBinary()
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size: null, precision: null);
                template.ShouldBe("BINARY");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            [TestCase(8388608)]
            public void ItMapsBinaryWithSizeToVarbinaryOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, precision: null);
                template.ShouldBe($"BINARY({size})");
            }

            [Test]
            [TestCase(8388609)]
            public void ItMapsBinaryWithSizeAbove8000ToImage(int size)
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.Binary, size, precision: null));
            }
        }

        [TestFixture]
        public class NumericTests : SnowflakeTypeMapTests
        {
            [Test]
            public void ItMapsBooleanToBit()
            {
                var template = TypeMap.GetTypeMap(DbType.Boolean, size: null, precision: null);

                template.ShouldBe("BOOLEAN");
            }

            [Test]
            public void ItMapsByteToNumber()
            {
                var template = TypeMap.GetTypeMap(DbType.Byte, size: null, precision: null);

                template.ShouldBe("NUMBER");
            }

            [Test]
            public void ItMapsInt16ToNumber()
            {
                var template = TypeMap.GetTypeMap(DbType.Int16, size: null, precision: null);

                template.ShouldBe("NUMBER");
            }

            [Test]
            public void ItMapsInt32ToNumber()
            {
                var template = TypeMap.GetTypeMap(DbType.Int32, size: null, precision: null);

                template.ShouldBe("NUMBER");
            }

            [Test]
            public void ItMapsInt64ToNumber()
            {
                var template = TypeMap.GetTypeMap(DbType.Int64, size: null, precision: null);

                template.ShouldBe("NUMBER");
            }

            [Test]
            public void ItMapsSingleToFloat()
            {
                var template = TypeMap.GetTypeMap(DbType.Single, size: null, precision: null);

                template.ShouldBe("FLOAT");
            }

            [Test]
            public void ItMapsDoubleToFloat()
            {
                var template = TypeMap.GetTypeMap(DbType.Double, size: null, precision: null);

                template.ShouldBe("FLOAT");
            }

            [Test]
            public void ItMapsDecimalByDefaultToNumber()
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, size: null, precision: null);

                template.ShouldBe("NUMBER");
            }

            [Test]
            [TestCase(1)]
            [TestCase(20)]
            [TestCase(38)]
            public void ItMapsDecimalWithPrecisionToNumber(int precision)
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, precision, 1);

                template.ShouldBe($"NUMBER({precision},1)");
            }

            [Test]
            public void ItThrowsIfDecimalPrecisionIsAbove38()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.Decimal, 39, precision: null));
            }
        }

        [TestFixture]
        public class DateTimeTests : SnowflakeTypeMapTests
        {
            [Test]
            public void ItMapsTimeToTime()
            {
                var template = TypeMap.GetTypeMap(DbType.Time, size: null, precision: null);

                template.ShouldBe("TIME");
            }

            [Test]
            public void ItMapsDateToDate()
            {
                var template = TypeMap.GetTypeMap(DbType.Date, size: null, precision: null);

                template.ShouldBe("DATE");
            }

            [Test]
            public void ItMapsDatetimeTimestampNtz()
            {
                var template = TypeMap.GetTypeMap(DbType.DateTime, size: null, precision: null);

                template.ShouldBe("TIMESTAMP_NTZ");
            }

            [Test]
            public void ItMapsDatetime2TimestampNtz()
            {
                var template = TypeMap.GetTypeMap(DbType.DateTime2, size: null, precision: null);

                template.ShouldBe("TIMESTAMP_NTZ");
            }

            [Test]
            public void ItMapsDatetimeOffsetTimestampTz()
            {
                var template = TypeMap.GetTypeMap(DbType.DateTimeOffset, size: null, precision: null);

                template.ShouldBe("TIMESTAMP_TZ");
            }
        }
    }
}
