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

namespace FluentMigrator.Postgres
{
    /// <summary>
    /// Specifies the type of database object to apply a PostgreSQL security label to.
    /// </summary>
    public enum PostgresSecurityLabelObjectType
    {
        /// <summary>
        /// Security label for a table.
        /// </summary>
        Table,

        /// <summary>
        /// Security label for a column.
        /// </summary>
        Column,

        /// <summary>
        /// Security label for a schema.
        /// </summary>
        Schema,

        /// <summary>
        /// Security label for a role.
        /// </summary>
        Role,

        /// <summary>
        /// Security label for a view.
        /// </summary>
        View
    }
}
