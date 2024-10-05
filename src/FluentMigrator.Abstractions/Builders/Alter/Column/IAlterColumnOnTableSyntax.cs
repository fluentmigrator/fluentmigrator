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

namespace FluentMigrator.Builders.Alter.Column
{
    /// <summary>
    /// Interface to specify the table for the column modification
    /// </summary>
    public interface IAlterColumnOnTableSyntax : IFluentSyntax
    {
        /// <summary>
        /// Specify the table for the column modification
        /// </summary>
        /// <param name="name">The table name</param>
        /// <returns>Interface for the column modification or the schema specification</returns>
        IAlterColumnAsTypeOrInSchemaSyntax OnTable(string name);
    }
}
