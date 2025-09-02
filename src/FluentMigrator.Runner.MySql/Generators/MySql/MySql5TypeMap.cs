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

namespace FluentMigrator.Runner.Generators.MySql
{
    /// <summary>
    /// Represents the type mapping for MySQL 5 database.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="FluentMigrator.Runner.Generators.MySql.MySql4TypeMap"/> to provide
    /// additional or updated type mappings specific to MySQL 5. It is used to map .NET data types to
    /// their corresponding MySQL 5 database types.
    /// </remarks>
    public class MySql5TypeMap : MySql4TypeMap
    {
        /// <summary>
        /// Represents the maximum capacity for the DECIMAL data type in MySQL 5.
        /// </summary>
        /// <remarks>
        /// This constant overrides the <see cref="FluentMigrator.Runner.Generators.MySql.MySql4TypeMap.DecimalCapacity"/> 
        /// to define the updated maximum capacity for the DECIMAL type specific to MySQL 5.
        /// </remarks>
        public new const int DecimalCapacity = 65;

        /// <inheritdoc />
        protected override void SetupMySqlTypeMaps()
        {
            base.SetupMySqlTypeMaps();

            SetTypeMap(DbType.Decimal, "DECIMAL($size,$precision)", DecimalCapacity);

            SetTypeMap(DbType.StringFixedLength, "NCHAR(255)");
            SetTypeMap(DbType.StringFixedLength, "NCHAR($size)", StringCapacity);
            SetTypeMap(DbType.StringFixedLength, "TEXT CHARACTER SET utf8", TextCapacity);
            SetTypeMap(DbType.StringFixedLength, "MEDIUMTEXT CHARACTER SET utf8", MediumTextCapacity);
            SetTypeMap(DbType.StringFixedLength, "LONGTEXT CHARACTER SET utf8", LongTextCapacity);
            SetTypeMap(DbType.String, "NVARCHAR(255)");
            SetTypeMap(DbType.String, "NVARCHAR($size)", VarcharCapacity);
            SetTypeMap(DbType.String, "TEXT CHARACTER SET utf8", TextCapacity);
            SetTypeMap(DbType.String, "MEDIUMTEXT CHARACTER SET utf8", MediumTextCapacity);
            SetTypeMap(DbType.String, "LONGTEXT CHARACTER SET utf8", LongTextCapacity);
        }

    }
}
