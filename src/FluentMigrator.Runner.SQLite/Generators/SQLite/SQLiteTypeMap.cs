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

namespace FluentMigrator.Runner.Generators.SQLite
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Represents a type mapping implementation for SQLite, providing mappings between .NET <see cref="DbType"/> values
    /// and their corresponding SQLite data types.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="TypeMapBase"/> and implements <see cref="ISQLiteTypeMap"/> to define SQLite-specific
    /// type mappings. It supports both strict and non-strict table modes, which influence the mapping of certain data types.
    /// </remarks>
    public sealed class SQLiteTypeMap : TypeMapBase, ISQLiteTypeMap
    {
        /// <summary>
        /// Gets a value indicating whether strict table mode is enabled for SQLite type mappings.
        /// </summary>
        /// <value>
        /// <c>true</c> if strict table mode is enabled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When strict table mode is enabled, certain data types are mapped differently to enforce stricter type constraints.
        /// For example, numeric and date/time types are mapped to <c>TEXT</c> instead of their default SQLite types.
        /// </remarks>
        public bool UseStrictTables { get; }

        /// <summary>
        /// Specifies the default maximum capacity for ANSI string data types in SQLite.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum length, in characters, for ANSI strings that can be stored in SQLite.
        /// It is used as a default size for mapping <see cref="DbType.AnsiString"/> to SQLite data types.
        /// </remarks>
        public const int AnsiStringCapacity = 8000;
        /// <summary>
        /// Specifies the maximum capacity for ANSI text data types in SQLite.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum length, in characters, for ANSI text that can be stored in SQLite.
        /// It is used as a default size for mapping large ANSI text data, such as <see cref="DbType.AnsiString"/> 
        /// with unspecified or very large sizes, to SQLite data types.
        /// </remarks>
        public const int AnsiTextCapacity = 2147483647;
        /// <summary>
        /// Specifies the default maximum capacity for Unicode string data types in SQLite.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum length, in characters, for Unicode strings that can be stored in SQLite.
        /// It is used as a default size for mapping <see cref="DbType.String"/> to SQLite data types.
        /// </remarks>
        public const int UnicodeStringCapacity = 4000;
        /// <summary>
        /// Specifies the default maximum capacity for Unicode text data types in SQLite.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum length, in characters, for Unicode text that can be stored in SQLite.
        /// It is used as a default size for mapping <see cref="DbType.String"/> to SQLite data types.
        /// </remarks>
        public const int UnicodeTextCapacity = 1073741823;
        /// <summary>
        /// Represents the maximum capacity, in bytes, for storing image data in SQLite.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum size for binary data that can be stored in a column
        /// intended for image data in SQLite. It is typically used in conjunction with the 
        /// <see cref="DbType.Binary"/> type mapping.
        /// </remarks>
        public const int ImageCapacity = 2147483647;
        /// <summary>
        /// Represents the default precision for decimal values in SQLite type mappings.
        /// </summary>
        /// <remarks>
        /// This constant is used to define the maximum precision for decimal data types when mapping
        /// .NET <see cref="System.Data.DbType.Decimal"/> to SQLite.
        /// </remarks>
        public const int DecimalCapacity = 19;
        /// <summary>
        /// Represents the maximum capacity for XML data types in SQLite.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum size, in bytes, that can be allocated for XML data storage
        /// when using SQLite. It is primarily used to ensure consistent handling of XML data types
        /// across migrations.
        /// </remarks>
        public const int XmlCapacity = 1073741823;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteTypeMap"/> class.
        /// </summary>
        /// <param name="useStrictTables">
        /// A value indicating whether to use strict tables in SQLite.
        /// When set to <c>true</c>, strict table definitions are enabled; otherwise, they are disabled.
        /// </param>
        public SQLiteTypeMap(bool useStrictTables = false)
        {
            UseStrictTables = useStrictTables;
            SetupTypeMaps();
        }

        /// <inheritdoc />
        protected override void SetupTypeMaps()
        {
            SetTypeMap(DbType.Binary, "BLOB");
            SetTypeMap(DbType.Byte, "INTEGER");
            SetTypeMap(DbType.Int16, "INTEGER");
            SetTypeMap(DbType.Int32, "INTEGER");
            SetTypeMap(DbType.Int64, "INTEGER");
            SetTypeMap(DbType.SByte, "INTEGER");
            SetTypeMap(DbType.UInt16, "INTEGER");
            SetTypeMap(DbType.UInt32, "INTEGER");
            SetTypeMap(DbType.UInt64, "INTEGER");
            if (!UseStrictTables)
            {
                SetTypeMap(DbType.Currency, "NUMERIC");
                SetTypeMap(DbType.Decimal, "NUMERIC");
                SetTypeMap(DbType.Double, "NUMERIC");
                SetTypeMap(DbType.Single, "NUMERIC");
                SetTypeMap(DbType.VarNumeric, "NUMERIC");
                SetTypeMap(DbType.Date, "DATETIME");
                SetTypeMap(DbType.DateTime, "DATETIME");
                SetTypeMap(DbType.DateTime2, "DATETIME");
                SetTypeMap(DbType.Time, "DATETIME");
                SetTypeMap(DbType.Guid, "UNIQUEIDENTIFIER");

            }
            else
            {
                SetTypeMap(DbType.Currency, "TEXT");
                SetTypeMap(DbType.Decimal, "TEXT");
                SetTypeMap(DbType.Double, "REAL");
                SetTypeMap(DbType.Single, "REAL");
                SetTypeMap(DbType.VarNumeric, "TEXT");
                SetTypeMap(DbType.Date, "TEXT");
                SetTypeMap(DbType.DateTime, "TEXT");
                SetTypeMap(DbType.DateTime2, "TEXT");
                SetTypeMap(DbType.Time, "TEXT");
                SetTypeMap(DbType.Guid, "TEXT");
            }
            
            SetTypeMap(DbType.AnsiString, "TEXT");
            SetTypeMap(DbType.String, "TEXT");
            SetTypeMap(DbType.AnsiStringFixedLength, "TEXT");
            SetTypeMap(DbType.StringFixedLength, "TEXT");
            SetTypeMap(DbType.Boolean, "INTEGER");
        }

        /// <inheritdoc />
        public override string GetTypeMap(DbType type, int? size, int? precision)
        {
            return base.GetTypeMap(type, size: null, precision: null);
        }
    }
}
