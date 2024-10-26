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

using System.Data;

using FluentMigrator.Runner.Generators.Oracle;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    [TestFixture]
    [Category("Oracle")]
    [Category("Generator")]
    [Category("TypeMap")]
    public class OracleTypeMapTests
    {
        private OracleTypeMap _typeMap;

        [SetUp]
        public void SetUp()
        {
            _typeMap = new OracleTypeMap();
        }

        // See https://docs.oracle.com/cd/B28359_01/server.111/b28320/limits001.htm#i287903
        // and http://docs.oracle.com/cd/B19306_01/server.102/b14220/datatype.htm#i13446
        // for limits in Oracle data types.
        [Test]
        public void AnsiStringDefaultIsVarchar2_255()
        {
            _typeMap.GetTypeMap(DbType.AnsiString, size: null, precision: null).ShouldBe("VARCHAR2(255 CHAR)");
        }

        [Test]
        public void AnsiStringOfSizeIsVarchar2OfSize()
        {
            _typeMap.GetTypeMap(DbType.AnsiString, size: 4000, precision: null).ShouldBe("VARCHAR2(4000 CHAR)");
        }

        [Test]
        public void AnsiStringOver4000IsClob()
        {
            _typeMap.GetTypeMap(DbType.AnsiString, size: 4001, precision: null).ShouldBe("CLOB");
        }

        [Test]
        public void AnsiStringFixedDefaultIsChar_255()
        {
            _typeMap.GetTypeMap(DbType.AnsiStringFixedLength, size: null, precision: null).ShouldBe("CHAR(255 CHAR)");
        }

        [Test]
        public void AnsiStringFixedOfSizeIsCharOfSize()
        {
            _typeMap.GetTypeMap(DbType.AnsiStringFixedLength, size: 2000, precision: null).ShouldBe("CHAR(2000 CHAR)");
        }


        [Test]
        public void BinaryDefaultIsRaw_2000()
        {
            _typeMap.GetTypeMap(DbType.Binary, size: null, precision: null).ShouldBe("RAW(2000)");
        }

        [Test]
        public void BinaryOfSizeIsRawOfSize()
        {
            _typeMap.GetTypeMap(DbType.Binary, size: 2000, precision: null).ShouldBe("RAW(2000)");
        }


        [Test]
        public void BinaryOver2000IsBlob()
        {
            _typeMap.GetTypeMap(DbType.Binary, size: 2001, precision: null).ShouldBe("BLOB");
        }

        [Test]
        public void BooleanIsNumber()
        {
            _typeMap.GetTypeMap(DbType.Boolean, size: null, precision: null).ShouldBe("NUMBER(1,0)");
        }

        [Test]
        public void ByteIsNumber()
        {
            _typeMap.GetTypeMap(DbType.Byte, size: null, precision: null).ShouldBe("NUMBER(3,0)");
        }

        [Test]
        public void CurrencyIsNumber()
        {
            _typeMap.GetTypeMap(DbType.Currency, size: null, precision: null).ShouldBe("NUMBER(19,4)");
        }

        [Test]
        public void DateIsDate()
        {
            _typeMap.GetTypeMap(DbType.Date, size: null, precision: null).ShouldBe("DATE");
        }

        [Test]
        public void DateTimeIsTimestamp()
        {
            _typeMap.GetTypeMap(DbType.DateTime, size: null, precision: null).ShouldBe("TIMESTAMP(4)");
        }

        [Test]
        public void DateTimeOffsetIsTimestampWithTimeZone()
        {
            _typeMap.GetTypeMap(DbType.DateTimeOffset, size: null, precision: null).ShouldBe("TIMESTAMP(4) WITH TIME ZONE");
        }

        [Test]
        public void DecimalDefaultIsNumber()
        {
            _typeMap.GetTypeMap(DbType.Decimal, size: null, precision: null).ShouldBe("NUMBER(19,5)");
        }

        [Test]
        public void DecimalOfPrecisionIsNumberWithPrecision()
        {
            _typeMap.GetTypeMap(DbType.Decimal, size: 8, precision: 3).ShouldBe("NUMBER(8,3)");
        }

        [Test]
        public void DoubleIsDouble()
        {
            _typeMap.GetTypeMap(DbType.Double, size: null, precision: null).ShouldBe("DOUBLE PRECISION");
        }

        [Test]
        public void GuidIsRaw()
        {
            _typeMap.GetTypeMap(DbType.Guid, size: null, precision: null).ShouldBe("RAW(16)");
        }

        [Test]
        public void Int16IsNumber()
        {
            _typeMap.GetTypeMap(DbType.Int16, size: null, precision: null).ShouldBe("NUMBER(5,0)");
        }

        [Test]
        public void In32IsNumber()
        {
            _typeMap.GetTypeMap(DbType.Int32, size: null, precision: null).ShouldBe("NUMBER(10,0)");
        }

        [Test]
        public void Int64IsNumber()
        {
            _typeMap.GetTypeMap(DbType.Int64, size: null, precision: null).ShouldBe("NUMBER(19,0)");
        }

        [Test]
        public void SingleIsFloat()
        {
            _typeMap.GetTypeMap(DbType.Single, size: null, precision: null).ShouldBe("FLOAT(24)");
        }

        [Test]
        public void StringFixedLengthDefaultIsNChar_255()
        {
            _typeMap.GetTypeMap(DbType.StringFixedLength, size: null, precision: null).ShouldBe("NCHAR(255)");
        }

        [Test]
        public void StringFixedLengthOfSizeIsNCharOfSize()
        {
            _typeMap.GetTypeMap(DbType.StringFixedLength, size: 2000, precision: null).ShouldBe("NCHAR(2000)");
        }


        [Test]
        public void StringDefaultIsNVarchar2_255()
        {
            _typeMap.GetTypeMap(DbType.String, size: null, precision: null).ShouldBe("NVARCHAR2(255)");
        }

        [Test]
        public void StringOfLengthIsNVarchar2Length()
        {
            _typeMap.GetTypeMap(DbType.String, size: 4000, precision: null).ShouldBe("NVARCHAR2(4000)");
        }

        [Test]
        public void TimeIsDate()
        {
            _typeMap.GetTypeMap(DbType.Time, size: null, precision: null).ShouldBe("DATE");
        }

        [Test]
        public void XmlIsXmltype()
        {
            _typeMap.GetTypeMap(DbType.Xml, size: null, precision: null).ShouldBe("XMLTYPE");
        }
    }
}
