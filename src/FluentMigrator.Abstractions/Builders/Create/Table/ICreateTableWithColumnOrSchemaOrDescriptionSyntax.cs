#region License
//
// Copyright (c) 2007-2014, Fluent Migrator Project
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

namespace FluentMigrator.Builders.Create.Table
{
    /// <summary>
    /// Define table schema, a new column or set the tables description
    /// </summary>
    public interface ICreateTableWithColumnOrSchemaOrDescriptionSyntax : ICreateTableWithColumnOrSchemaSyntax
    {
        /// <summary>
        /// Set the tables description
        /// </summary>
        /// <param name="description">The description</param>
        /// <returns>Define the table schema or a new column</returns>
        ICreateTableWithColumnOrSchemaSyntax WithDescription(string description);

        /// <summary>
        /// Specifies the command should only be applied if the table does not already exist.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Databases with <c>CREATE TABLE IF NOT EXISTS</c> get that statement. SQL Server, Oracle and Firebird get a
        /// <c>CREATE TABLE</c> guarded by a check of the database's catalog, which also works in a connectionless script.
        /// </para>
        /// <para>
        /// The table's descriptions, and the indexes and foreign keys declared on its columns (<c>Indexed()</c>,
        /// <c>Unique()</c>, <c>ForeignKey(...)</c>, <c>ReferencedBy(...)</c>), are created only along with the table, so an
        /// existing table is left as it is. The exceptions follow.
        /// </para>
        /// <para>
        /// On SQLite the indexes are <c>CREATE INDEX IF NOT EXISTS</c>, so an index the existing table lacks is created.
        /// SQLite can't add a foreign key after the table, so a <c>ReferencedBy(...)</c> key there, even to the same
        /// table, fails in STRICT mode.
        /// </para>
        /// <para>
        /// Some indexes and foreign keys are separate statements after the table, which run whether or not the table was
        /// created: they fail when the existing table already has them, and are created when it doesn't. That is all of
        /// them on Redshift, Snowflake, SAP HANA and Db2; on MySQL, an index with options and a <c>ReferencedBy(...)</c>
        /// foreign key from another table; and on PostgreSQL, an index built with <c>CONCURRENTLY</c>.
        /// </para>
        /// <para>
        /// On Redshift and SAP HANA, the descriptions are <c>COMMENT ON</c> statements after the table, which also set
        /// the existing table's descriptions.
        /// </para>
        /// </remarks>
        /// <returns>Define the table schema, description or a new column</returns>
        ICreateTableWithColumnOrSchemaOrDescriptionSyntax IfNotExists();
    }
}
