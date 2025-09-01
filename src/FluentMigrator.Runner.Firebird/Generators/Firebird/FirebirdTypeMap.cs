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

namespace FluentMigrator.Runner.Generators.Firebird
{
    /// <summary>
    /// Represents a specialized type map for the Firebird database.
    /// </summary>
    /// <remarks>
    /// This class provides mappings between .NET <see cref="DbType"/> values and their corresponding Firebird SQL types.
    /// It extends the <see cref="TypeMapBase"/> class and implements the <see cref="IFirebirdTypeMap"/> interface.
    /// </remarks>
    public sealed class FirebirdTypeMap : TypeMapBase, IFirebirdTypeMap
    {
        /// <summary>
        /// Represents the maximum capacity for the decimal type in Firebird SQL.
        /// </summary>
        /// <remarks>
        /// This constant is used to define the maximum size for the decimal type when mapping
        /// .NET <see cref="DbType.Decimal"/> to Firebird SQL.
        /// </remarks>
        private const int DecimalCapacity = 19;
        /// <summary>
        /// Represents the maximum size, in bytes, for a VARCHAR type in Firebird SQL.
        /// </summary>
        /// <remarks>
        /// This constant is used to define the upper limit for the size of VARCHAR fields
        /// when mapping .NET <see cref="DbType.AnsiString"/> to Firebird SQL.
        /// </remarks>
        private const int FirebirdMaxVarcharSize = 32765;
        
        /// <summary>
        /// Represents the maximum size, in bytes, for a fixed-length character type (CHAR) in Firebird.
        /// </summary>
        /// <remarks>
        /// This constant defines the upper limit for the size of a CHAR column in Firebird, which is 32,767 bytes.
        /// It is used in type mappings to ensure that the size of CHAR columns does not exceed this limit.
        /// </remarks>
        private const int FirebirdMaxCharSize = 32767;
        /// <summary>
        /// Represents the maximum size, in characters, for Unicode CHAR and VARCHAR types in Firebird.
        /// </summary>
        /// <remarks>
        /// This constant defines the upper limit for Unicode character storage in Firebird database fields
        /// of type CHAR and VARCHAR. It is used to ensure that the size of these fields does not exceed
        /// the database's supported maximum for Unicode data.
        /// </remarks>
        private const int FirebirdMaxUnicodeCharSize = 4000;
        // http://www.firebirdsql.org/en/firebird-technical-specifications/
        /// <summary>
        /// Represents the maximum size, in bytes, for a Firebird text field.
        /// </summary>
        /// <remarks>
        /// This constant defines the largest possible size for a Firebird text field, 
        /// constrained by the maximum value of a 32-bit integer. It is used to approximate
        /// the theoretical 32GB limit for text fields in Firebird.
        /// </remarks>
        private const int FirebirdMaxTextSize = int.MaxValue;  // as close as Int32 can come to 32GB

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebirdTypeMap"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets up the type mappings between .NET <see cref="DbType"/> values and Firebird SQL types
        /// by invoking the <see cref="SetupTypeMaps"/> method.
        /// </remarks>
        public FirebirdTypeMap()
        {
            SetupTypeMaps();
        }
        
        /// <summary>
        /// Configures the type mappings between .NET <see cref="DbType"/> values and Firebird SQL types.
        /// </summary>
        /// <remarks>
        /// This method defines the mapping rules for various .NET <see cref="DbType"/> values to their corresponding
        /// Firebird SQL types. It ensures that the Firebird database can correctly interpret and store data
        /// based on the specified .NET data types.
        /// </remarks>
        /// <example>
        /// For example, <see cref="DbType.String"/> is mapped to "VARCHAR($size)" with a maximum size defined
        /// by <c>FirebirdMaxUnicodeCharSize</c>.
        /// </example>
        protected override void SetupTypeMaps()
        {
            /*
             * Values were taken from the Interbase 6 Data Definition Guide
             * 
             * */
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size)", FirebirdMaxCharSize);
            SetTypeMap(DbType.AnsiString, "VARCHAR(255)");
            SetTypeMap(DbType.AnsiString, "VARCHAR($size)", FirebirdMaxVarcharSize);
            SetTypeMap(DbType.AnsiString, "BLOB SUB_TYPE TEXT", int.MaxValue);
            SetTypeMap(DbType.Binary, "BLOB SUB_TYPE BINARY");
            SetTypeMap(DbType.Binary, "BLOB SUB_TYPE BINARY", int.MaxValue);
            SetTypeMap(DbType.Boolean, "SMALLINT"); //no direct boolean support
            SetTypeMap(DbType.Byte, "SMALLINT");
            SetTypeMap(DbType.Currency, "DECIMAL(18, 4)");
            SetTypeMap(DbType.Date, "DATE");
            SetTypeMap(DbType.DateTime, "TIMESTAMP");
            SetTypeMap(DbType.DateTime2, "TIMESTAMP");
            SetTypeMap(DbType.Decimal, "DECIMAL(18, 4)");
            SetTypeMap(DbType.Decimal, "DECIMAL($size, $precision)", DecimalCapacity);
            SetTypeMap(DbType.Double, "DOUBLE PRECISION"); //64 bit double precision
            SetTypeMap(DbType.Guid, "CHAR(16) CHARACTER SET OCTETS"); //no guid support, "only" uuid is supported(via gen_uuid() built-in function)
            SetTypeMap(DbType.Int16, "SMALLINT");
            SetTypeMap(DbType.Int32, "INTEGER");
            SetTypeMap(DbType.Int64, "BIGINT");
            SetTypeMap(DbType.Single, "FLOAT");
            SetTypeMap(DbType.StringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.StringFixedLength, "CHAR($size)", FirebirdMaxUnicodeCharSize);
            SetTypeMap(DbType.String, "VARCHAR(255)");
            SetTypeMap(DbType.String, "VARCHAR($size)", FirebirdMaxUnicodeCharSize);
            SetTypeMap(DbType.String, "BLOB SUB_TYPE TEXT", FirebirdMaxTextSize);
            SetTypeMap(DbType.Time, "TIME");
        }
    }
}
