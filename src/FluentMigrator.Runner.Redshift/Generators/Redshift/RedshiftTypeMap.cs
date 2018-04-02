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

namespace FluentMigrator.Runner.Generators.Redshift
{
    internal class RedshiftTypeMap : TypeMapBase
    {
        private const int DecimalCapacity = 1000;
        private const int RedshiftMaxVarcharSize = 10485760;

        protected override void SetupTypeMaps()
        {
            SetTypeMap(DbType.AnsiStringFixedLength, "char(255)");
            SetTypeMap(DbType.AnsiStringFixedLength, "char($size)", int.MaxValue);
            SetTypeMap(DbType.AnsiString, "text");
            SetTypeMap(DbType.AnsiString, "varchar($size)", RedshiftMaxVarcharSize);
            SetTypeMap(DbType.AnsiString, "text", int.MaxValue);
            SetTypeMap(DbType.Boolean, "boolean");
            SetTypeMap(DbType.Byte, "smallint"); //no built-in support for single byte unsigned integers
            SetTypeMap(DbType.DateTime, "timestamp");
            SetTypeMap(DbType.DateTime2, "timestamp"); // timestamp columns in Redshift can support a larger date range.  Source: http://www.postgresql.org/docs/9.1/static/datatype-datetime.html
            SetTypeMap(DbType.DateTimeOffset, "timestamptz");
            SetTypeMap(DbType.Decimal, "decimal(18,0)");
            SetTypeMap(DbType.Decimal, "decimal($size,$precision)", DecimalCapacity);
            SetTypeMap(DbType.Double, "float8");
            SetTypeMap(DbType.Int16, "smallint");
            SetTypeMap(DbType.Int32, "integer");
            SetTypeMap(DbType.Int64, "bigint");
            SetTypeMap(DbType.Single, "float4");
            SetTypeMap(DbType.StringFixedLength, "char(255)");
            SetTypeMap(DbType.StringFixedLength, "char($size)", int.MaxValue);
            SetTypeMap(DbType.String, "text");
            SetTypeMap(DbType.String, "varchar($size)", RedshiftMaxVarcharSize);
            SetTypeMap(DbType.String, "text", int.MaxValue);
        }
    }
}
