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
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Postgres;

using JetBrains.Annotations;

namespace FluentMigrator.Builder.Create.Index
{
    /// <summary>
    /// Provides syntax for configuring GIN (Generalized Inverted Index) index options in PostgreSQL.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="AbstractCreateIndexMethodOptionsSyntax"/> to provide additional
    /// configuration options specific to GIN indexes, such as fill factor, fast update, and pending list limit.
    /// </remarks>
    public class CreateGinIndexOptionsSyntax : AbstractCreateIndexMethodOptionsSyntax, ICreateGinIndexOptionsSyntax
    {
        /// <inheritdoc />
        public CreateGinIndexOptionsSyntax([NotNull] ICreateIndexOptionsSyntax createIndexOptionsSyntax)
            : base(createIndexOptionsSyntax)
        {
        }

        /// <inheritdoc />
        public new ICreateGinIndexOptionsSyntax Fillfactor(int fillfactor)
        {
            base.Fillfactor(fillfactor);
            return this;
        }

        /// <inheritdoc />
        public ICreateGinIndexOptionsSyntax FastUpdate()
        {
            return FastUpdate(true);
        }

        /// <inheritdoc />
        public ICreateGinIndexOptionsSyntax DisableFastUpdate()
        {
            return FastUpdate(false);
        }

        /// <inheritdoc />
        public ICreateGinIndexOptionsSyntax FastUpdate(bool fastUpdate)
        {
            var additionalFeatures = CreateIndexOptionsSyntax as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(PostgresExtensions.IndexFastUpdate, fastUpdate);
            return this;
        }

        /// <inheritdoc />
        public ICreateGinIndexOptionsSyntax PendingListLimit(long limit)
        {
            var additionalFeatures = CreateIndexOptionsSyntax as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(PostgresExtensions.IndexGinPendingListLimit, limit);
            return this;
        }
    }
}
