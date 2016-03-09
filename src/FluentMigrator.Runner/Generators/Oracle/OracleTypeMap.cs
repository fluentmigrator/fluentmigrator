#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleTypeMap : TypeMapBase
    {
        // See https://docs.oracle.com/cd/B28359_01/server.111/b28320/limits001.htm#i287903 
        // and http://docs.oracle.com/cd/B19306_01/server.102/b14220/datatype.htm#i13446
        // for limits in Oracle data types. 
        public const int AnsiStringCapacity = 4000;
        public const int AnsiTextCapacity = int.MaxValue;
        public const int BlobCapacity = int.MaxValue;
        public const int CharStringCapacity = 2000;
        public const int DecimalCapacity = 38;
        public const int RawCapacity = 2000;
        public const int UnicodeStringCapacity = 4000;
        public const int UnicodeTextCapacity = int.MaxValue;
        
        protected override void SetupTypeMaps()
        {
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255 CHAR)");
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size CHAR)", CharStringCapacity);
            SetTypeMap(DbType.AnsiString, "VARCHAR2(255 CHAR)");
            SetTypeMap(DbType.AnsiString, "VARCHAR2($size CHAR)", AnsiStringCapacity);
            SetTypeMap(DbType.AnsiString, "CLOB", AnsiTextCapacity);
            SetTypeMap(DbType.Binary, "RAW(2000)");
            SetTypeMap(DbType.Binary, "RAW($size)", RawCapacity);
            SetTypeMap(DbType.Binary, "BLOB", BlobCapacity);
            SetTypeMap(DbType.Boolean, "NUMBER(1,0)");
            SetTypeMap(DbType.Byte, "NUMBER(3,0)");
            SetTypeMap(DbType.Currency, "NUMBER(19,4)");
            SetTypeMap(DbType.Date, "DATE");
            SetTypeMap(DbType.DateTime, "TIMESTAMP(4)");
            SetTypeMap(DbType.DateTimeOffset, "TIMESTAMP(4) WITH TIME ZONE");
            SetTypeMap(DbType.Decimal, "NUMBER(19,5)");
            SetTypeMap(DbType.Decimal, "NUMBER($size,$precision)", DecimalCapacity);
            SetTypeMap(DbType.Double, "DOUBLE PRECISION");
            SetTypeMap(DbType.Guid, "RAW(16)");
            SetTypeMap(DbType.Int16, "NUMBER(5,0)");
            SetTypeMap(DbType.Int32, "NUMBER(10,0)");
            SetTypeMap(DbType.Int64, "NUMBER(19,0)");
            SetTypeMap(DbType.Single, "FLOAT(24)");
            SetTypeMap(DbType.StringFixedLength, "NCHAR(255)");
            SetTypeMap(DbType.StringFixedLength, "NCHAR($size)", CharStringCapacity);
            SetTypeMap(DbType.String, "NVARCHAR2(255)");
            SetTypeMap(DbType.String, "NVARCHAR2($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.String, "NCLOB", UnicodeTextCapacity);
            SetTypeMap(DbType.Time, "DATE");
            SetTypeMap(DbType.Xml, "XMLTYPE");
        }
    }
}
