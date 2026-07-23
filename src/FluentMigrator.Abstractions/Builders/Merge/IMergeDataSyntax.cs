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

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Merge
{
    /// <summary>
    /// Specify the data to merge
    /// </summary>
    public interface IMergeDataSyntax : IFluentSyntax
    {
        /// <summary>
        /// Specify the data to merge
        /// </summary>
        /// <param name="recordAsAnonymousType">An anonymous object that is used to merge data</param>
        /// <remarks>
        /// The properties are the column names and their values are the row values.
        /// </remarks>
        /// <returns>The next step</returns>
        IMergeRowSyntax Row(object recordAsAnonymousType);

        /// <summary>
        /// Specify the data to merge
        /// </summary>
        /// <param name="record">The dictionary containing column name/value combinations</param>
        /// <returns>The next step</returns>
        IMergeRowSyntax Row(IDictionary<string, object> record);
    }

    /// <summary>
    /// Specify additional row data or match columns for merge
    /// </summary>
    public interface IMergeRowSyntax : IFluentSyntax
    {
        /// <summary>
        /// Specify additional data to merge
        /// </summary>
        /// <param name="recordAsAnonymousType">An anonymous object that is used to merge data</param>
        /// <returns>The next step</returns>
        IMergeRowSyntax Row(object recordAsAnonymousType);

        /// <summary>
        /// Specify additional data to merge
        /// </summary>
        /// <param name="record">The dictionary containing column name/value combinations</param>
        /// <returns>The next step</returns>
        IMergeRowSyntax Row(IDictionary<string, object> record);

        /// <summary>
        /// Specify the columns to match for determining if a row should be updated or inserted
        /// </summary>
        /// <param name="selector">A function that selects the match columns from the row data</param>
        /// <returns>The next step</returns>
        void Match<T>(Func<T, object> selector);

        /// <summary>
        /// Specify the columns to match for determining if a row should be updated or inserted
        /// </summary>
        /// <param name="columnNames">The column names to use for matching</param>
        void Match(params string[] columnNames);
    }
}