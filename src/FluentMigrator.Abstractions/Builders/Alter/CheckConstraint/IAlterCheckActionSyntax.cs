#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

using FluentMigrator.Builders.Create.CheckConstraint;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Alter.CheckConstraint
{
    /// <summary>
    /// Selects the alteration to make to an existing check constraint.
    /// </summary>
    public interface IAlterCheckActionSyntax : IFluentSyntax
    {
        /// <summary>
        /// Replaces the existing check constraint with one that uses the given condition.
        /// </summary>
        /// <param name="sqlCondition">The raw SQL inserted into the CHECK clause.</param>
        /// <returns>The options for the replacement check constraint.</returns>
        ICreateCheckOptionsSyntax Condition(string sqlCondition);

        /// <summary>
        /// Enables enforcement and validates existing rows by default.
        /// </summary>
        /// <returns>The validation options.</returns>
        IAlterCheckEnableOptionsSyntax Enabled();

        /// <summary>
        /// Disables enforcement of the existing constraint.
        /// </summary>
        void Disabled();
    }
}
