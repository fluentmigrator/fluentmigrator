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

namespace FluentMigrator.Runner.Generators.DB2
{
    internal class Db2TypeMap : TypeMapBase, IDb2TypeMap
    {
        public Db2TypeMap()
        {
            SetupTypeMaps();
        }

        /// <summary>
        /// The setup type maps.
        /// </summary>
        protected sealed override void SetupTypeMaps()
        {
            SetTypeMap(DbType.AnsiStringFixedLength, "CHARACTER(255)");
            SetTypeMap(DbType.AnsiStringFixedLength, "CHARACTER($size)", 255);
            SetTypeMap(DbType.AnsiString, "VARCHAR(255)");
            SetTypeMap(DbType.AnsiString, "VARCHAR($size)", 32704);
            SetTypeMap(DbType.AnsiString, "CLOB(1048576)");
            SetTypeMap(DbType.AnsiString, "CLOB($size)", int.MaxValue);
            SetTypeMap(DbType.Binary, "BINARY(255)");
            SetTypeMap(DbType.Binary, "BINARY($size)", 255);
            SetTypeMap(DbType.Binary, "VARBINARY(8000)");
            SetTypeMap(DbType.Binary, "VARBINARY($size)", 32704);
            SetTypeMap(DbType.Binary, "BLOB(1048576)");
            SetTypeMap(DbType.Binary, "BLOB($size)", 2147483647);
            SetTypeMap(DbType.Boolean, "CHAR(1)");
            SetTypeMap(DbType.Byte, "SMALLINT");
            SetTypeMap(DbType.Time, "TIME");
            SetTypeMap(DbType.Date, "DATE");
            SetTypeMap(DbType.DateTime, "TIMESTAMP");
            SetTypeMap(DbType.DateTime2, "TIMESTAMP");
            SetTypeMap(DbType.Decimal, "NUMERIC(19,5)");
            SetTypeMap(DbType.Decimal, "NUMERIC($size,$precision)", 31);
            SetTypeMap(DbType.Decimal, "DECIMAL(19,5)");
            SetTypeMap(DbType.Decimal, "DECIMAL($size,$precision)", 31);
            SetTypeMap(DbType.Double, "DOUBLE");
            SetTypeMap(DbType.Int16, "SMALLINT");
            SetTypeMap(DbType.Int32, "INT");
            SetTypeMap(DbType.Int32, "INTEGER");
            SetTypeMap(DbType.Int64, "BIGINT");
            SetTypeMap(DbType.Single, "REAL");
            SetTypeMap(DbType.Single, "DECFLOAT", 34);
            SetTypeMap(DbType.StringFixedLength, "GRAPHIC(128)");
            SetTypeMap(DbType.StringFixedLength, "GRAPHIC($size)", 128);
            SetTypeMap(DbType.String, "VARGRAPHIC(8000)");
            SetTypeMap(DbType.String, "VARGRAPHIC($size)", 16352);
            SetTypeMap(DbType.String, "DBCLOB(1048576)");
            SetTypeMap(DbType.String, "DBCLOB($size)", 1073741824);
            SetTypeMap(DbType.Xml, "XML");
        }
    }
}
