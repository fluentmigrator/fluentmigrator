#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Data;

using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.MySql
{
    /// <summary>
    /// Represents the type mapping for MySQL 4 database.
    /// </summary>
    /// <remarks>
    /// This class provides a set of predefined type mappings for MySQL 4, including constants for
    /// various data type capacities such as string, text, and decimal types. It extends the 
    /// <see cref="FluentMigrator.Runner.Generators.Base.TypeMapBase"/> class and implements the 
    /// <see cref="FluentMigrator.Runner.Generators.MySql.IMySqlTypeMap"/> interface.
    /// </remarks>
    public class MySql4TypeMap : TypeMapBase, IMySqlTypeMap
    {
        public const int AnsiTinyStringCapacity = 127;
        public const int StringCapacity = 255;
        public const int VarcharCapacity = 8192;
        public const int TextCapacity = 65535;
        public const int MediumTextCapacity = 16777215;
        public const int LongTextCapacity = int.MaxValue;
        public const int DecimalCapacity = 254;

        public MySql4TypeMap()
        {
            SetupTypeMaps();
        }

        protected virtual void SetupMySqlTypeMaps()
        {
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size)", StringCapacity);
            SetTypeMap(DbType.AnsiStringFixedLength, "TEXT", TextCapacity);
            SetTypeMap(DbType.AnsiStringFixedLength, "MEDIUMTEXT", MediumTextCapacity);
            SetTypeMap(DbType.AnsiStringFixedLength, "LONGTEXT", LongTextCapacity);
            SetTypeMap(DbType.AnsiString, "VARCHAR(255)");
            SetTypeMap(DbType.AnsiString, "VARCHAR($size)", VarcharCapacity);
            SetTypeMap(DbType.AnsiString, "TEXT", TextCapacity);
            SetTypeMap(DbType.AnsiString, "MEDIUMTEXT", MediumTextCapacity);
            SetTypeMap(DbType.AnsiString, "LONGTEXT", LongTextCapacity);
            SetTypeMap(DbType.Binary, "LONGBLOB");
            SetTypeMap(DbType.Binary, "LONGBLOB", int.MaxValue);
            SetTypeMap(DbType.Binary, "TINYBLOB", AnsiTinyStringCapacity);
            SetTypeMap(DbType.Binary, "BLOB", TextCapacity);
            SetTypeMap(DbType.Binary, "MEDIUMBLOB", MediumTextCapacity);
            SetTypeMap(DbType.Boolean, "TINYINT(1)");
            SetTypeMap(DbType.Byte, "TINYINT UNSIGNED");
            SetTypeMap(DbType.Currency, "DECIMAL(19,4)");
            SetTypeMap(DbType.Date, "DATE");
            SetTypeMap(DbType.DateTime, "DATETIME");
            SetTypeMap(DbType.DateTime2, "DATETIME");
            SetTypeMap(DbType.Decimal, "DECIMAL(19,5)");
            SetTypeMap(DbType.Decimal, "DECIMAL($size,$precision)", DecimalCapacity);
            SetTypeMap(DbType.Double, "DOUBLE");
            SetTypeMap(DbType.Guid, "CHAR(36)");
            SetTypeMap(DbType.Int16, "SMALLINT");
            SetTypeMap(DbType.Int32, "INTEGER");
            SetTypeMap(DbType.Int64, "BIGINT");
            SetTypeMap(DbType.Single, "FLOAT");
            SetTypeMap(DbType.StringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.StringFixedLength, "CHAR($size)", StringCapacity);
            SetTypeMap(DbType.StringFixedLength, "TEXT", TextCapacity);
            SetTypeMap(DbType.StringFixedLength, "MEDIUMTEXT", MediumTextCapacity);
            SetTypeMap(DbType.StringFixedLength, "LONGTEXT", LongTextCapacity);
            SetTypeMap(DbType.String, "VARCHAR(255)");
            SetTypeMap(DbType.String, "VARCHAR($size)", VarcharCapacity);
            SetTypeMap(DbType.String, "TEXT", TextCapacity);
            SetTypeMap(DbType.String, "MEDIUMTEXT", MediumTextCapacity);
            SetTypeMap(DbType.String, "LONGTEXT", LongTextCapacity);
            SetTypeMap(DbType.Time, "DATETIME");
        }
        
        protected sealed override void SetupTypeMaps()
        {
            SetupMySqlTypeMaps();
        }
    }
}
