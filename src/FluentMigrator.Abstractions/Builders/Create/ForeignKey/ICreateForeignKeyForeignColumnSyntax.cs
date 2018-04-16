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

namespace FluentMigrator.Builders.Create.ForeignKey
{
    /// <summary>
    /// Interface to define the foreign key columns
    /// </summary>
    public interface ICreateForeignKeyForeignColumnSyntax : IFluentSyntax
    {
        /// <summary>
        /// Define the foreign key column
        /// </summary>
        /// <param name="column">The column name</param>
        /// <returns>Define the foreign keys primary table</returns>
        ICreateForeignKeyToTableSyntax ForeignColumn(string column);

        /// <summary>
        /// Define the foreign key columns
        /// </summary>
        /// <param name="columns">The column names</param>
        /// <returns>Define the foreign keys primary table</returns>
        ICreateForeignKeyToTableSyntax ForeignColumns(params string[] columns);
    }
}
