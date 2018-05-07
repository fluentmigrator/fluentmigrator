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
    /// Specify the options or table of the index to delete
    /// </summary>
    public interface IDeleteIndexForTableSyntax : IFluentSyntax
    {
        /// <summary>
        /// Specify the table of the index to delete
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <returns>The next step</returns>
        IDeleteIndexOnColumnOrInSchemaSyntax OnTable(string tableName);

        /// <summary>
        /// Specify the options of the index to delete
        /// </summary>
        /// <returns>The next step</returns>
        IDeleteIndexOptionsSyntax WithOptions();
    }
}
