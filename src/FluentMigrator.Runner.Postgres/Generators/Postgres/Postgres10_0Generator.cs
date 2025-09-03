#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System.Collections.Generic;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Postgres;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.Postgres
{
    /// <summary>
    /// The PostgreSQL 10.0 SQL generator for FluentMigrator.
    /// </summary>
    public class Postgres10_0Generator : PostgresGenerator
    {
        /// <inheritdoc />
        public Postgres10_0Generator([NotNull] PostgresQuoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        /// <inheritdoc />
        public Postgres10_0Generator([NotNull] PostgresQuoter quoter, [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new Postgres10_0Column(quoter, new Postgres92.Postgres92TypeMap()), quoter, generatorOptions)
        {
        }

        /// <inheritdoc />
        protected Postgres10_0Generator([NotNull] PostgresQuoter quoter, [NotNull] IOptions<GeneratorOptions> generatorOptions, [NotNull] IPostgresTypeMap typeMap)
            : base(new Postgres10_0Column(quoter, typeMap), quoter, generatorOptions)
        {
        }

        /// <inheritdoc />
        protected Postgres10_0Generator(
            [NotNull] IColumn column,
            [NotNull] PostgresQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, generatorOptions)
        {
        }

        /// <inheritdoc />
        protected override HashSet<string> GetAllowIndexStorageParameters()
        {
            var allow = base.GetAllowIndexStorageParameters();

            allow.Add(PostgresExtensions.IndexBuffering);
            allow.Add(PostgresExtensions.IndexGinPendingListLimit);
            allow.Add(PostgresExtensions.IndexPagesPerRange);
            allow.Add(PostgresExtensions.IndexAutosummarize);

            return allow;
        }

        /// <inheritdoc />
        protected override string GetOverridingIdentityValuesString(InsertDataExpression expression)
        {
            if (!expression.AdditionalFeatures.ContainsKey(PostgresExtensions.OverridingIdentityValues))
            {
                return string.Empty;
            }

            var overridingIdentityValues =
                expression.GetAdditionalFeature<PostgresOverridingIdentityValuesType>(
                    PostgresExtensions.OverridingIdentityValues);

            return
                $" OVERRIDING {(overridingIdentityValues == PostgresOverridingIdentityValuesType.User ? "USER" : "SYSTEM")} VALUE";
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.PostgreSQL10_0;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases =>
            [GeneratorIdConstants.PostgreSQL10_0, GeneratorIdConstants.PostgreSQL];
    }
}
