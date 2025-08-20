#region License
//
// Copyright (c) 2010, Nathan Brown
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

using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Generation;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.SqlServer;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    public class SqlServer2008Generator : SqlServer2005Generator
    {
        private static readonly HashSet<string> _supportedAdditionalFeatures = new HashSet<string>
        {
            SqlServerExtensions.IndexColumnNullsDistinct,
        };

        public SqlServer2008Generator()
            : this(new SqlServer2008Quoter())
        {
        }

        public SqlServer2008Generator(
            [NotNull] SqlServer2008Quoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public SqlServer2008Generator(
            [NotNull] SqlServer2008Quoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : this(
                new SqlServer2008Column(new SqlServer2008TypeMap(), quoter),
                quoter,
                new SqlServer2005DescriptionGenerator(),
                generatorOptions)
        {
        }

        protected SqlServer2008Generator(
            [NotNull] IColumn column,
            [NotNull] IQuoter quoter,
            [NotNull] IDescriptionGenerator descriptionGenerator,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, descriptionGenerator, generatorOptions)
        {
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.SqlServer2008;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases =>
            [GeneratorIdConstants.SqlServer2008, GeneratorIdConstants.SqlServer];

        public override bool IsAdditionalFeatureSupported(string feature)
        {
            return _supportedAdditionalFeatures.Contains(feature)
             || base.IsAdditionalFeatureSupported(feature);
        }

        /// <inheritdoc />
        public override string GetFilterString(CreateIndexExpression createIndexExpression)
        {
            var baseFilter = base.GetFilterString(createIndexExpression);
            var nullsDistinct = GetWithNullsDistinctString(createIndexExpression.Index);

            if (string.IsNullOrEmpty(baseFilter) && string.IsNullOrEmpty(nullsDistinct))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(nullsDistinct))
            {
                return baseFilter;
            }

            baseFilter = string.IsNullOrEmpty(baseFilter) ?
                $" WHERE {nullsDistinct}" :
                $" AND  {nullsDistinct}";

            return baseFilter;
        }

        protected string GetWithNullsDistinctString(IndexDefinition index)
        {
            bool? GetNullsDistinct(IndexColumnDefinition column)
            {
                return column.GetAdditionalFeature(SqlServerExtensions.IndexColumnNullsDistinct, (bool?)null);
            }

            var indexNullsDistinct = index.GetAdditionalFeature(SqlServerExtensions.IndexColumnNullsDistinct, (bool?)null);

            var nullDistinctColumns = index.Columns.Where(c => indexNullsDistinct != null || GetNullsDistinct(c) != null).ToList();
            if (nullDistinctColumns.Count != 0 && !index.IsUnique)
            {
                // Should never occur
                CompatibilityMode.HandleCompatibility("With nulls distinct can only be used for unique indexes");
                return string.Empty;
            }

            // The "Nulls (not) distinct" value of the column
            // takes higher precedence than the value of the index
            // itself.
            var conditions = nullDistinctColumns
                .Where(x => (GetNullsDistinct(x) ?? indexNullsDistinct ?? true) == false)
                .Select(c => $"{Quoter.QuoteColumnName(c.Name)} IS NOT NULL");

            var condition = string.Join(" AND ", conditions);
            if (condition.Length == 0)
                return string.Empty;

            return condition;
        }

        /// <inheritdoc />
        public override string GetWithOptions(ISupportAdditionalFeatures expression)
        {
            var items = new List<string>();
            var options = base.GetWithOptions(expression);

            if (!string.IsNullOrEmpty(options))
            {
                items.Add(options);
            }

            var dataCompressionType = expression.GetAdditionalFeature(SqlServerExtensions.DataCompression, (DataCompressionType)null);
            if (dataCompressionType != null)
            {
                items.Add($"DATA_COMPRESSION = {dataCompressionType}");
            }

            return string.Join(", ", items);
        }
    }
}
