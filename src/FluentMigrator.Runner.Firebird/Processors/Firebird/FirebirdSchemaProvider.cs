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

using System;
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Models;

namespace FluentMigrator.Runner.Processors.Firebird
{
    /// <summary>
    /// Provides schema-related operations for Firebird databases, such as retrieving table definitions,
    /// column definitions, indexes, and sequences. This class facilitates interaction with the database
    /// schema and supports schema validation and manipulation.
    /// </summary>
    public class FirebirdSchemaProvider
    {
        private readonly FirebirdQuoter _quoter;
        internal Dictionary<string, FirebirdTableSchema> TableSchemas = new Dictionary<string, FirebirdTableSchema>();
        /// <summary>
        /// Gets or sets the <see cref="FirebirdProcessor"/> instance used to execute database operations.
        /// </summary>
        /// <remarks>
        /// The <see cref="FirebirdProcessor"/> is responsible for interacting with the Firebird database,
        /// executing SQL commands, and performing schema-related operations.
        /// </remarks>
        public FirebirdProcessor Processor { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdSchemaProvider"/> class.
        /// </summary>
        /// <param name="processor">
        /// The <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdProcessor"/> instance used to interact with the Firebird database.
        /// </param>
        /// <param name="quoter">
        /// The <see cref="FluentMigrator.Runner.Generators.Firebird.FirebirdQuoter"/> instance used for quoting database object names.
        /// </param>
        public FirebirdSchemaProvider(FirebirdProcessor processor, FirebirdQuoter quoter)
        {
            _quoter = quoter;
            Processor = processor;
        }

        /// <summary>
        /// Retrieves the definition of a specific column within a specified table in the Firebird database.
        /// </summary>
        /// <param name="tableName">The name of the table containing the column.</param>
        /// <param name="columnName">The name of the column whose definition is to be retrieved.</param>
        /// <returns>
        /// A <see cref="FluentMigrator.Model.ColumnDefinition"/> object representing the definition of the specified column,
        /// including its name, data type, constraints, and other metadata.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the specified column is not found in the table.
        /// </exception>
        public ColumnDefinition GetColumnDefinition(string tableName, string columnName)
        {
            FirebirdTableDefinition firebirdTableDef = GetTableDefinition(tableName);
            return firebirdTableDef.Columns.First(x => x.Name == columnName);
        }

        /// <summary>
        /// Retrieves the definition of a table in the Firebird database.
        /// </summary>
        /// <param name="tableName">The name of the table whose definition is to be retrieved.</param>
        /// <returns>
        /// A <see cref="FirebirdTableDefinition"/> object containing details about the table, 
        /// including its columns, indexes, and foreign keys.
        /// </returns>
        /// <remarks>
        /// This method relies on the <see cref="GetTableSchema"/> method to fetch the schema information
        /// for the specified table and extracts its definition.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the specified table name does not exist in the database schema.
        /// </exception>
        internal FirebirdTableDefinition GetTableDefinition(string tableName)
        {
            return GetTableSchema(tableName).Definition;
        }

        /// <summary>
        /// Retrieves the schema information for a specified table in the Firebird database.
        /// </summary>
        /// <param name="tableName">The name of the table whose schema information is to be retrieved.</param>
        /// <returns>
        /// A <see cref="FirebirdTableSchema"/> object containing detailed schema information about the table, 
        /// including its columns, indexes, constraints, and triggers.
        /// </returns>
        /// <remarks>
        /// If the schema information for the specified table has already been loaded, this method retrieves it 
        /// from the internal cache. Otherwise, it loads the schema information from the database.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the specified table name does not exist in the database schema.
        /// </exception>
        internal FirebirdTableSchema GetTableSchema(string tableName)
        {
            if (TableSchemas.ContainsKey(tableName))
                return TableSchemas[tableName];
            return LoadTableSchema(tableName);
        }

        /// <summary>
        /// Loads the schema information for the specified table in the Firebird database.
        /// </summary>
        /// <param name="tableName">The name of the table whose schema is to be loaded.</param>
        /// <returns>
        /// An instance of <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdTableSchema"/> 
        /// containing the schema details of the specified table.
        /// </returns>
        /// <remarks>
        /// This method creates a new <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdTableSchema"/> 
        /// object for the specified table, adds it to the internal cache, and returns it. 
        /// It facilitates schema retrieval for tables that are not already cached.
        /// </remarks>
        internal FirebirdTableSchema LoadTableSchema(string tableName)
        {
            FirebirdTableSchema schema = new FirebirdTableSchema(tableName, Processor, _quoter);
            TableSchemas.Add(tableName, schema);
            return schema;
        }

        /// <summary>
        /// Retrieves the definition of a specific index from a table in the Firebird database.
        /// </summary>
        /// <param name="tableName">The name of the table containing the index.</param>
        /// <param name="indexName">The name of the index to retrieve.</param>
        /// <returns>
        /// An <see cref="IndexDefinition"/> object representing the index definition, 
        /// or <c>null</c> if the index does not exist in the specified table.
        /// </returns>
        /// <remarks>
        /// This method fetches the table definition using <see cref="GetTableDefinition"/> 
        /// and searches for the specified index within the table's index collection.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the specified table name does not exist in the database schema.
        /// </exception>
        public IndexDefinition GetIndex(string tableName, string indexName)
        {
            FirebirdTableDefinition firebirdTableDef = GetTableDefinition(tableName);
            if (firebirdTableDef.Indexes.Any(x => x.Name == indexName))
                return firebirdTableDef.Indexes.First(x => x.Name == indexName);
            return null;
        }

        /// <summary>
        /// Retrieves metadata for a specified sequence in the Firebird database.
        /// </summary>
        /// <param name="sequenceName">
        /// The name of the sequence whose metadata is to be retrieved.
        /// </param>
        /// <returns>
        /// A <see cref="FluentMigrator.Runner.Processors.Firebird.SequenceInfo"/> object containing metadata about the specified sequence,
        /// including its name and current value.
        /// </returns>
        /// <remarks>
        /// This method uses the <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdProcessor"/> to query the database
        /// and the <see cref="FluentMigrator.Runner.Generators.Firebird.FirebirdQuoter"/> to properly quote the sequence name.
        /// </remarks>
        public SequenceInfo GetSequence(string sequenceName)
        {
            return SequenceInfo.Read(Processor, sequenceName, _quoter);
        }
    }
}
