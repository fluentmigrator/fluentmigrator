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

namespace FluentMigrator.Builders.Create.ForeignKey
{
    /// <summary>
    /// Define the primary table columns for a foreign key
    /// </summary>
    public interface ICreateForeignKeyPrimaryColumnSyntax : IFluentSyntax
    {
        /// <summary>
        /// Define the primary table column for a foreign key
        /// </summary>
        /// <param name="column">The column name</param>
        /// <returns>Define the cascade rules</returns>
        ICreateForeignKeyCascadeSyntax PrimaryColumn(string column);

        /// <summary>
        /// Define the primary table columns for a foreign key
        /// </summary>
        /// <param name="columns">The column names</param>
        /// <returns>Define the cascade rules</returns>
        ICreateForeignKeyCascadeSyntax PrimaryColumns(params string[] columns);
    }
}
