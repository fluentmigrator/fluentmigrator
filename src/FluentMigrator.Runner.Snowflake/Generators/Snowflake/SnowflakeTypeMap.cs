#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

namespace FluentMigrator.Runner.Generators.Snowflake
{
    /// <summary>
    /// Represents a type map specific to the Snowflake database.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="FluentMigrator.Runner.Generators.Base.TypeMapBase"/> and implements
    /// <see cref="FluentMigrator.Runner.Generators.Snowflake.ISnowflakeTypeMap"/> to provide mappings
    /// between <see cref="System.Data.DbType"/> and Snowflake-specific SQL types.
    /// </remarks>
    public class SnowflakeTypeMap : TypeMapBase, ISnowflakeTypeMap
    {
        /// <summary>
        /// Represents the maximum capacity for Unicode string types in Snowflake.
        /// </summary>
        /// <remarks>
        /// This field defines the maximum size, in characters, for Unicode string types
        /// such as <c>VARCHAR</c> when mapped to Snowflake-specific SQL types.
        /// </remarks>
        public const int UnicodeStringCapacity = 134217728;
        /// <summary>
        /// Represents the maximum precision for decimal types in Snowflake.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum number of digits (precision) that can be used
        /// for decimal types when mapped to Snowflake-specific SQL types.
        /// </remarks>
        private const int DecimalCapacity = 38;
        /// <summary>
        /// Represents the maximum capacity, in bytes, for binary data types in Snowflake.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum size for binary data types, such as <c>BINARY</c>,
        /// when mapped to Snowflake-specific SQL types.
        /// </remarks>
        private const int BinaryCapacity = 8388608;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Generators.Snowflake.SnowflakeTypeMap"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets up the type mappings between <see cref="System.Data.DbType"/> and Snowflake-specific SQL types
        /// by invoking the <see cref="SetupTypeMaps"/> method.
        /// </remarks>
        public SnowflakeTypeMap()
        {
            SetupTypeMaps();
        }

        /// <inheritdoc />
        protected sealed override void SetupTypeMaps()
        {
            SetTypeMap(DbType.AnsiStringFixedLength, "VARCHAR");
            SetTypeMap(DbType.AnsiStringFixedLength, "VARCHAR($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.AnsiString, "VARCHAR");
            SetTypeMap(DbType.AnsiString, "VARCHAR($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.Binary, "BINARY");
            SetTypeMap(DbType.Binary, "BINARY($size)", BinaryCapacity);
            SetTypeMap(DbType.Boolean, "BOOLEAN");
            SetTypeMap(DbType.Byte, "NUMBER");
            SetTypeMap(DbType.Date, "DATE");
            SetTypeMap(DbType.DateTime, "TIMESTAMP_NTZ");
            SetTypeMap(DbType.DateTime2, "TIMESTAMP_NTZ");
            SetTypeMap(DbType.DateTimeOffset, "TIMESTAMP_TZ");
            SetTypeMap(DbType.Decimal, "NUMBER");
            SetTypeMap(DbType.Decimal, "NUMBER($size,$precision)", DecimalCapacity);
            SetTypeMap(DbType.Double, "FLOAT");
            SetTypeMap(DbType.Int16, "NUMBER");
            SetTypeMap(DbType.Int32, "NUMBER");
            SetTypeMap(DbType.Int64, "NUMBER");
            SetTypeMap(DbType.Single, "FLOAT");
            SetTypeMap(DbType.StringFixedLength, "VARCHAR");
            SetTypeMap(DbType.StringFixedLength, "VARCHAR($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.String, "VARCHAR");
            SetTypeMap(DbType.String, "VARCHAR($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.Time, "TIME");
            SetTypeMap(DbType.Object, "OBJECT");
        }
    }
}
