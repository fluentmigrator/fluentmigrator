using System;
using System.Data;

using FluentMigrator.Runner.Generators.SqlAnywhere;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlAnywhere
{
    [TestFixture]
    public abstract class SqlAnywhere16TypeMapTests
    {
        protected SqlAnywhere16TypeMap TypeMap { get; private set; }

        [SetUp]
        public void Setup()
        {
            TypeMap = new SqlAnywhere16TypeMap();
        }

        [TestFixture]
        public class AnsistringTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsAnsistringByDefaultToVarchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, 0, 0);

                template.ShouldBe("VARCHAR(255)");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            public void ItMapsAnsistringWithSizeToVarcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size, 0);

                template.ShouldBe(string.Format("VARCHAR({0})", size));
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            [TestCase(8001)]
            [TestCase(2147483647)]
            public void ItMapsAnsistringWithSizeAbove8000ToText(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size, 0);

                template.ShouldBe("TEXT");
            }
        }

        [TestFixture]
        public class AnsistringFixedLengthTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsAnsistringFixedLengthByDefaultToChar255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, 0, 0);

                template.ShouldBe("CHAR(255)");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            public void ItMapsAnsistringFixedLengthWithSizeToCharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, size, 0);

                template.ShouldBe(string.Format("CHAR({0})", size));
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItThrowsIfAnsistringFixedLengthHasSizeAbove8000()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, 8001, 0));
            }
        }

        [TestFixture]
        public class StringTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsStringByDefaultToNvarchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.String, 0, 0);

                template.ShouldBe("NVARCHAR(255)");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            [TestCase(1)]
            [TestCase(4000)]
            public void ItMapsStringWithSizeToNvarcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.String, size, 0);

                template.ShouldBe(string.Format("NVARCHAR({0})", size));
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            [TestCase(4001)]
            [TestCase(1073741823)]
            public void ItMapsStringWithSizeAbove4000ToNtext(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.String, size, 0);

                template.ShouldBe("NTEXT");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsStringWithSizeAbove1073741823ToNtextToAllowIntMaxvalueConvention()
            {
                var template = TypeMap.GetTypeMap(DbType.String, int.MaxValue, 0);

                template.ShouldBe("NTEXT");
            }
        }

        [TestFixture]
        public class StringFixedLengthTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsStringFixedLengthByDefaultToNchar255()
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, 0, 0);

                template.ShouldBe("NCHAR(255)");
            }


            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            [TestCase(1)]
            [TestCase(4000)]
            public void ItMapsStringFixedLengthWithSizeToNcharOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, size, 0);

                template.ShouldBe(string.Format("NCHAR({0})", size));
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItThrowsIfStringFixedLengthHasSizeAbove4000()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.StringFixedLength, 4001, 0));
            }
        }

        [TestFixture]
        public class BinaryTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsBinaryByDefaultToVarbinary8000()
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, 0, 0);

                template.ShouldBe("VARBINARY(8000)");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            public void ItMapsBinaryWithSizeToVarbinaryOfSize(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, 0);

                template.ShouldBe(string.Format("VARBINARY({0})", size));
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            [TestCase(8001)]
            [TestCase(int.MaxValue)]
            public void ItMapsBinaryWithSizeAbove8000ToImage(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, 0);

                template.ShouldBe("IMAGE");
            }
        }

        [TestFixture]
        public class NumericTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsBooleanToBit()
            {
                var template = TypeMap.GetTypeMap(DbType.Boolean, 0, 0);

                template.ShouldBe("BIT");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsByteToTinyint()
            {
                var template = TypeMap.GetTypeMap(DbType.Byte, 0, 0);

                template.ShouldBe("TINYINT");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsInt16ToSmallint()
            {
                var template = TypeMap.GetTypeMap(DbType.Int16, 0, 0);

                template.ShouldBe("SMALLINT");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsInt32ToInteger()
            {
                var template = TypeMap.GetTypeMap(DbType.Int32, 0, 0);

                template.ShouldBe("INTEGER");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsInt64ToBigint()
            {
                var template = TypeMap.GetTypeMap(DbType.Int64, 0, 0);

                template.ShouldBe("BIGINT");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsSingleToReal()
            {
                var template = TypeMap.GetTypeMap(DbType.Single, 0, 0);

                template.ShouldBe("REAL");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsDoubleToDoublePrecision()
            {
                var template = TypeMap.GetTypeMap(DbType.Double, 0, 0);

                template.ShouldBe("DOUBLE PRECISION");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsCurrencyToMoney()
            {
                var template = TypeMap.GetTypeMap(DbType.Currency, 0, 0);

                template.ShouldBe("MONEY");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsDecimalByDefaultToDecimal306()
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, 0, 0);

                template.ShouldBe("DECIMAL(30,6)");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            [TestCase(1)]
            [TestCase(20)]
            [TestCase(38)]
            public void ItMapsDecimalWithPrecisionToDecimal(int precision)
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, precision, 1);

                template.ShouldBe(string.Format("DECIMAL({0},1)", precision));
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItThrowsIfDecimalPrecisionIsAbove127()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.Decimal, 128, 0));
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsVarnumericByDefaultToNumeric195()
            {
                var template = TypeMap.GetTypeMap(DbType.VarNumeric, 0, 0);

                template.ShouldBe("NUMERIC(30,6)");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            [TestCase(1)]
            [TestCase(20)]
            [TestCase(38)]
            public void ItMapsVarnumericWithPrecisionToNumeric(int precision)
            {
                var template = TypeMap.GetTypeMap(DbType.VarNumeric, precision, 1);

                template.ShouldBe(string.Format("NUMERIC({0},1)", precision));
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItThrowsIfVarnumericPrecisionIsAbove127()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.VarNumeric, 128, 0));
            }
        }

        [TestFixture]
        public class GuidTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsGUIDToUniqueidentifier()
            {
                var template = TypeMap.GetTypeMap(DbType.Guid, 0, 0);

                template.ShouldBe("UNIQUEIDENTIFIER");
            }
        }

        [TestFixture]
        public class DateTimeTests : SqlAnywhere16TypeMapTests
        {
            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsTimeToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Time, 0, 0);

                template.ShouldBe("DATETIME");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsDateToDate()
            {
                var template = TypeMap.GetTypeMap(DbType.Date, 0, 0);

                template.ShouldBe("DATE");
            }

            [Test]
            [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("TypeMap")]
            public void ItMapsDatetimeToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.DateTime, 0, 0);

                template.ShouldBe("DATETIME");
            }
        }
    }
}
