using System;
using System.Data;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    public abstract class SqlServer2005TypeMapTests
    {
        protected SqlServer2005TypeMap TypeMap { get; private set; }

        [SetUp]
        public void Setup()
        {
            TypeMap = new SqlServer2005TypeMap();
        }

        [TestFixture]
        public class AnsistringTests : SqlServer2005TypeMapTests
        {
            [Test]
            public void ItMapsAnsistringByDefaultToVarchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, 0, 0);

                template.ShouldBe("VARCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            public void ItMapsAnsistringWithSizeToVarcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size, 0);

                template.ShouldBe(string.Format("VARCHAR({0})", size));
            }

            [Test]
            [TestCase(8001)]
            [TestCase(2147483647)]
            public void ItMapsAnsistringWithSizeAbove8000ToVarcharMax(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size, 0);

                template.ShouldBe("VARCHAR(MAX)");
            }
        }

        [TestFixture]
        public class AnsistringFixedLengthTests : SqlServer2005TypeMapTests
        {
            [Test]
            public void ItMapsAnsistringFixedLengthByDefaultToChar255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, 0, 0);

                template.ShouldBe("CHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            public void ItMapsAnsistringFixedLengthWithSizeToCharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, size, 0);

                template.ShouldBe(string.Format("CHAR({0})", size));
            }

            [Test]
            public void ItThrowsIfAnsistringFixedLengthHasSizeAbove8000()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, 8001, 0));
            }
        }

        [TestFixture]
        public class StringTests : SqlServer2005TypeMapTests
        {
            [Test]
            public void ItMapsStringByDefaultToNvarchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.String, 0, 0);

                template.ShouldBe("NVARCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            public void ItMapsStringWithSizeToNvarcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.String, size, 0);

                template.ShouldBe(string.Format("NVARCHAR({0})", size));
            }

            [Test]
            [TestCase(4001)]
            [TestCase(1073741823)]
            public void ItMapsStringWithSizeAbove4000ToNvarcharMax(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.String, size, 0);

                template.ShouldBe("NVARCHAR(MAX)");
            }

            [Test]
            public void ItMapsStringWithSizeAbove1073741823ToNvarcharMaxToAllowIntMaxvalueConvention()
            {
                var template = TypeMap.GetTypeMap(DbType.String, int.MaxValue, 0);

                template.ShouldBe("NVARCHAR(MAX)");
            }
        }

        [TestFixture]
        public class StringFixedLengthTests : SqlServer2005TypeMapTests
        {
            [Test]
            public void ItMapsStringFixedLengthByDefaultToNchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, 0, 0);

                template.ShouldBe("NCHAR(255)");
            }


            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            public void ItMapsStringFixedLengthWithSizeToNcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, size, 0);

                template.ShouldBe(string.Format("NCHAR({0})", size));
            }

            [Test]
            public void ItThrowsIfStringFixedLengthHasSizeAbove4000()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.StringFixedLength, 4001, 0));
            }
        }

        [TestFixture]
        public class BinaryTests : SqlServer2005TypeMapTests
        {
            [Test]
            public void ItMapsBinaryByDefaultToVarbinary8000()
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, 0, 0);

                template.ShouldBe("VARBINARY(8000)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            public void ItMapsBinaryWithSizeToVarbinaryOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, 0);

                template.ShouldBe(string.Format("VARBINARY({0})", size));
            }

            [Test]
            [TestCase(8001)]
            [TestCase(int.MaxValue)]
            public void ItMapsBinaryWithSizeAbove8000ToVarbinaryMax(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, 0);

                template.ShouldBe("VARBINARY(MAX)");
            }
        }

        [TestFixture]
        public class NumericTests : SqlServer2005TypeMapTests
        {
            [Test]
            public void ItMapsBooleanToBit()
            {
                var template = TypeMap.GetTypeMap(DbType.Boolean, 0, 0);

                template.ShouldBe("BIT");
            }

            [Test]
            public void ItMapsByteToTinyint()
            {
                var template = TypeMap.GetTypeMap(DbType.Byte, 0, 0);

                template.ShouldBe("TINYINT");
            }

            [Test]
            public void ItMapsInt16ToSmallint()
            {
                var template = TypeMap.GetTypeMap(DbType.Int16, 0, 0);

                template.ShouldBe("SMALLINT");
            }

            [Test]
            public void ItMapsInt32ToInt()
            {
                var template = TypeMap.GetTypeMap(DbType.Int32, 0, 0);

                template.ShouldBe("INT");
            }

            [Test]
            public void ItMapsInt64ToBigint()
            {
                var template = TypeMap.GetTypeMap(DbType.Int64, 0, 0);

                template.ShouldBe("BIGINT");
            }

            [Test]
            public void ItMapsSingleToReal()
            {
                var template = TypeMap.GetTypeMap(DbType.Single, 0, 0);

                template.ShouldBe("REAL");
            }

            [Test]
            public void ItMapsDoubleToDoublePrecision()
            {
                var template = TypeMap.GetTypeMap(DbType.Double, 0, 0);

                template.ShouldBe("DOUBLE PRECISION");
            }

            [Test]
            public void ItMapsCurrencyToMoney()
            {
                var template = TypeMap.GetTypeMap(DbType.Currency, 0, 0);

                template.ShouldBe("MONEY");
            }

            [Test]
            public void ItMapsDecimalByDefaultToDecimal195()
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, 0, 0);

                template.ShouldBe("DECIMAL(19,5)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(20)]
            [TestCase(38)]
            public void ItMapsDecimalWithPrecisionToDecimal(int precision)
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, precision, 1);

                template.ShouldBe(string.Format("DECIMAL({0},1)", precision));
            }

            [Test]
            public void ItThrowsIfDecimalPrecisionIsAbove38()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.Decimal, 39, 0));
            }
        }

        [TestFixture]
        public class GuidTests : SqlServer2005TypeMapTests
        {
            [Test]
            public void ItMapsGUIDToUniqueidentifier()
            {
                var template = TypeMap.GetTypeMap(DbType.Guid, 0, 0);

                template.ShouldBe("UNIQUEIDENTIFIER");
            }
        }

        [TestFixture]
        public class DateTimeTests : SqlServer2005TypeMapTests
        {
            [Test]
            public void ItMapsTimeToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Time, 0, 0);

                template.ShouldBe("DATETIME");
            }

            [Test]
            public void ItMapsDateToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Date, 0, 0);

                template.ShouldBe("DATETIME");
            }

            [Test]
            public void ItMapsDatetimeToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.DateTime, 0, 0);

                template.ShouldBe("DATETIME");
            }
        }

        [TestFixture]
        public class XmlTests : SqlServer2005TypeMapTests
        {
            [Test]
            public void ItMapsXmlToXml()
            {
                var template = TypeMap.GetTypeMap(DbType.Xml, 0, 0);

                template.ShouldBe("XML");
            }
        }
    }
}
