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
    /// Setting the default index column options
    /// </summary>
    public interface ICreateIndexColumnOptionsSyntax : IFluentSyntax
    {
        /// <summary>
        /// Mark the index column as ascending
        /// </summary>
        /// <returns>More column options</returns>
        ICreateIndexMoreColumnOptionsSyntax Ascending();

        /// <summary>
        /// Mark the index column as descending
        /// </summary>
        /// <returns>More column options</returns>
        ICreateIndexMoreColumnOptionsSyntax Descending();

        /// <summary>
        /// Mark the index column as unique
        /// </summary>
        /// <returns>More column options for the unique column</returns>
        ICreateIndexColumnUniqueOptionsSyntax Unique();
    }
}
