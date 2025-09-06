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

using System.Collections.Generic;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders
{
    /// <summary>
    /// Specify column options
    /// </summary>
    /// <typeparam name="TNext">The interface of the next step</typeparam>
    /// <typeparam name="TNextFk">The interface of the next step after a foreign key definition</typeparam>
    public interface IColumnOptionSyntax<TNext,TNextFk> : IFluentSyntax
        where TNext : IFluentSyntax
        where TNextFk : IFluentSyntax
    {
        /// <summary>
        /// Sets the default function for the column
        /// </summary>
        /// <param name="method">The function providing the default value</param>
        /// <returns>The next step</returns>
        TNext WithDefault(SystemMethods method);

        /// <summary>
        /// Sets the default value for the column
        /// </summary>
        /// <param name="value">The default value</param>
        /// <returns>The next step</returns>
        TNext WithDefaultValue(object value);

        /// <summary>
        /// Sets the columns description
        /// </summary>
        /// <param name="description">The description</param>
        /// <returns>The next step</returns>
        TNext WithColumnDescription(string description);

        /// <summary>
        /// Sets any additional column descriptions using a pair of name and description content
        /// </summary>
        /// <param name="descriptionName">The descriptionName</param>
        /// <param name="description">The description</param>
        /// <returns>The next step</returns>
        TNext WithColumnAdditionalDescription(string descriptionName, string description);

        /// <summary>
        /// Sets any additional column descriptions using a Dictionary containing multiple pairs of name and description content
        /// </summary>
        /// <param name="columnDescriptions">The columnDescriptions list</param>
        /// <returns>The next step</returns>
        TNext WithColumnAdditionalDescriptions(Dictionary<string,string> columnDescriptions);

        /// <summary>
        /// Sets the columns identity configuration
        /// </summary>
        /// <returns>The next step</returns>
        TNext Identity();

        /// <summary>
        /// Create an index for this column
        /// </summary>
        /// <returns>The next step</returns>
        TNext Indexed();

        /// <summary>
        /// Create an index for this column
        /// </summary>
        /// <param name="indexName">The index name</param>
        /// <returns>The next step</returns>
        TNext Indexed(string indexName);

        /// <summary>
        /// Define the column as primary key
        /// </summary>
        /// <returns>The next step</returns>
        TNext PrimaryKey();

        /// <summary>
        /// Define the column as primary key
        /// </summary>
        /// <param name="primaryKeyName">The primary key constraint name</param>
        /// <returns>The next step</returns>
        TNext PrimaryKey(string primaryKeyName);

        /// <summary>
        /// Specify the column as nullable
        /// </summary>
        /// <returns>The next step</returns>
        TNext Nullable();

        /// <summary>
        /// Specify the column as not-nullable
        /// </summary>
        /// <returns>The next step</returns>
        TNext NotNullable();

        /// <summary>
        /// Defines the column as a computed column
        /// </summary>
        /// <param name="expression">The expression to calculate</param>
        /// <param name="stored">Whether the computed column is virtual or stored</param>
        /// <returns>The next step</returns>
        TNext Computed(string expression, bool stored = false);

        /// <summary>
        /// Specify a unique index for the column
        /// </summary>
        /// <returns>The next step</returns>
        TNext Unique();

        /// <summary>
        /// Specify a unique index for the column
        /// </summary>
        /// <param name="indexName">The index name</param>
        /// <returns>The next step</returns>
        TNext Unique(string indexName);

        /// <summary>
        /// Specifies a foreign key
        /// </summary>
        /// <param name="primaryTableName">The primary table name</param>
        /// <param name="primaryColumnName">The primary tables column name</param>
        /// <returns>The next step</returns>
        TNextFk ForeignKey(string primaryTableName, string primaryColumnName);

        /// <summary>
        /// Specifies a foreign key
        /// </summary>
        /// <param name="foreignKeyName">The foreign key name</param>
        /// <param name="primaryTableName">The primary table name</param>
        /// <param name="primaryColumnName">The primary tables column name</param>
        /// <returns>The next step</returns>
        TNextFk ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName);

        /// <summary>
        /// Specifies a foreign key
        /// </summary>
        /// <param name="foreignKeyName">The foreign key name</param>
        /// <param name="primaryTableSchema">The primary tables schema name</param>
        /// <param name="primaryTableName">The primary table name</param>
        /// <param name="primaryColumnName">The primary tables column name</param>
        /// <returns>The next step</returns>
        TNextFk ForeignKey(string foreignKeyName, string primaryTableSchema, string primaryTableName, string primaryColumnName);

        /// <summary>
        /// Specifies a foreign key
        /// </summary>
        /// <returns>The next step</returns>
        TNextFk ForeignKey();

        /// <summary>
        /// Specifies a multi-column foreign key
        /// </summary>
        /// <param name="foreignColumns">The foreign key column names</param>
        /// <param name="primaryTableName">The primary table name</param>
        /// <param name="primaryColumns">The primary table column names</param>
        /// <returns>The next step</returns>
        TNextFk ForeignKey(string[] foreignColumns, string primaryTableName, string[] primaryColumns);

        /// <summary>
        /// Specifies a multi-column foreign key
        /// </summary>
        /// <param name="foreignKeyName">The foreign key name</param>
        /// <param name="foreignColumns">The foreign key column names</param>
        /// <param name="primaryTableName">The primary table name</param>
        /// <param name="primaryColumns">The primary table column names</param>
        /// <returns>The next step</returns>
        TNextFk ForeignKey(string foreignKeyName, string[] foreignColumns, string primaryTableName, string[] primaryColumns);

        /// <summary>
        /// Specifies a multi-column foreign key
        /// </summary>
        /// <param name="foreignKeyName">The foreign key name</param>
        /// <param name="foreignColumns">The foreign key column names</param>
        /// <param name="primaryTableSchema">The primary table schema name</param>
        /// <param name="primaryTableName">The primary table name</param>
        /// <param name="primaryColumns">The primary table column names</param>
        /// <returns>The next step</returns>
        TNextFk ForeignKey(string foreignKeyName, string[] foreignColumns, string primaryTableSchema, string primaryTableName, string[] primaryColumns);

        /// <summary>
        /// Specify a foreign key pointing to the current column
        /// </summary>
        /// <param name="foreignTableName">The foreign key table</param>
        /// <param name="foreignColumnName">The foreign key column</param>
        /// <returns>The next step</returns>
        TNextFk ReferencedBy(string foreignTableName, string foreignColumnName);

        /// <summary>
        /// Specify a foreign key pointing to the current column
        /// </summary>
        /// <param name="foreignKeyName">The foreign key name</param>
        /// <param name="foreignTableName">The foreign key table</param>
        /// <param name="foreignColumnName">The foreign key column</param>
        /// <returns>The next step</returns>
        TNextFk ReferencedBy(string foreignKeyName, string foreignTableName, string foreignColumnName);

        /// <summary>
        /// Specify a foreign key pointing to the current column
        /// </summary>
        /// <param name="foreignKeyName">The foreign key name</param>
        /// <param name="foreignTableSchema">The foreign key table schema</param>
        /// <param name="foreignTableName">The foreign key table</param>
        /// <param name="foreignColumnName">The foreign key column</param>
        /// <returns>The next step</returns>
        TNextFk ReferencedBy(string foreignKeyName, string foreignTableSchema, string foreignTableName, string foreignColumnName);
    }
}
