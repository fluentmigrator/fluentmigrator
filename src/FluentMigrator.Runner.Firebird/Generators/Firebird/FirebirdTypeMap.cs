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
    public sealed class FirebirdTypeMap : TypeMapBase, IFirebirdTypeMap
    {
        private const int DecimalCapacity = 19;
        private const int FirebirdMaxVarcharSize = 32765;
        private const int FirebirdMaxCharSize = 32767;
        private const int FirebirdMaxUnicodeCharSize = 4000;
        // http://www.firebirdsql.org/en/firebird-technical-specifications/
        private const int FirebirdMaxTextSize = int.MaxValue;  // as close as Int32 can come to 32GB

        public FirebirdTypeMap()
        {
            SetupTypeMaps();
        }
        
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
