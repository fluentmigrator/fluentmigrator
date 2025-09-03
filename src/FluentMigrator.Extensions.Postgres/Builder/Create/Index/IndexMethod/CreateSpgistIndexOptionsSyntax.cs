#region License
// Copyright (c) 2021, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using FluentMigrator.Builders.Create.Index;

using JetBrains.Annotations;

namespace FluentMigrator.Builder.Create.Index
{
    /// <summary>
    /// Provides SP-GiST (Space-Partitioned Generalized Search Tree) index options
    /// for use in PostgreSQL migrations.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="AbstractCreateIndexMethodOptionsSyntax"/> and implements
    /// <see cref="ICreateSpgistIndexOptionsSyntax"/> to allow fluent configuration of SP-GiST index options,
    /// such as setting the fill factor.
    /// </remarks>
    public class CreateSpgistIndexOptionsSyntax : AbstractCreateIndexMethodOptionsSyntax, ICreateSpgistIndexOptionsSyntax
    {
        /// <inheritdoc />
        public CreateSpgistIndexOptionsSyntax([NotNull] ICreateIndexOptionsSyntax createIndexOptionsSyntax)
            : base(createIndexOptionsSyntax)
        {
        }

        /// <inheritdoc />
        public new ICreateSpgistIndexOptionsSyntax Fillfactor(int fillfactor)
        {
            base.Fillfactor(fillfactor);
            return this;
        }
    }
}
