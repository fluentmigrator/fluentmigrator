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

using System;

using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;

using JetBrains.Annotations;

namespace FluentMigrator.Builder.Create.Index
{
    /// <inheritdoc />
    public abstract class AbstractCreateIndexMethodOptionsSyntax : ICreateIndexMethodOptionsSyntax
    {
        /// <summary>
        /// Used to accumulate configuration state on create index options.
        /// </summary>
        protected ICreateIndexOptionsSyntax CreateIndexOptionsSyntax { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractCreateIndexMethodOptionsSyntax"/> class.
        /// </summary>
        /// <param name="createIndexOptionsSyntax">
        /// The syntax object used to define index options.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="createIndexOptionsSyntax"/> is <c>null</c>.
        /// </exception>
        protected AbstractCreateIndexMethodOptionsSyntax([NotNull] ICreateIndexOptionsSyntax createIndexOptionsSyntax)
        {
            CreateIndexOptionsSyntax = createIndexOptionsSyntax ?? throw new ArgumentNullException(nameof(createIndexOptionsSyntax));
        }

        /// <inheritdoc />
        public ICreateIndexOnColumnSyntax Unique()
        {
            return CreateIndexOptionsSyntax.Unique();
        }

        /// <inheritdoc />
        public ICreateIndexOnColumnSyntax NonClustered()
        {
            return CreateIndexOptionsSyntax.NonClustered();
        }

        /// <inheritdoc />
        public ICreateIndexOnColumnSyntax Clustered()
        {
            return CreateIndexOptionsSyntax.Clustered();
        }
    }
}
