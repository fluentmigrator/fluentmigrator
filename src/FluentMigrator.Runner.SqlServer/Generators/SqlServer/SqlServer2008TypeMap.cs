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

namespace FluentMigrator.Runner.Generators.SqlServer
{
    /// <summary>
    /// Represents the SQL Server 2008-specific type map used for database migrations.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="SqlServer2005TypeMap"/> to include support for 
    /// SQL Server 2008-specific data types, such as <c>DATETIME2</c>, <c>DATETIMEOFFSET</c>, 
    /// <c>DATE</c>, and <c>TIME</c>. It provides mappings between .NET <see cref="System.Data.DbType"/> 
    /// values and their corresponding SQL Server 2008 data types.
    /// </remarks>
    public class SqlServer2008TypeMap : SqlServer2005TypeMap
    {
        /// <summary>
        /// Configures the SQL Server 2008-specific type mappings.
        /// </summary>
        /// <remarks>
        /// This method extends the base type mappings defined in <see cref="SqlServer2005TypeMap"/> 
        /// by adding support for SQL Server 2008-specific types, such as <c>DATETIME2</c>, 
        /// <c>DATETIMEOFFSET</c>, <c>DATE</c>, and <c>TIME</c>.
        /// </remarks>
        protected override void SetupSqlServerTypeMaps()
        {
            base.SetupSqlServerTypeMaps();

            SetTypeMap(DbType.DateTime2, "DATETIME2");
            SetTypeMap(DbType.DateTimeOffset, "DATETIMEOFFSET");
            SetTypeMap(DbType.DateTimeOffset, "DATETIMEOFFSET($size)", maxSize: 7);
            SetTypeMap(DbType.Date, "DATE");
            SetTypeMap(DbType.Time, "TIME");
        }
    }
}
