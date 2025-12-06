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

namespace FluentMigrator.Runner.Generators.Jet
{
    /// <summary>
    /// Provides type mapping between <see cref="DbType"/> and Microsoft Jet SQL types.
    /// </summary>
    public sealed class JetTypeMap : TypeMapBase, IJetTypeMap
    {
        /// <summary>Default capacity for ANSI string columns.</summary>
        public const int AnsiStringCapacity = 255;
        /// <summary>Default capacity for ANSI text columns.</summary>
        public const int AnsiTextCapacity = 1073741823;
        /// <summary>Default capacity for Unicode string columns.</summary>
        public const int UnicodeStringCapacity = 255;
        /// <summary>Default capacity for Unicode text columns.</summary>
        public const int UnicodeTextCapacity = 1073741823;
        /// <summary>Default capacity for IMAGE columns.</summary>
        public const int ImageCapacity = 2147483647;
        /// <summary>Default capacity for DECIMAL columns.</summary>
        public const int DecimalCapacity = 19;

        /// <inheritdoc />
        public JetTypeMap()
        {
            SetupTypeMaps();
        }
        
        /// <inheritdoc />
        protected override void SetupTypeMaps()
        {
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size)", AnsiStringCapacity);
            SetTypeMap(DbType.AnsiString, "VARCHAR(255)");
            SetTypeMap(DbType.AnsiString, "VARCHAR($size)", AnsiStringCapacity);
            SetTypeMap(DbType.AnsiString, "TEXT", AnsiTextCapacity);
            SetTypeMap(DbType.Binary, "VARBINARY(8000)");
            SetTypeMap(DbType.Binary, "VARBINARY($size)", ImageCapacity);
            SetTypeMap(DbType.Binary, "IMAGE", ImageCapacity);
            SetTypeMap(DbType.Boolean, "BIT");
            SetTypeMap(DbType.Byte, "TINYINT");
            SetTypeMap(DbType.Currency, "MONEY");
            SetTypeMap(DbType.Date, "DATETIME");
            SetTypeMap(DbType.DateTime, "DATETIME");
            SetTypeMap(DbType.DateTime2, "DATETIME");
            SetTypeMap(DbType.Decimal, "DECIMAL(19,5)");
            SetTypeMap(DbType.Decimal, "DECIMAL($size,$precision)", DecimalCapacity);
            SetTypeMap(DbType.Double, "FLOAT");
            SetTypeMap(DbType.Guid, "UNIQUEIDENTIFIER");
            SetTypeMap(DbType.Int16, "SMALLINT");
            SetTypeMap(DbType.Int32, "INTEGER");
            // Jet does not have a 64-bit integer type, thus mapping it to a decimal
            SetTypeMap(DbType.Int64, "DECIMAL(20,0)");
            SetTypeMap(DbType.Single, "REAL");
            SetTypeMap(DbType.StringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.StringFixedLength, "CHAR($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.String, "VARCHAR(255)");
            SetTypeMap(DbType.String, "VARCHAR($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.String, "TEXT", UnicodeTextCapacity);
            SetTypeMap(DbType.Time, "DATETIME");
        }
    }
}
