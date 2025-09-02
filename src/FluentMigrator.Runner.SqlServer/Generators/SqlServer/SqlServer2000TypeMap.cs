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

namespace FluentMigrator.Runner.Generators.SqlServer
{
    /// <summary>
    /// Represents the type mapping for SQL Server 2000, providing mappings between .NET data types and SQL Server 2000 data types.
    /// </summary>
    /// <remarks>
    /// This class defines the specific type mappings for SQL Server 2000, including capacities for various data types such as 
    /// ANSI strings, Unicode strings, images, and decimals. It extends <see cref="FluentMigrator.Runner.Generators.Base.TypeMapBase"/> 
    /// and implements <see cref="FluentMigrator.Runner.Generators.SqlServer.ISqlServerTypeMap"/>.
    /// </remarks>
    public class SqlServer2000TypeMap : TypeMapBase, ISqlServerTypeMap
    {
        /// <summary>
        /// Represents the maximum capacity, in characters, for ANSI string types in SQL Server 2000.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum allowable size for ANSI strings, such as those mapped to 
        /// <c>VARCHAR</c> or <c>CHAR</c> types, when using SQL Server 2000.
        /// </remarks>
        public const int AnsiStringCapacity = 8000;
        /// <summary>
        /// Represents the maximum capacity, in characters, for ANSI text data types in SQL Server 2000.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum size for ANSI text data types, such as <c>TEXT</c> or <c>VARCHAR(MAX)</c>, 
        /// supported by SQL Server 2000. It is used to configure type mappings for ANSI text fields.
        /// </remarks>
        public const int AnsiTextCapacity = 2147483647;
        /// <summary>
        /// Represents the maximum capacity, in characters, for Unicode string data types in SQL Server 2000.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum number of characters that can be stored in a Unicode string 
        /// column (e.g., <c>NVARCHAR</c> or <c>NCHAR</c>) for SQL Server 2000. It is used in type mappings 
        /// to ensure compatibility with the database's limitations.
        /// </remarks>
        public const int UnicodeStringCapacity = 4000;
        /// <summary>
        /// Represents the maximum capacity, in characters, for Unicode text data in SQL Server 2000.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum size for Unicode text data stored in a SQL Server 2000 database.
        /// It is used to map .NET data types to SQL Server 2000 data types when working with Unicode text.
        /// </remarks>
        public const int UnicodeTextCapacity = 1073741823;
        /// <summary>
        /// Represents the maximum storage capacity, in bytes, for the SQL Server 2000 "IMAGE" data type.
        /// </summary>
        /// <remarks>
        /// The "IMAGE" data type is used to store binary large objects (BLOBs) such as images, documents, or other binary data.
        /// This constant defines the maximum size supported by SQL Server 2000 for this type, which is 2,147,483,647 bytes.
        /// </remarks>
        public const int ImageCapacity = 2147483647;
        /// <summary>
        /// Represents the maximum precision for the <c>DECIMAL</c> data type in SQL Server 2000.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum number of digits that a <c>DECIMAL</c> type can store,
        /// including both the integral and fractional parts.
        /// </remarks>
        /// <summary>
        /// Represents the maximum capacity for the <c>DECIMAL</c> data type in SQL Server 2000.
        /// </summary>
        /// <remarks>
        /// This constant specifies the maximum precision of a <c>DECIMAL</c> type, which is the total number of digits
        /// that can be stored, including both the integral and fractional parts.
        /// </remarks>
        public const int DecimalCapacity = 38;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Generators.SqlServer.SqlServer2000TypeMap"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets up the type mappings specific to SQL Server 2000 by invoking the <see cref="SetupTypeMaps"/> method.
        /// </remarks>
        public SqlServer2000TypeMap()
        {
            SetupTypeMaps();
        }

        /// <summary>
        /// Configures the SQL Server 2000-specific type mappings for database types.
        /// </summary>
        /// <remarks>
        /// This method defines the mappings between <see cref="DbType"/> values and their corresponding
        /// SQL Server 2000 data type representations. It includes default mappings as well as mappings
        /// with specific size constraints.
        /// </remarks>
        /// <seealso cref="TypeMapBase"/>
        /// <seealso cref="ISqlServerTypeMap"/>
        protected virtual void SetupSqlServerTypeMaps()
        {
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size)", AnsiStringCapacity);
            SetTypeMap(DbType.AnsiString, "VARCHAR(255)");
            SetTypeMap(DbType.AnsiString, "VARCHAR($size)", AnsiStringCapacity);
            SetTypeMap(DbType.AnsiString, "TEXT", AnsiTextCapacity);
            SetTypeMap(DbType.Binary, "VARBINARY(8000)");
            SetTypeMap(DbType.Binary, "VARBINARY($size)", AnsiStringCapacity);
            SetTypeMap(DbType.Binary, "IMAGE", ImageCapacity);
            SetTypeMap(DbType.Boolean, "BIT");
            SetTypeMap(DbType.Byte, "TINYINT");
            SetTypeMap(DbType.Currency, "MONEY");
            SetTypeMap(DbType.Date, "DATETIME");
            SetTypeMap(DbType.DateTime, "DATETIME");
            SetTypeMap(DbType.DateTime2, "DATETIME");
            SetTypeMap(DbType.Decimal, "DECIMAL(19,5)");
            SetTypeMap(DbType.Decimal, "DECIMAL($size,$precision)", DecimalCapacity);
            SetTypeMap(DbType.Double, "DOUBLE PRECISION");
            SetTypeMap(DbType.Guid, "UNIQUEIDENTIFIER");
            SetTypeMap(DbType.Int16, "SMALLINT");
            SetTypeMap(DbType.Int32, "INT");
            SetTypeMap(DbType.Int64, "BIGINT");
            SetTypeMap(DbType.Single, "REAL");
            SetTypeMap(DbType.StringFixedLength, "NCHAR(255)");
            SetTypeMap(DbType.StringFixedLength, "NCHAR($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.String, "NVARCHAR(255)");
            SetTypeMap(DbType.String, "NVARCHAR($size)", UnicodeStringCapacity);
            // Officially this is 1073741823 but we will allow the int.MaxValue Convention
            SetTypeMap(DbType.String, "NTEXT", int.MaxValue);
            SetTypeMap(DbType.Time, "DATETIME");
        }

        /// <inheritdoc />
        protected sealed override void SetupTypeMaps()
        {
            SetupSqlServerTypeMaps();
        }
    }
}
