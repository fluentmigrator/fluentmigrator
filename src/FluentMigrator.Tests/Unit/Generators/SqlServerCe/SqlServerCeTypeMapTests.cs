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
            public void it_maps_ansistring_by_default_to_nvarchar_255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, 0, 0);

                template.ShouldBe("NVARCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(2000)]
            [TestCase(4000)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_ansistring_with_size_to_nvarchar_of_size(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiString, size, 0);

                template.ShouldBe(string.Format("NVARCHAR({0})", size));
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_ansistring_with_max_size_to_ntext()
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
            public void it_maps_ansistring_fixed_length_by_default_to_nchar_255()
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, 0, 0);

                template.ShouldBe("NCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(2000)]
            [TestCase(4000)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_ansistring_fixed_length_with_size_to_nchar_of_size(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.AnsiStringFixedLength, size, 0);

                template.ShouldBe(string.Format("NCHAR({0})", size));
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_throws_if_ansistring_fixed_length_has_size_above_4000()
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
            public void it_maps_string_by_default_to_nvarchar_255()
            {
                var template = TypeMap.GetTypeMap(DbType.String, 0, 0);

                template.ShouldBe("NVARCHAR(255)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_string_with_size_to_nvarchar_of_size(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.String, size, 0);

                template.ShouldBe(string.Format("NVARCHAR({0})", size));
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_throws_if_string_has_size_above_4000()
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
            public void it_maps_string_fixed_length_by_default_to_nchar_255()
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, 0, 0);

                template.ShouldBe("NCHAR(255)");
            }


            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_string_fixed_length_with_size_to_nchar_of_size(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.StringFixedLength, size, 0);

                template.ShouldBe(string.Format("NCHAR({0})", size));
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_throws_if_string_fixed_length_has_size_above_4000()
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
            public void it_maps_binary_by_default_to_varbinary_8000()
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, 0, 0);

                template.ShouldBe("VARBINARY(8000)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(4000)]
            [TestCase(8000)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_binary_with_size_to_varbinary_of_size(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, 0);

                template.ShouldBe(string.Format("VARBINARY({0})", size));
            }

            [Test]
            [TestCase(8001)]
            [TestCase(1073741823)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_binary_with_size_above_8000_to_image(int size)
            {
                var template = TypeMap.GetTypeMap(DbType.Binary, size, 0);

                template.ShouldBe("IMAGE");
            }

            [Test]
            [TestCase(1073741824)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_throws_if_binary_size_is_above_1073741823(int size)
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
            public void it_maps_boolean_to_bit()
            {
                var template = TypeMap.GetTypeMap(DbType.Boolean, 0, 0);

                template.ShouldBe("BIT");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_byte_to_tinyint()
            {
                var template = TypeMap.GetTypeMap(DbType.Byte, 0, 0);

                template.ShouldBe("TINYINT");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_int16_to_smallint()
            {
                var template = TypeMap.GetTypeMap(DbType.Int16, 0, 0);

                template.ShouldBe("SMALLINT");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_int32_to_int()
            {
                var template = TypeMap.GetTypeMap(DbType.Int32, 0, 0);

                template.ShouldBe("INT");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_int64_to_bigint()
            {
                var template = TypeMap.GetTypeMap(DbType.Int64, 0, 0);

                template.ShouldBe("BIGINT");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_single_to_real()
            {
                var template = TypeMap.GetTypeMap(DbType.Single, 0, 0);

                template.ShouldBe("REAL");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_double_to_double_precision()
            {
                var template = TypeMap.GetTypeMap(DbType.Double, 0, 0);

                template.ShouldBe("FLOAT");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_currency_to_money()
            {
                var template = TypeMap.GetTypeMap(DbType.Currency, 0, 0);

                template.ShouldBe("MONEY");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_decimal_by_default_to_decimal_19_5()
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, 0, 0);

                template.ShouldBe("NUMERIC(19,5)");
            }

            [Test]
            [TestCase(1)]
            [TestCase(20)]
            [TestCase(38)]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_decimal_with_precision_to_decimal(int precision)
            {
                var template = TypeMap.GetTypeMap(DbType.Decimal, precision, 1);

                template.ShouldBe(string.Format("NUMERIC({0},1)", precision));
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_throws_if_decimal_precision_is_above_38()
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
            public void it_maps_guid_to_uniqueidentifier()
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
            public void it_maps_time_to_datetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Time, 0, 0);

                template.ShouldBe("DATETIME");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_date_to_datetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Date, 0, 0);

                template.ShouldBe("DATETIME");
            }

            [Test]
            [Category("SqlServerCe"), Category("Generator"), Category("TypeMap")]
            public void it_maps_datetime_to_datetime()
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
            public void it_maps_xml_to_ntext()
            {
                var template = TypeMap.GetTypeMap(DbType.Xml, 0, 0);

                template.ShouldBe("NTEXT");
            }
        }
    }
}
