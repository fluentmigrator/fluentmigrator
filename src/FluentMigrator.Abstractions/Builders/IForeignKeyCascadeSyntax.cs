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
using System.Data;

namespace FluentMigrator.Builders
{
    /// <summary>
    /// Base interface for specifying the foreign key cascading
    /// </summary>
    /// <typeparam name="TNext">The interface for the next step after specifying the cascade rules for both DELETE and UPDATE</typeparam>
    /// <typeparam name="TNextFk">The interface for the next step after specifying the cascade rule for either DELETE or UPDATE</typeparam>
    public interface IForeignKeyCascadeSyntax<TNext,TNextFk> : IFluentSyntax
        where TNext : IFluentSyntax
        where TNextFk : IFluentSyntax
    {
        /// <summary>
        /// Specify the behavior for DELETEs
        /// </summary>
        /// <param name="rule">The rule to apply for DELETEs</param>
        /// <returns>The next step</returns>
        TNextFk OnDelete(Rule rule);

        /// <summary>
        /// Specify the behavior for UPDATEs
        /// </summary>
        /// <param name="rule">The rule to apply for UPDATEs</param>
        /// <returns>The next step</returns>
        TNextFk OnUpdate(Rule rule);

        /// <summary>
        /// Specify the behavior for UPDATEs and DELETEs
        /// </summary>
        /// <param name="rule">The rule to apply for UPDATEs and DELETEs</param>
        /// <returns>The next step</returns>
        TNext OnDeleteOrUpdate(Rule rule);
    }
}
