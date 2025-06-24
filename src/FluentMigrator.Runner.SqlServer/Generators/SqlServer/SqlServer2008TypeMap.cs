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
    public class SqlServer2008TypeMap : SqlServer2005TypeMap
    {
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
