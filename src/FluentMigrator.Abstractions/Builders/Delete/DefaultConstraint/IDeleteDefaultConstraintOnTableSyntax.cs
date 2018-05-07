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

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete.DefaultConstraint
{
    /// <summary>
    /// Define the table of the column to delete the default constraint from
    /// </summary>
    public interface IDeleteDefaultConstraintOnTableSyntax : IFluentSyntax
    {
        /// <summary>
        /// Specify the name of the table to delete the columns default constraint from
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <returns>The next step</returns>
        IDeleteDefaultConstraintOnColumnOrInSchemaSyntax OnTable(string tableName);
    }
}
