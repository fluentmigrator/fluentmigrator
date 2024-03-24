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
    /// Index options
    /// </summary>
    public interface ICreateIndexOptionsSyntax : IFluentSyntax
    {
        /// <summary>
        /// Defines the index as unique
        /// </summary>
        /// <returns>Defines the column for the index</returns>
        ICreateIndexOnColumnSyntax Unique();

        /// <summary>
        /// Defines the index as non-clustered
        /// </summary>
        /// <returns>Defines the column for the index</returns>
        ICreateIndexOnColumnSyntax NonClustered();

        /// <summary>
        /// Defines the index as clustered
        /// </summary>
        /// <returns>Defines the column for the index</returns>
        ICreateIndexOnColumnSyntax Clustered();
    }
}
