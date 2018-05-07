#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete.Index
{
    /// <summary>
    /// Specify the column or columns of the index to dlete
    /// </summary>
    public interface IDeleteIndexOnColumnSyntax : IFluentSyntax
    {
        /// <summary>
        /// Specify the column of the index to delete
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The next step</returns>
        IDeleteIndexOptionsSyntax OnColumn(string columnName);

        /// <summary>
        /// Specify the columns of the index to delete
        /// </summary>
        /// <param name="columnNames">The column names</param>
        /// <returns>The next step</returns>
        IDeleteIndexOptionsSyntax OnColumns(params string[] columnNames);

        /// <summary>
        /// Specify the options of the index to delete
        /// </summary>
        /// <returns>The next step</returns>
        IDeleteIndexOptionsSyntax WithOptions();
    }
}
