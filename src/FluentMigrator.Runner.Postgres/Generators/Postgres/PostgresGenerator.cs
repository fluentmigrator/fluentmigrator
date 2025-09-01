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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using FluentMigrator.Builder.Create.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Postgres;
using FluentMigrator.Runner.Generators.Generic;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.Postgres
{
    /// <summary>
    /// The PostgreSQL SQL generator for FluentMigrator.
    /// </summary>
    public class PostgresGenerator : GenericGenerator
    {
        private static readonly HashSet<string> _supportedAdditionalFeatures = new HashSet<string>
        {
            PostgresExtensions.IndexColumnNullsDistinct,
        };

        /// <inheritdoc />
        public PostgresGenerator(
            [NotNull] PostgresQuoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        /// <inheritdoc />
        public PostgresGenerator(
            [NotNull] PostgresQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new PostgresColumn(quoter, new PostgresTypeMap()), quoter, new PostgresDescriptionGenerator(quoter), generatorOptions)
        {
        }

        /// <inheritdoc />
        protected PostgresGenerator(
            [NotNull] PostgresQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions,
            IPostgresTypeMap typeMap)
            : base(new PostgresColumn(quoter, typeMap), quoter, new PostgresDescriptionGenerator(quoter), generatorOptions)
        {
        }

        /// <inheritdoc />
        protected PostgresGenerator(
            [NotNull] IColumn column,
            [NotNull] PostgresQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, new PostgresDescriptionGenerator(quoter), generatorOptions)
        {
        }

        /// <inheritdoc />
        public override bool IsAdditionalFeatureSupported(string feature) =>
            _supportedAdditionalFeatures.Contains(feature)
         || base.IsAdditionalFeatureSupported(feature);

        /// <inheritdoc />
        public override string AddColumn => "ALTER TABLE {0} ADD {1}";
        /// <inheritdoc />
        public override string AlterColumn => "ALTER TABLE {0} {1}";
        /// <inheritdoc />
        public override string RenameTable => "ALTER TABLE {0} RENAME TO {1}";

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.PostgreSQL;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases => [GeneratorIdConstants.PostgreSQL, GeneratorIdConstants.Postgres];

        /// <inheritdoc />
        public override string Generate(CreateSchemaExpression expression)
        {
            return FormatStatement(CreateSchema, Quoter.QuoteSchemaName(expression.SchemaName));
        }

        /// <inheritdoc />
        public override string Generate(DeleteSchemaExpression expression)
        {
            return FormatStatement(DropSchema, Quoter.QuoteSchemaName(expression.SchemaName));
        }

        /// <inheritdoc />
        public override string Generate(AlterColumnExpression expression)
        {
            var alterStatement = new StringBuilder();
            alterStatement.AppendFormat(
                AlterColumn,
                Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                ((PostgresColumn)Column).GenerateAlterClauses(expression.Column));

            AppendSqlStatementEndToken(alterStatement);

            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (!string.IsNullOrEmpty(descriptionStatement))
            {
                alterStatement.Append(descriptionStatement);
                AppendSqlStatementEndToken(alterStatement);
            }

            return alterStatement.ToString();
        }

        /// <inheritdoc />
        public override string Generate(CreateForeignKeyExpression expression)
        {
            var primaryColumns = GetColumnList(expression.ForeignKey.PrimaryColumns);
            var foreignColumns = GetColumnList(expression.ForeignKey.ForeignColumns);

            const string sql = "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6}";

            return FormatStatement(sql,
                Quoter.QuoteTableName(expression.ForeignKey.ForeignTable, expression.ForeignKey.ForeignTableSchema),
                Quoter.Quote(expression.ForeignKey.Name),
                foreignColumns,
                Quoter.QuoteTableName(expression.ForeignKey.PrimaryTable, expression.ForeignKey.PrimaryTableSchema),
                primaryColumns,
                Column.FormatCascade("DELETE", expression.ForeignKey.OnDelete),
                Column.FormatCascade("UPDATE", expression.ForeignKey.OnUpdate)
            );
        }

        /// <inheritdoc />
        public override string Generate(DeleteForeignKeyExpression expression)
        {
            return FormatStatement("ALTER TABLE {0} DROP CONSTRAINT {1}",
                Quoter.QuoteTableName(expression.ForeignKey.ForeignTable, expression.ForeignKey.ForeignTableSchema),
                Quoter.Quote(expression.ForeignKey.Name));
        }

        /// <inheritdoc />
        protected virtual string GetIncludeString(CreateIndexExpression column)
        {
            var includes = column.GetAdditionalFeature<IList<PostgresIndexIncludeDefinition>>(PostgresExtensions.IncludesList);

            if (includes == null || includes.Count == 0)
            {
                return string.Empty;
            }

            throw new NotSupportedException("The current version doesn't support include index. Please use Postgres 11.");
        }

        /// <inheritdoc />
        protected virtual Algorithm GetIndexMethod(CreateIndexExpression expression)
        {
            var algorithm = expression.GetAdditionalFeature<PostgresIndexAlgorithmDefinition>(PostgresExtensions.IndexAlgorithm);
            if (algorithm == null)
            {
                return Algorithm.BTree;
            }

            return algorithm.Algorithm;
        }

        /// <inheritdoc />
        protected virtual string GetFilter(CreateIndexExpression expression)
        {
            var filter = expression.Index.GetAdditionalFeature<string>(PostgresExtensions.IndexFilter);
            var nullsDistinctString = GetWithNullsDistinctStringInWhere(expression.Index);

            if (!string.IsNullOrWhiteSpace(filter) && !string.IsNullOrWhiteSpace(nullsDistinctString))
            {
                CompatibilityMode.HandleCompatibility("In PostgreSQL 14 or older, With nulls distinct can not be combined with WHERE");
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                return " WHERE " + filter;
            }

            return nullsDistinctString;
        }

        /// <inheritdoc />
        protected virtual string GetWithNullsDistinctStringInWhere(IndexDefinition index)
        {
            bool? GetNullsDistinct(IndexColumnDefinition column)
                => column.GetAdditionalFeature(PostgresExtensions.IndexColumnNullsDistinct, (bool?)null);

            var indexNullsDistinct = index.GetAdditionalFeature(PostgresExtensions.IndexColumnNullsDistinct, (bool?)null);

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
            return condition.Length == 0 ? string.Empty : $" WHERE {condition}";
        }

        /// <inheritdoc />
        protected virtual string GetWithNullsDistinctString(IndexDefinition index)
        {
            return string.Empty;
        }

        /// <inheritdoc />
        protected virtual string GetAsConcurrently(CreateIndexExpression expression)
        {
            var asConcurrently = expression.GetAdditionalFeature<PostgresIndexConcurrentlyDefinition>(PostgresExtensions.Concurrently);

            if (asConcurrently == null || !asConcurrently.IsConcurrently)
            {
                return string.Empty;
            }

            return " CONCURRENTLY";
        }

        /// <inheritdoc />
        protected virtual string GetAsOnly(CreateIndexExpression expression)
        {
            var asOnly = expression.GetAdditionalFeature<PostgresIndexOnlyDefinition>(PostgresExtensions.Only);

            if (asOnly == null || !asOnly.IsOnly)
            {
                return string.Empty;
            }

            throw new NotSupportedException("The current version doesn't support ONLY. Please use Postgres 11 or higher.");
        }

        /// <inheritdoc />
        protected virtual string GetNullsSort(IndexColumnDefinition column)
        {
            var sort = column.GetAdditionalFeature<PostgresIndexNullsSort>(PostgresExtensions.NullsSort);
            if (sort == null)
            {
                return string.Empty;
            }

            if (sort.Sort == NullSort.First)
            {
                return " NULLS FIRST";
            }

            return " NULLS LAST";
        }

        /// <inheritdoc />
        protected virtual string GetTablespace(CreateIndexExpression expression)
        {
            var tablespace = expression.Index.GetAdditionalFeature<string>(PostgresExtensions.IndexTablespace);
            if (!string.IsNullOrWhiteSpace(tablespace))
            {
                return " TABLESPACE " + tablespace;
            }

            return string.Empty;
        }

        /// <inheritdoc />
        protected virtual string GetWithIndexStorageParameters(CreateIndexExpression expression)
        {
            var allow = GetAllowIndexStorageParameters();
            var parameters = new List<string>();

            var fillFactor = GetIndexStorageParameters<int?>(PostgresExtensions.IndexFillFactor, "FillFactor");
            if (fillFactor.HasValue)
            {
                parameters.Add($"FILLFACTOR = {fillFactor}");
            }

            var fastUpdate = GetIndexStorageParameters<bool?>( PostgresExtensions.IndexFastUpdate, "FastUpdate");
            if (fastUpdate.HasValue)
            {
                parameters.Add($"FASTUPDATE = {ToOnOff(fastUpdate.Value)}");
            }

            // Postgres 10 or Higher
            var buffering = GetIndexStorageParameters<GistBuffering?>(PostgresExtensions.IndexBuffering, "Buffering");
            if (buffering.HasValue)
            {
                parameters.Add($"BUFFERING = {buffering.Value.ToString().ToUpper()}");
            }

            var pendingList = GetIndexStorageParameters<long?>(PostgresExtensions.IndexGinPendingListLimit, "GinPendingListLimit");
            if (pendingList.HasValue)
            {
                parameters.Add($"GIN_PENDING_LIST_LIMIT = {pendingList}");
            }

            var perRangePage = GetIndexStorageParameters<int?>(PostgresExtensions.IndexPagesPerRange, "PagesPerRange");
            if (perRangePage.HasValue)
            {
                parameters.Add($"PAGES_PER_RANGE = {perRangePage}");
            }

            var autosummarize = GetIndexStorageParameters<bool?>(PostgresExtensions.IndexAutosummarize, "Autosummarize");
            if (autosummarize.HasValue)
            {
                parameters.Add($"AUTOSUMMARIZE = {ToOnOff(autosummarize.Value)}");
            }

            // Postgres 11 or Higher
            var cleanup = GetIndexStorageParameters<float?>(PostgresExtensions.IndexVacuumCleanupIndexScaleFactor, "VacuumCleanupIndexScaleFactor");
            if (cleanup.HasValue)
            {
                parameters.Add($"VACUUM_CLEANUP_INDEX_SCALE_FACTOR = {cleanup.Value.ToString(CultureInfo.InvariantCulture)}");
            }

            if (parameters.Count == 0)
            {
                return string.Empty;
            }

            return $" WITH ( {string.Join(", ", parameters)} )";

            string ToOnOff(bool value) => value ? "ON" : "OFF";

            T GetIndexStorageParameters<T>(string indexStorageParameter, string indexStorageParameterName)
            {
                var parameter = expression.Index.GetAdditionalFeature<T>(indexStorageParameter);

                if (parameter != null && !allow.Contains(indexStorageParameter))
                {
                    throw new NotSupportedException($"{indexStorageParameterName} index storage not supported. Please use a new version of Postgres");
                }

                return parameter;
            }
        }

        /// <inheritdoc />
        protected virtual HashSet<string> GetAllowIndexStorageParameters()
        {
            return new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                PostgresExtensions.IndexFillFactor,
                PostgresExtensions.IndexFastUpdate
            };
        }

        /// <inheritdoc />
        public override string Generate(CreateIndexExpression expression)
        {
            var result = new StringBuilder("CREATE");

            if (expression.Index.IsUnique)
            {
                result.Append(" UNIQUE");
            }

            var indexMethod = GetIndexMethod(expression);

            result.AppendFormat(" INDEX{0} {1} ON{2} {3}{4} (",
                GetAsConcurrently(expression),
                Quoter.QuoteIndexName(expression.Index.Name),
                GetAsOnly(expression),
                Quoter.QuoteTableName(expression.Index.TableName, expression.Index.SchemaName),
                // B-Tree is default index method
                indexMethod == Algorithm.BTree ? string.Empty : $" USING {indexMethod.ToString().ToUpper()}");

            var first = true;
            foreach (var column in expression.Index.Columns)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    result.Append(",");
                }

                result.Append(Quoter.QuoteColumnName(column.Name));

                switch (indexMethod)
                {
                    // Doesn't support ASC/DESC neither nulls sorts
                    case Algorithm.Spgist:
                    case Algorithm.Gist:
                    case Algorithm.Gin:
                    case Algorithm.Brin:
                    case Algorithm.Hash:
                        continue;
                }

                result.Append(column.Direction == Direction.Ascending ? " ASC" : " DESC")
                    .Append(GetNullsSort(column));
            }

            result.Append(")")
                .Append(GetIncludeString(expression))
                .Append(GetWithNullsDistinctString(expression.Index))
                .Append(GetWithIndexStorageParameters(expression))
                .Append(GetTablespace(expression))
                .Append(GetFilter(expression));

            AppendSqlStatementEndToken(result);

            return result.ToString();
        }

        /// <inheritdoc />
        public override string Generate(DeleteIndexExpression expression)
        {
            var quotedSchema = Quoter.QuoteSchemaName(expression.Index.SchemaName);
            var quotedIndex = Quoter.QuoteIndexName(expression.Index.Name);
            var indexName = string.IsNullOrEmpty(quotedSchema) ? quotedIndex : $"{quotedSchema}.{quotedIndex}";
            return FormatStatement("DROP INDEX {0}", indexName);
        }

        /// <inheritdoc />
        public override string Generate(InsertDataExpression expression)
        {
            var result = new StringBuilder();
            foreach (var row in expression.Rows)
            {
                var columnNames = new List<string>();
                var columnData = new List<object>();
                foreach (var item in row)
                {
                    columnNames.Add(item.Key);
                    columnData.Add(item.Value);
                }

                var columns = GetColumnList(columnNames);
                var data = GetDataList(columnData);
                result.AppendFormat("INSERT INTO {0} ({1}){3} VALUES ({2})",
                    Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                    columns,
                    data,
                    GetOverridingIdentityValuesString(expression));

                AppendSqlStatementEndToken(result);
            }

            return result.ToString();
        }

        /// <inheritdoc />
        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return FormatStatement(
                "ALTER TABLE {0} ALTER {1} DROP DEFAULT, ALTER {1} {2}",
                Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                Quoter.QuoteColumnName(expression.ColumnName),
                ((PostgresColumn)Column).FormatAlterDefaultValue(expression.ColumnName, expression.DefaultValue));
        }

        /// <inheritdoc />
        public override string Generate(AlterSchemaExpression expression)
        {
            return FormatStatement("ALTER TABLE {0} SET SCHEMA {1}", Quoter.QuoteTableName(expression.TableName, expression.SourceSchemaName), Quoter.QuoteSchemaName(expression.DestinationSchemaName));
        }

        /// <inheritdoc />
        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return FormatStatement("ALTER TABLE {0} ALTER {1} DROP DEFAULT", Quoter.QuoteTableName(expression.TableName, expression.SchemaName), Quoter.Quote(expression.ColumnName));
        }

        /// <inheritdoc />
        public override string Generate(DeleteConstraintExpression expression)
        {
            return FormatStatement("ALTER TABLE {0} DROP CONSTRAINT {1}", Quoter.QuoteTableName(expression.Constraint.TableName, expression.Constraint.SchemaName), Quoter.Quote(expression.Constraint.ConstraintName));
        }

        /// <inheritdoc />
        public override string Generate(CreateConstraintExpression expression)
        {
            var constraintType = (expression.Constraint.IsPrimaryKeyConstraint) ? "PRIMARY KEY" : "UNIQUE";

            string[] columns = new string[expression.Constraint.Columns.Count];

            for (int i = 0; i < expression.Constraint.Columns.Count; i++)
            {
                columns[i] = Quoter.QuoteColumnName(expression.Constraint.Columns.ElementAt(i));
            }

            return FormatStatement(
                "ALTER TABLE {0} ADD CONSTRAINT {1} {2} ({3})",
                Quoter.QuoteTableName(expression.Constraint.TableName, expression.Constraint.SchemaName),
                Quoter.QuoteConstraintName(expression.Constraint.ConstraintName),
                constraintType,
                string.Join(", ", columns));
        }

        /// <inheritdoc />
        protected string GetColumnList(IEnumerable<string> columns)
        {
            var result = "";
            foreach (var column in columns)
            {
                result += Quoter.QuoteColumnName(column) + ",";
            }
            return result.TrimEnd(',');
        }

        /// <inheritdoc />
        protected string GetDataList(List<object> data)
        {
            var result = "";
            foreach (var column in data)
            {
                result += Quoter.QuoteValue(column) + ",";
            }
            return result.TrimEnd(',');
        }

        /// <inheritdoc />
        public override string Generate(CreateSequenceExpression expression)
        {
            var result = new StringBuilder("CREATE SEQUENCE ");
            var seq = expression.Sequence;
            result.AppendFormat(Quoter.QuoteSequenceName(seq.Name, seq.SchemaName));

            if (seq.Increment.HasValue)
            {
                result.AppendFormat(" INCREMENT BY {0}", seq.Increment);
            }

            if (seq.MinValue.HasValue)
            {
                result.AppendFormat(" MINVALUE {0}", seq.MinValue);
            }

            if (seq.MaxValue.HasValue)
            {
                result.AppendFormat(" MAXVALUE {0}", seq.MaxValue);
            }

            if (seq.StartWith.HasValue)
            {
                result.AppendFormat(" START WITH {0}", seq.StartWith);
            }

            const long MINIMUM_CACHE_VALUE = 2;
            if (seq.Cache.HasValue)
            {
                if (seq.Cache.Value < MINIMUM_CACHE_VALUE)
                {
                    return CompatibilityMode.HandleCompatibility("Cache size must be greater than 1; if you intended to disable caching, set Cache to null.");
                }
                result.AppendFormat(" CACHE {0}", seq.Cache);
            }
            else
            {
                result.Append(" CACHE 1");
            }

            if (seq.Cycle)
            {
                result.Append(" CYCLE");
            }

            AppendSqlStatementEndToken(result);

            return result.ToString();
        }

        /// <inheritdoc />
        protected virtual string GetOverridingIdentityValuesString(InsertDataExpression expression)
        {
            if (!expression.AdditionalFeatures.ContainsKey(PostgresExtensions.OverridingIdentityValues))
            {
                return string.Empty;
            }

            throw new NotSupportedException("The current version doesn't support OVERRIDING {SYSTEM|USER} VALUE. Please use Postgres 10+.");
        }
    }
}
