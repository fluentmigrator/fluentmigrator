#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Generators
{
    /// <summary>
    /// The interface to be implemented for handling quotes
    /// </summary>
    [Obsolete("Warning! This type will move in a future version of FluentMigrator to FluentMigrator.Generation.IQuoter")]
    public interface IQuoter
    {
        /// <summary>
        /// Returns a quoted string that has been correctly escaped
        /// </summary>
        string Quote([CanBeNull] string name);

        /// <summary>
        /// Provides an unquoted, unescaped string
        /// </summary>
        string UnQuote(string value);

        /// <summary>
        /// Quotes a value to be embedded into an SQL script/statement
        /// </summary>
        /// <param name="value">The value to be quoted</param>
        /// <returns>The quoted value</returns>
        string QuoteValue(object value);

        /// <summary>
        /// Returns true is the value starts and ends with a close quote
        /// </summary>
        bool IsQuoted(string value);

        /// <summary>
        /// Quotes a column name
        /// </summary>
        string QuoteColumnName(string columnName);

        /// <summary>
        /// Quotes a Table name
        /// </summary>
        string QuoteTableName(string tableName, string schemaName = null);

        /// <summary>
        /// Quote an index name
        /// </summary>
        string QuoteIndexName(string indexName, string schemaName = null);

        /// <summary>
        /// Quotes a constraint name
        /// </summary>
        string QuoteConstraintName(string constraintName, string schemaName = null);

        /// <summary>
        /// Quotes a Sequence name
        /// </summary>
        string QuoteSequenceName(string sequenceName, string schemaName = null);

        /// <summary>
        /// Quotes a schema name
        /// </summary>
        /// <param name="schemaName">The schema name to quote</param>
        /// <returns>The quoted schema name</returns>
        string QuoteSchemaName(string schemaName);
    }
}
