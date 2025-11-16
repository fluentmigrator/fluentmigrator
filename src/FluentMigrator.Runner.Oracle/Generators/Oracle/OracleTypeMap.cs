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
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Oracle
{
    /// <summary>
    /// Provides Oracle-specific mappings between <see cref="System.Data.DbType"/> and SQL types.
    /// </summary>
    public class OracleTypeMap : TypeMapBase, IOracleTypeMap
    {
        // See https://docs.oracle.com/cd/B28359_01/server.111/b28320/limits001.htm#i287903
        // and http://docs.oracle.com/cd/B19306_01/server.102/b14220/datatype.htm#i13446
        // for limits in Oracle data types.
        /// <summary>
        /// Specifies the maximum capacity, in characters, for ANSI string types in Oracle databases.
        /// </summary>
        /// <remarks>
        /// This constant is used to define the upper limit for the size of ANSI string columns
        /// when mapping <see cref="System.Data.DbType.AnsiString"/> to Oracle SQL types.
        /// The value is based on Oracle's VARCHAR2 data type limitations.
        /// </remarks>
        public const int AnsiStringCapacity = 4000;
        /// <summary>
        /// Specifies the maximum capacity, in characters, for ANSI text types in Oracle databases.
        /// </summary>
        /// <remarks>
        /// This constant is used to define the upper limit for the size of ANSI text columns
        /// when mapping <see cref="System.Data.DbType.AnsiString"/> to Oracle SQL types.
        /// The value <c>int.MaxValue</c> represents the unbounded, theoretical maximum size for large text data in Oracle databases.
        /// </remarks>
        public const int AnsiTextCapacity = int.MaxValue;
        /// <summary>
        /// Represents the maximum capacity for a BLOB (Binary Large Object) in Oracle databases.
        /// </summary>
        /// <remarks>
        /// This constant is used to define the maximum size for BLOB data types in Oracle-specific mappings.
        /// It is set to <see cref="int.MaxValue"/>, which indicates the largest possible size for a BLOB.
        /// </remarks>
        public const int BlobCapacity = int.MaxValue;
        /// <summary>
        /// Represents the maximum capacity, in characters, for Oracle CHAR data types.
        /// </summary>
        /// <remarks>
        /// This constant is used to define the maximum size for Oracle CHAR types when mapping
        /// <see cref="System.Data.DbType.StringFixedLength"/> to Oracle SQL types.
        /// </remarks>
        public const int CharStringCapacity = 2000;
        /// <summary>
        /// Represents the maximum precision for a decimal type in Oracle databases.
        /// </summary>
        public const int DecimalCapacity = 38;
        /// <summary>
        /// Represents the maximum capacity, in bytes, for the Oracle RAW data type.
        /// </summary>
        /// <remarks>
        /// This constant is used to define the maximum size of binary data that can be stored in a RAW column.
        /// </remarks>
        public const int RawCapacity = 2000;
        /// <summary>
        /// Represents the maximum capacity for Unicode string types in Oracle databases.
        /// </summary>
        public const int UnicodeStringCapacity = 4000;
        /// <summary>
        /// Represents the maximum capacity for Unicode text in Oracle databases.
        /// </summary>
        /// <remarks>
        /// This constant is used to define the size limit for NCLOB data types in Oracle,
        /// which are used to store large Unicode text data.
        /// </remarks>
        public const int UnicodeTextCapacity = int.MaxValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleTypeMap"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets up the Oracle-specific mappings between 
        /// <see cref="System.Data.DbType"/> and SQL types by invoking the <see cref="SetupTypeMaps"/> method.
        /// </remarks>
        public OracleTypeMap()
        {
            SetupTypeMaps();
        }

        /// <inheritdoc />
        protected sealed override void SetupTypeMaps()
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
            SetTypeMap(DbType.DateTime2, "TIMESTAMP(4)");
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
