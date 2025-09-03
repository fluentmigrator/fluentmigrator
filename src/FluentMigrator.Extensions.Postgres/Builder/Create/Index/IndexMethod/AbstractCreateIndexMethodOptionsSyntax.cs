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
using FluentMigrator.Postgres;

using JetBrains.Annotations;

namespace FluentMigrator.Builder.Create.Index
{
    /// <summary>
    /// Provides an abstract base class for defining index creation options syntax.
    /// </summary>
    /// <remarks>
    /// This class serves as a foundation for implementing specific index creation options
    /// for various database systems. It encapsulates common functionality and delegates
    /// specific operations to the underlying <see cref="ICreateIndexOptionsSyntax"/> implementation.
    /// </remarks>
    public abstract class AbstractCreateIndexMethodOptionsSyntax : ICreateIndexMethodOptionsSyntax
    {
        /// <summary>
        /// Gets the underlying <see cref="ICreateIndexOptionsSyntax"/> implementation used to define index creation options.
        /// </summary>
        /// <remarks>
        /// This property provides access to the encapsulated <see cref="ICreateIndexOptionsSyntax"/> instance, 
        /// which is responsible for implementing the specific index creation options.
        /// </remarks>
        protected ICreateIndexOptionsSyntax CreateIndexOptionsSyntax { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractCreateIndexMethodOptionsSyntax"/> class.
        /// </summary>
        /// <param name="createIndexOptionsSyntax">
        /// The <see cref="ICreateIndexOptionsSyntax"/> instance used to configure index options.
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

        /// <inheritdoc />
        public ICreateIndexMethodOptionsSyntax Fillfactor(int fillfactor)
        {
            var additionalFeatures = CreateIndexOptionsSyntax as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(PostgresExtensions.IndexFillFactor, fillfactor);
            return this;
        }
    }
}
