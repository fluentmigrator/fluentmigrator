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
    public sealed class SQLiteTypeMap : TypeMapBase, ISQLiteTypeMap
    {
        public bool UseStrictTables { get; }

        public const int AnsiStringCapacity = 8000;
        public const int AnsiTextCapacity = 2147483647;
        public const int UnicodeStringCapacity = 4000;
        public const int UnicodeTextCapacity = 1073741823;
        public const int ImageCapacity = 2147483647;
        public const int DecimalCapacity = 19;
        public const int XmlCapacity = 1073741823;

        public SQLiteTypeMap(bool useStrictTables = false)
        {
            UseStrictTables = useStrictTables;
            SetupTypeMaps();
        }

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

        public override string GetTypeMap(DbType type, int? size, int? precision)
        {
            return base.GetTypeMap(type, size: null, precision: null);
        }
    }
}
