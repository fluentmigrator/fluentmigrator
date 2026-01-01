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
    /// Represents the definition of a PostgreSQL security label.
    /// </summary>
    public class PostgresSecurityLabelDefinition
    {
        /// <summary>
        /// Gets or sets the type of object to apply the security label to.
        /// </summary>
        public PostgresSecurityLabelObjectType ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the object. Optional for some object types.
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the name of the object (table, view, schema, or role name).
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// Gets or sets the column name. Only used when <see cref="ObjectType"/> is <see cref="PostgresSecurityLabelObjectType.Column"/>.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the security label provider name. Optional.
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Gets or sets the security label value.
        /// </summary>
        public string Label { get; set; }
    }
}
