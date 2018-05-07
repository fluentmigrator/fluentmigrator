using System;
using System.Data;

using FluentMigrator.Runner.Generators.SqlServer;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServerCe
{
    [TestFixture]
    [Category("SqlServerCe")]
    [Category("Generator")]
    [Category("TypeMap")]
    public abstract class SqlServerCeTypeMapTests
    {
        private SqlServerCeTypeMap TypeMap { get; set; }

        [SetUp]
        public void Setup()
        {
            TypeMap = new SqlServerCeTypeMap();
        }

        [TestFixture]
        public class AnsistringTests : SqlServerCeTypeMapTests
        {
            [Test]
            public void ItMapsAnsistringByDefaultToNvarchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size: null, precision: null);

                template.ShouldBe("NVARCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(2000)]
            [TestCase(4000)]
            public void ItMapsAnsistringWithSizeToNvarcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size, precision: null);

                template.ShouldBe($"NVARCHAR({size})");
            }

            [Test]
            public void ItMapsAnsistringWithMaxSizeToNtext()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, int.MaxValue, precision: null);
                template.ShouldBe("NTEXT");
            }
        }

        [TestFixture]
        public class AnsistringFixedLengthTests : SqlServerCeTypeMapTests
        {
            [Test]
            public void ItMapsAnsistringFixedLengthByDefaultToNchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, size: null, precision: null);

                template.ShouldBe("NCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(2000)]
            [TestCase(4000)]
            public void ItMapsAnsistringFixedLengthWithSizeToNcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, size, precision: null);

                template.ShouldBe($"NCHAR({size})");
            }

            [Test]
            public void ItThrowsIfAnsistringFixedLengthHasSizeAbove4000()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, 4001, precision: null));
            }
        }

        [TestFixture]
        public class StringTests : SqlServerCeTypeMapTests
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
            public void ItThrowsIfStringHasSizeAbove4000()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, 4001, precision: null));
            }
        }

        [TestFixture]
        public class StringFixedLengthTests : SqlServerCeTypeMapTests
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
                    () => TypeMap.GetTypeMap(DbType.StringFixedLength, 4001, precision: null));
            }
        }

        [TestFixture]
        public class BinaryTests : SqlServerCeTypeMapTests
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
            [TestCase(1073741823)]
            public void ItMapsBinaryWithSizeAbove8000ToImage(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, precision: null);

                template.ShouldBe("IMAGE");
            }

            [Test]
            [TestCase(1073741824)]
            public void ItThrowsIfBinarySizeIsAbove1073741823(int size)
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.Binary, size, precision: null));
            }
        }

        [TestFixture]
        public class NumericTests : SqlServerCeTypeMapTests
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
            public void ItMapsInt32ToInt()
            {
                var template = TypeMap.GetTypeMap(DbType.Int32, size: null, precision: null);

                template.ShouldBe("INT");
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

                template.ShouldBe("FLOAT");
            }

            [Test]
            public void ItMapsCurrencyToMoney()
            {
                var template = TypeMap.GetTypeMap(DbType.Currency, size: null, precision: null);

                template.ShouldBe("MONEY");
            }

            [Test]
            public void ItMapsDecimalByDefaultToDecimal195()
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, size: null, precision: null);

                template.ShouldBe("NUMERIC(19,5)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(20)]
            [TestCase(38)]
            public void ItMapsDecimalWithPrecisionToDecimal(int precision)
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, (int?) precision, precision: 1);

                template.ShouldBe($"NUMERIC({precision},1)");
            }

            [Test]
            public void ItThrowsIfDecimalPrecisionIsAbove38()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.Decimal, 39, precision: null));
            }
        }

        [TestFixture]
        public class GuidTests : SqlServerCeTypeMapTests
        {
            [Test]
            public void ItMapsGUIDToUniqueidentifier()
            {
                var template = TypeMap.GetTypeMap(DbType.Guid, size: null, precision: null);

                template.ShouldBe("UNIQUEIDENTIFIER");
            }
        }

        [TestFixture]
        public class DateTimeTests : SqlServerCeTypeMapTests
        {
            [Test]
            public void ItMapsTimeToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Time, size: null, precision: null);

                template.ShouldBe("DATETIME");
            }

            [Test]
            public void ItMapsDateToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Date, size: null, precision: null);

                template.ShouldBe("DATETIME");
            }

            [Test]
            public void ItMapsDatetimeToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.DateTime, size: null, precision: null);

                template.ShouldBe("DATETIME");
            }
        }

        [TestFixture]
        public class XmlTests : SqlServerCeTypeMapTests
        {
            [Test]
            public void ItMapsXmlToNtext()
            {
                var template = TypeMap.GetTypeMap(DbType.Xml, size: null, precision: null);

                template.ShouldBe("NTEXT");
            }
        }
    }
}
