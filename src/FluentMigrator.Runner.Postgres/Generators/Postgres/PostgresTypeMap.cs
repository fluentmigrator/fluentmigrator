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

namespace FluentMigrator.Runner.Generators.Postgres
{
    internal class PostgresTypeMap : TypeMapBase, IPostgresTypeMap
    {
        private const int DecimalCapacity = 1000;
        private const int PostgresMaxVarcharSize = 10485760;

        public PostgresTypeMap()
        {
            SetupTypeMaps();
        }

        protected virtual void SetupPostgresTypeMaps()
        {
            SetTypeMap(DbType.AnsiStringFixedLength, "char(255)");
            SetTypeMap(DbType.AnsiStringFixedLength, "char($size)", int.MaxValue);
            SetTypeMap(DbType.AnsiString, "text");
            SetTypeMap(DbType.AnsiString, "varchar($size)", PostgresMaxVarcharSize);
            SetTypeMap(DbType.AnsiString, "text", int.MaxValue);
            SetTypeMap(DbType.Binary, "bytea");
            SetTypeMap(DbType.Binary, "bytea", int.MaxValue);
            SetTypeMap(DbType.Boolean, "boolean");
            SetTypeMap(DbType.Byte, "smallint"); //no built-in support for single byte unsigned integers
            SetTypeMap(DbType.Currency, "money");
            SetTypeMap(DbType.Date, "date");
            SetTypeMap(DbType.DateTime, "timestamp");
            SetTypeMap(DbType.DateTime2, "timestamp"); // timestamp columns in postgres can support a larger date range.  Source: http://www.postgresql.org/docs/9.1/static/datatype-datetime.html
            SetTypeMap(DbType.DateTimeOffset, "timestamptz");
            SetTypeMap(DbType.Decimal, "decimal(19,5)");
            SetTypeMap(DbType.Decimal, "decimal($size,$precision)", DecimalCapacity);
            SetTypeMap(DbType.Double, "float8");
            SetTypeMap(DbType.Guid, "uuid");
            SetTypeMap(DbType.Int16, "smallint");
            SetTypeMap(DbType.Int32, "integer");
            SetTypeMap(DbType.Int64, "bigint");
            SetTypeMap(DbType.Single, "float4");
            SetTypeMap(DbType.StringFixedLength, "char(255)");
            SetTypeMap(DbType.StringFixedLength, "char($size)", int.MaxValue);
            SetTypeMap(DbType.String, "text");
            SetTypeMap(DbType.String, "varchar($size)", PostgresMaxVarcharSize);
            SetTypeMap(DbType.String, "text", int.MaxValue);
            SetTypeMap(DbType.Time, "time");
            SetTypeMap(DbType.Xml, "xml");
        }


        protected sealed override void SetupTypeMaps()
        {
            SetupPostgresTypeMaps();
        }
    }
}
