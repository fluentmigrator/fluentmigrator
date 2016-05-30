#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2010, Nathan Brown
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
    public class SqlServerCeTypeMap : TypeMapBase
    {
        public const int UnicodeStringCapacity = 4000;
        public const int ImageCapacity = 1073741823;
        public const int BinaryCapacity = 8000;
        public const int DecimalCapacity = 38;

        protected override void SetupTypeMaps()
        {
            SetTypeMap(DbType.AnsiStringFixedLength, "NCHAR(255)");  // No CHAR support
            SetTypeMap(DbType.AnsiStringFixedLength, "NCHAR($size)", UnicodeStringCapacity);  // No CHAR support
            SetTypeMap(DbType.AnsiString, "NVARCHAR(255)");  // No VARCHAR support
            SetTypeMap(DbType.AnsiString, "NVARCHAR($size)", UnicodeStringCapacity);  // No VARCHAR support
            SetTypeMap(DbType.AnsiString, "NTEXT", int.MaxValue);  // No TEXT support
            SetTypeMap(DbType.Binary, "VARBINARY(8000)");
            SetTypeMap(DbType.Binary, "VARBINARY($size)", BinaryCapacity);
            SetTypeMap(DbType.Binary, "IMAGE", ImageCapacity);
            SetTypeMap(DbType.Boolean, "BIT");
            SetTypeMap(DbType.Byte, "TINYINT");
            SetTypeMap(DbType.Currency, "MONEY");
            SetTypeMap(DbType.Date, "DATETIME");
            SetTypeMap(DbType.DateTime, "DATETIME");
            SetTypeMap(DbType.Decimal, "NUMERIC(19,5)");
            SetTypeMap(DbType.Decimal, "NUMERIC($size,$precision)", DecimalCapacity);
            SetTypeMap(DbType.Double, "FLOAT");
            SetTypeMap(DbType.Guid, "UNIQUEIDENTIFIER");
            SetTypeMap(DbType.Int16, "SMALLINT");
            SetTypeMap(DbType.Int32, "INT");
            SetTypeMap(DbType.Int64, "BIGINT");
            SetTypeMap(DbType.Single, "REAL");
            SetTypeMap(DbType.StringFixedLength, "NCHAR(255)");
            SetTypeMap(DbType.StringFixedLength, "NCHAR($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.String, "NVARCHAR(255)");
            SetTypeMap(DbType.String, "NVARCHAR($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.String, "NTEXT", int.MaxValue);
            SetTypeMap(DbType.Time, "DATETIME");
            SetTypeMap(DbType.Xml, "NTEXT");  // No XML support
            SetTypeMap(DbType.Xml, "NTEXT", int.MaxValue);  // No XML support
        }
    }
}