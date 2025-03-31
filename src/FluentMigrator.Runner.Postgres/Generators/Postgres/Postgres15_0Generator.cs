using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Postgres;

using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.Postgres
{
    public class Postgres15_0Generator : Postgres11_0Generator
    {
        public Postgres15_0Generator([NotNull] PostgresQuoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public Postgres15_0Generator([NotNull] PostgresQuoter quoter, [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new Postgres10_0Column(quoter, new Postgres92.Postgres92TypeMap()), quoter, generatorOptions)
        {
        }

        protected Postgres15_0Generator([NotNull] PostgresQuoter quoter, [NotNull] IOptions<GeneratorOptions> generatorOptions, [NotNull] IPostgresTypeMap typeMap)
            : base(new Postgres10_0Column(quoter, typeMap), quoter, generatorOptions)
        {
        }

        protected Postgres15_0Generator(
            [NotNull] IColumn column,
            [NotNull] PostgresQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, generatorOptions)
        {
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.PostgreSQL15_0;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases =>
            [GeneratorIdConstants.PostgreSQL15_0, GeneratorIdConstants.PostgreSQL];

        protected override string GetWithNullsDistinctStringInWhere(IndexDefinition index)
        {
            return string.Empty;
        }

        protected override string GetWithNullsDistinctString(IndexDefinition index)
        {
            bool? GetNullsDistinct(IndexColumnDefinition column)
                => column.GetAdditionalFeature(PostgresExtensions.IndexColumnNullsDistinct, (bool?)null);

            var indexNullsDistinct = index.GetAdditionalFeature(PostgresExtensions.IndexColumnNullsDistinct, (bool?)null);
            var columnNullsDistinct = index.Columns.Select(c => GetNullsDistinct(c)).FirstOrDefault(nd => nd != null);
            var nullsDistinct = columnNullsDistinct ?? indexNullsDistinct;

            if (nullsDistinct != null && !index.IsUnique)
            {
                // Should never occur
                CompatibilityMode.HandleCompatibility("With nulls distinct can only be used for unique indexes");
                return string.Empty;
            }

            return nullsDistinct == false ? " NULLS NOT DISTINCT" : string.Empty;
        }
    }
}
