using System;
using System.Data;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2000
{
    [TestFixture]
    public abstract class SqlServer2000TypeMapTests
    {
        protected SqlServer2000TypeMap TypeMap { get; private set; }

        [SetUp]
        public void Setup()
        {
            TypeMap = new SqlServer2000TypeMap();
        }

        [TestFixture]
        public class AnsistringTests : SqlServer2000TypeMapTests
        {
            [Test]
            public void it_maps_ansistring_by_default_to_varchar_255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, 0, 0);

                template.ShouldBe("VARCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            public void it_maps_ansistring_with_size_to_varchar_of_size(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size, 0);

                template.ShouldBe(string.Format("VARCHAR({0})", size));
            }

            [Test]
            [TestCase(8001)]
            [TestCase(2147483647)]
            public void it_maps_ansistring_with_size_above_8000_to_text(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size, 0);

                template.ShouldBe("TEXT");
            }
        }

        [TestFixture]
        public class AnsistringFixedLengthTests : SqlServer2000TypeMapTests
        {
            [Test]
            public void it_maps_ansistring_fixed_length_by_default_to_char_255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, 0, 0);

                template.ShouldBe("CHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            public void it_maps_ansistring_fixed_length_with_size_to_char_of_size(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, size, 0);

                template.ShouldBe(string.Format("CHAR({0})", size));
            }

            [Test]
            public void it_throws_if_ansistring_fixed_length_has_size_above_8000()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, 8001, 0));
            }
        }

        [TestFixture]
        public class StringTests : SqlServer2000TypeMapTests
        {
            [Test]
            public void it_maps_string_by_default_to_nvarchar_255()
            {
                var template = TypeMap.GetTypeMap(DbType.String, 0, 0);

                template.ShouldBe("NVARCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            public void it_maps_string_with_size_to_nvarchar_of_size(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.String, size, 0);

                template.ShouldBe(string.Format("NVARCHAR({0})", size));
            }

            [Test]
            [TestCase(4001)]
            [TestCase(1073741823)]
            public void it_maps_string_with_size_above_4000_to_ntext(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.String, size, 0);

                template.ShouldBe("NTEXT");
            }

            [Test]
            public void it_maps_string_with_size_above_1073741823_to_ntext_to_allow_int_maxvalue_convention()
            {
                var template = TypeMap.GetTypeMap(DbType.String, int.MaxValue, 0);

                template.ShouldBe("NTEXT");
            }
        }

        [TestFixture]
        public class StringFixedLengthTests : SqlServer2000TypeMapTests
        {
            [Test]
            public void it_maps_string_fixed_length_by_default_to_nchar_255()
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, 0, 0);

                template.ShouldBe("NCHAR(255)");
            }


            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            public void it_maps_string_fixed_length_with_size_to_nchar_of_size(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, size, 0);

                template.ShouldBe(string.Format("NCHAR({0})", size));
            }

            [Test]
            public void it_throws_if_string_fixed_length_has_size_above_4000()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.StringFixedLength, 4001, 0));
            }
        }

        [TestFixture]
        public class BinaryTests : SqlServer2000TypeMapTests
        {
            [Test]
            public void it_maps_binary_by_default_to_varbinary_8000()
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, 0, 0);

                template.ShouldBe("VARBINARY(8000)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            public void it_maps_binary_with_size_to_varbinary_of_size(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, 0);

                template.ShouldBe(string.Format("VARBINARY({0})", size));
            }

            [Test]
            [TestCase(8001)]
            [TestCase(int.MaxValue)]
            public void it_maps_binary_with_size_above_8000_to_image(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, 0);

                template.ShouldBe("IMAGE");
            }
        }

        [TestFixture]
        public class NumericTests : SqlServer2000TypeMapTests
        {
            [Test]
            public void it_maps_boolean_to_bit()
            {
                var template = TypeMap.GetTypeMap(DbType.Boolean, 0, 0);

                template.ShouldBe("BIT");
            }

            [Test]
            public void it_maps_byte_to_tinyint()
            {
                var template = TypeMap.GetTypeMap(DbType.Byte, 0, 0);

                template.ShouldBe("TINYINT");
            }

            [Test]
            public void it_maps_int16_to_smallint()
            {
                var template = TypeMap.GetTypeMap(DbType.Int16, 0, 0);

                template.ShouldBe("SMALLINT");
            }

            [Test]
            public void it_maps_int32_to_int()
            {
                var template = TypeMap.GetTypeMap(DbType.Int32, 0, 0);

                template.ShouldBe("INT");
            }

            [Test]
            public void it_maps_int64_to_bigint()
            {
                var template = TypeMap.GetTypeMap(DbType.Int64, 0, 0);

                template.ShouldBe("BIGINT");
            }

            [Test]
            public void it_maps_single_to_real()
            {
                var template = TypeMap.GetTypeMap(DbType.Single, 0, 0);

                template.ShouldBe("REAL");
            }

            [Test]
            public void it_maps_double_to_double_precision()
            {
                var template = TypeMap.GetTypeMap(DbType.Double, 0, 0);

                template.ShouldBe("DOUBLE PRECISION");
            }

            [Test]
            public void it_maps_currency_to_money()
            {
                var template = TypeMap.GetTypeMap(DbType.Currency, 0, 0);

                template.ShouldBe("MONEY");
            }

            [Test]
            public void it_maps_decimal_by_default_to_decimal_19_5()
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, 0, 0);

                template.ShouldBe("DECIMAL(19,5)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(20)]
            [TestCase(38)]
            public void it_maps_decimal_with_precision_to_decimal(int precision)
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, precision, 1);

                template.ShouldBe(string.Format("DECIMAL({0},1)", precision));
            }

            [Test]
            public void it_throws_if_decimal_precision_is_above_38()
            {
                Should.Throw<NotSupportedException>(
                    () => TypeMap.GetTypeMap(DbType.Decimal, 39, 0));
            }
        }

        [TestFixture]
        public class GuidTests : SqlServer2000TypeMapTests
        {
            [Test]
            public void it_maps_guid_to_uniqueidentifier()
            {
                var template = TypeMap.GetTypeMap(DbType.Guid, 0, 0);

                template.ShouldBe("UNIQUEIDENTIFIER");
            }
        }

        [TestFixture]
        public class DateTimeTests : SqlServer2000TypeMapTests
        {
            [Test]
            public void it_maps_time_to_datetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Time, 0, 0);

                template.ShouldBe("DATETIME");
            }

            [Test]
            public void it_maps_date_to_datetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Date, 0, 0);

                template.ShouldBe("DATETIME");
            }

            [Test]
            public void it_maps_datetime_to_datetime()
            {
                var template = TypeMap.GetTypeMap(DbType.DateTime, 0, 0);

                template.ShouldBe("DATETIME");
            }
        }
    }
}