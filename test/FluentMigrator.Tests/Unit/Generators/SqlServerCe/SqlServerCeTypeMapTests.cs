using System;
using System.Data;

using FluentMigrator.Runner.Generators.SqlServer;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServerCe
{
    [TestFixture]
    public abstract class SqlServerCeTypeMapTests
    {
        protected SqlServerCeTypeMap TypeMap { get; private set; }

        [SetUp]
        public void Setup()
        {
            TypeMap = new SqlServerCeTypeMap();
        }

        [TestFixture]
        public class AnsistringTests : SqlServerCeTypeMapTests
        {
            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsAnsistringByDefaultToNvarchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, 0, 0);

                template.ShouldBe("NVARCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(2000)]
            [TestCase(4000)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsAnsistringWithSizeToNvarcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size, 0);

                template.ShouldBe(string.Format("NVARCHAR({0})", size));
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsAnsistringWithMaxSizeToNtext()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, int.MaxValue, 0);
                template.ShouldBe("NTEXT");
            }
        }

        [TestFixture]
        public class AnsistringFixedLengthTests : SqlServerCeTypeMapTests
        {
            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsAnsistringFixedLengthByDefaultToNchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, 0, 0);

                template.ShouldBe("NCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(2000)]
            [TestCase(4000)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsAnsistringFixedLengthWithSizeToNcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, size, 0);

                template.ShouldBe(string.Format("NCHAR({0})", size));
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItThrowsIfAnsistringFixedLengthHasSizeAbove4000()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, 4001, 0));
            }
        }

        [TestFixture]
        public class StringTests : SqlServerCeTypeMapTests
        {
            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsStringByDefaultToNvarchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.String, 0, 0);

                template.ShouldBe("NVARCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsStringWithSizeToNvarcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.String, size, 0);

                template.ShouldBe(string.Format("NVARCHAR({0})", size));
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItThrowsIfStringHasSizeAbove4000()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, 4001, 0));
            }
        }

        [TestFixture]
        public class StringFixedLengthTests : SqlServerCeTypeMapTests
        {
            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsStringFixedLengthByDefaultToNchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, 0, 0);

                template.ShouldBe("NCHAR(255)");
            }


            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsStringFixedLengthWithSizeToNcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, size, 0);

                template.ShouldBe(string.Format("NCHAR({0})", size));
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItThrowsIfStringFixedLengthHasSizeAbove4000()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.StringFixedLength, 4001, 0));
            }
        }

        [TestFixture]
        public class BinaryTests : SqlServerCeTypeMapTests
        {
            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsBinaryByDefaultToVarbinary8000()
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, 0, 0);

                template.ShouldBe("VARBINARY(8000)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsBinaryWithSizeToVarbinaryOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, 0);

                template.ShouldBe(string.Format("VARBINARY({0})", size));
            }

            [Test]
            [TestCase(8001)]
            [TestCase(1073741823)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsBinaryWithSizeAbove8000ToImage(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, 0);

                template.ShouldBe("IMAGE");
            }

            [Test]
            [TestCase(1073741824)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItThrowsIfBinarySizeIsAbove1073741823(int size)
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.Binary, size, 0));
            }
        }

        [TestFixture]
        public class NumericTests : SqlServerCeTypeMapTests
        {
            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsBooleanToBit()
            {
                var template = TypeMap.GetTypeMap(DbType.Boolean, 0, 0);

                template.ShouldBe("BIT");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsByteToTinyint()
            {
                var template = TypeMap.GetTypeMap(DbType.Byte, 0, 0);

                template.ShouldBe("TINYINT");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsInt16ToSmallint()
            {
                var template = TypeMap.GetTypeMap(DbType.Int16, 0, 0);

                template.ShouldBe("SMALLINT");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsInt32ToInt()
            {
                var template = TypeMap.GetTypeMap(DbType.Int32, 0, 0);

                template.ShouldBe("INT");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsInt64ToBigint()
            {
                var template = TypeMap.GetTypeMap(DbType.Int64, 0, 0);

                template.ShouldBe("BIGINT");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsSingleToReal()
            {
                var template = TypeMap.GetTypeMap(DbType.Single, 0, 0);

                template.ShouldBe("REAL");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsDoubleToDoublePrecision()
            {
                var template = TypeMap.GetTypeMap(DbType.Double, 0, 0);

                template.ShouldBe("FLOAT");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsCurrencyToMoney()
            {
                var template = TypeMap.GetTypeMap(DbType.Currency, 0, 0);

                template.ShouldBe("MONEY");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsDecimalByDefaultToDecimal195()
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, 0, 0);

                template.ShouldBe("NUMERIC(19,5)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(20)]
            [TestCase(38)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsDecimalWithPrecisionToDecimal(int precision)
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, precision, 1);

                template.ShouldBe(string.Format("NUMERIC({0},1)", precision));
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItThrowsIfDecimalPrecisionIsAbove38()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.Decimal, 39, 0));
            }
        }

        [TestFixture]
        public class GuidTests : SqlServerCeTypeMapTests
        {
            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsGUIDToUniqueidentifier()
            {
                var template = TypeMap.GetTypeMap(DbType.Guid, 0, 0);

                template.ShouldBe("UNIQUEIDENTIFIER");
            }
        }

        [TestFixture]
        public class DateTimeTests : SqlServerCeTypeMapTests
        {
            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsTimeToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Time, 0, 0);

                template.ShouldBe("DATETIME");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsDateToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Date, 0, 0);

                template.ShouldBe("DATETIME");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsDatetimeToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.DateTime, 0, 0);

                template.ShouldBe("DATETIME");
            }
        }

        [TestFixture]
        public class XmlTests : SqlServerCeTypeMapTests
        {
            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void ItMapsXmlToNtext()
            {
                var template = TypeMap.GetTypeMap(DbType.Xml, 0, 0);

                template.ShouldBe("NTEXT");
            }
        }
    }
}
