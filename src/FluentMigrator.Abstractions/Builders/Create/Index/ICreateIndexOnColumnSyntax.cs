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

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.Index
{
    /// <summary>
    /// Definition of index columns or options
    /// </summary>
    public interface ICreateIndexOnColumnSyntax : IFluentSyntax
    {
        /// <summary>
        /// Defines the index column
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns>Defines the index column options</returns>
        ICreateIndexColumnOptionsSyntax OnColumn(string columnName);

        /// <summary>
        /// Set the index options
        /// </summary>
        /// <returns>Defines the index options</returns>
        ICreateIndexOptionsSyntax WithOptions();
    }
}
