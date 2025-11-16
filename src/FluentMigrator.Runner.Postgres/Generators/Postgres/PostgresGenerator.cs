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

        /// <summary>
        /// Generates the SQL statement to alter a column in a PostgreSQL database.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="FluentMigrator.Expressions.AlterColumnExpression"/> containing the details of the column alteration.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> representing the SQL statement to alter the column.
        /// </returns>
        /// <remarks>
        /// This method handles compatibility issues, such as unsupported virtual computed columns, 
        /// and appends additional SQL statements for column descriptions if applicable.
        /// </remarks>
        public override string Generate(AlterColumnExpression expression)
        {
            if (expression.Column.Expression != null && !expression.Column.ExpressionStored)
            {
                CompatibilityMode.HandleCompatibility("Virtual computed columns are not supported");
            }
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

        /// <summary>
        /// Generates the "INCLUDE" clause for a PostgreSQL index creation statement.
        /// </summary>
        /// <param name="column">The <see cref="CreateIndexExpression"/> containing the index definition.</param>
        /// <returns>
        /// A string representing the "INCLUDE" clause for the index, or an empty string if no includes are defined.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when the current version of PostgreSQL does not support include indexes.
        /// </exception>
        /// <remarks>
        /// This method retrieves additional features from the <paramref name="column"/> using the key
        /// <see cref="PostgresExtensions.IncludesList"/>. If no includes are defined, an empty string is returned.
        /// </remarks>
        protected virtual string GetIncludeString(CreateIndexExpression column)
        {
            var includes = column.GetAdditionalFeature<IList<PostgresIndexIncludeDefinition>>(PostgresExtensions.IncludesList);

            if (includes == null || includes.Count == 0)
            {
                return string.Empty;
            }

            throw new NotSupportedException("The current version doesn't support include index. Please use Postgres 11.");
        }

        /// <summary>
        /// Determines the index algorithm to be used for a PostgreSQL index creation based on the provided
        /// <see cref="FluentMigrator.Expressions.CreateIndexExpression"/>.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="FluentMigrator.Expressions.CreateIndexExpression"/> containing the index definition and additional features.
        /// </param>
        /// <returns>
        /// The <see cref="FluentMigrator.Model.Algorithm"/> to be used for the index. Defaults to <see cref="FluentMigrator.Model.Algorithm.BTree"/>
        /// if no specific algorithm is defined.
        /// </returns>
        /// <remarks>
        /// This method retrieves the algorithm from the additional features of the <paramref name="expression"/> using
        /// the <c>PostgresExtensions.IndexAlgorithm</c> key. If no algorithm is specified, it defaults to <see cref="FluentMigrator.Model.Algorithm.BTree"/>.
        /// </remarks>
        protected virtual Algorithm GetIndexMethod(CreateIndexExpression expression)
        {
            var algorithm = expression.GetAdditionalFeature<PostgresIndexAlgorithmDefinition>(PostgresExtensions.IndexAlgorithm);
            if (algorithm == null)
            {
                return Algorithm.BTree;
            }

            return algorithm.Algorithm;
        }

        /// <summary>
        /// Generates a filter clause for a PostgreSQL index creation statement based on the provided
        /// <see cref="CreateIndexExpression"/>. The filter clause is derived from additional features
        /// or specific PostgreSQL index options, such as "WITH NULLS DISTINCT".
        /// </summary>
        /// <param name="expression">
        /// The <see cref="CreateIndexExpression"/> containing the index definition and additional features.
        /// </param>
        /// <returns>
        /// A string representing the filter clause for the index, or an empty string if the filter cannot
        /// be combined with certain PostgreSQL options. Returns <c>null</c> if no filter or relevant options are specified.
        /// </returns>
        /// <remarks>
        /// If both a filter and a "WITH NULLS DISTINCT" option are specified, compatibility issues with
        /// PostgreSQL 14 or older are handled by returning an empty string.
        /// </remarks>
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

        /// <summary>
        /// Generates a SQL condition string for handling "nulls distinct" behavior in a PostgreSQL index.
        /// </summary>
        /// <param name="index">The <see cref="IndexDefinition"/> representing the index for which the condition is generated.</param>
        /// <returns>
        /// A SQL condition string to be included in the WHERE clause of the index definition, 
        /// or an empty string if no "nulls distinct" behavior is applicable.
        /// </returns>
        /// <remarks>
        /// This method evaluates the "nulls distinct" feature for both the index and its columns.
        /// If "nulls distinct" is enabled, it ensures that only non-null values are considered for the index.
        /// This feature is only applicable for unique indexes.
        /// </remarks>
        /// <exception cref="CompatibilityModeExtension">
        /// Thrown if "nulls distinct" is used on a non-unique index, as this is not supported.
        /// </exception>
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

        /// <summary>
        /// Generates a SQL fragment for handling "NULLS DISTINCT" behavior in PostgreSQL index definitions.
        /// </summary>
        /// <param name="index">The <see cref="IndexDefinition"/> representing the index for which the SQL fragment is generated.</param>
        /// <returns>A SQL fragment string that specifies the "NULLS DISTINCT" behavior for the index, or an empty string if not applicable.</returns>
        /// <remarks>
        /// This method is intended to be overridden in derived classes to provide specific behavior for different PostgreSQL versions.
        /// </remarks>
        protected virtual string GetWithNullsDistinctString(IndexDefinition index)
        {
            return string.Empty;
        }

        /// <summary>
        /// Generates the "CONCURRENTLY" keyword for a PostgreSQL "CREATE INDEX" statement
        /// if the <see cref="CreateIndexExpression"/> specifies the "Concurrently" feature.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="CreateIndexExpression"/> containing the index definition and additional features.
        /// </param>
        /// <returns>
        /// A string containing " CONCURRENTLY" if the "Concurrently" feature is enabled; otherwise, an empty string.
        /// </returns>
        /// <remarks>
        /// This method checks for the presence of the "Concurrently" feature in the additional features
        /// of the provided <see cref="CreateIndexExpression"/>. If the feature is enabled, it appends
        /// the "CONCURRENTLY" keyword to the generated SQL.
        /// </remarks>
        protected virtual string GetAsConcurrently(CreateIndexExpression expression)
        {
            var asConcurrently = expression.GetAdditionalFeature<PostgresIndexConcurrentlyDefinition>(PostgresExtensions.Concurrently);

            if (asConcurrently == null || !asConcurrently.IsConcurrently)
            {
                return string.Empty;
            }

            return " CONCURRENTLY";
        }

        /// <summary>
        /// Generates the "ONLY" clause for a PostgreSQL index creation statement if the index is restricted to a specific table.
        /// </summary>
        /// <param name="expression">The <see cref="CreateIndexExpression"/> containing the index definition.</param>
        /// <returns>
        /// A string representing the "ONLY" clause if applicable; otherwise, an empty string.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when the "ONLY" clause is requested but the PostgreSQL version does not support it.
        /// </exception>
        protected virtual string GetAsOnly(CreateIndexExpression expression)
        {
            var asOnly = expression.GetAdditionalFeature<PostgresIndexOnlyDefinition>(PostgresExtensions.Only);

            if (asOnly == null || !asOnly.IsOnly)
            {
                return string.Empty;
            }

            throw new NotSupportedException("The current version doesn't support ONLY. Please use Postgres 11 or higher.");
        }

        /// <summary>
        /// Generates the SQL fragment for specifying the sorting of NULL values in an index column.
        /// </summary>
        /// <param name="column">The <see cref="IndexColumnDefinition"/> representing the index column.</param>
        /// <returns>
        /// A string containing the SQL fragment for NULL sorting, such as "NULLS FIRST" or "NULLS LAST",
        /// or an empty string if no NULL sorting is specified.
        /// </returns>
        /// <remarks>
        /// This method retrieves the NULL sorting behavior from the additional features of the column
        /// using the key <see cref="PostgresExtensions.NullsSort"/>.
        /// </remarks>
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

        /// <summary>
        /// Retrieves the tablespace definition for a PostgreSQL index creation statement.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="FluentMigrator.Expressions.CreateIndexExpression"/> containing the index definition.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> representing the tablespace clause for the index, or an empty string if no tablespace is specified.
        /// </returns>
        /// <remarks>
        /// The method checks for an additional feature named <c>PostgresExtensions.IndexTablespace</c> in the index definition.
        /// If a valid tablespace is specified, it returns a formatted "TABLESPACE" clause; otherwise, it returns an empty string.
        /// </remarks>
        protected virtual string GetTablespace(CreateIndexExpression expression)
        {
            var tablespace = expression.Index.GetAdditionalFeature<string>(PostgresExtensions.IndexTablespace);
            if (!string.IsNullOrWhiteSpace(tablespace))
            {
                return " TABLESPACE " + tablespace;
            }

            return string.Empty;
        }

        /// <summary>
        /// Generates a string representation of the storage parameters for an index in PostgreSQL.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="FluentMigrator.Expressions.CreateIndexExpression"/> containing the index definition
        /// and additional features to be included in the storage parameters.
        /// </param>
        /// <returns>
        /// A string containing the formatted storage parameters for the index, or an empty string if no parameters are specified.
        /// </returns>
        /// <remarks>
        /// This method processes various PostgreSQL-specific index storage parameters, such as fill factor, fast update,
        /// buffering, pending list limit, pages per range, auto-summarize, and vacuum cleanup index scale factor.
        /// It ensures that only allowed parameters are included in the final output.
        /// </remarks>
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

        /// <summary>
        /// Retrieves a set of allowed index storage parameters for PostgreSQL.
        /// </summary>
        /// <remarks>
        /// This method defines the default set of index storage parameters that are supported
        /// by PostgreSQL. Derived classes can override this method to extend or modify the
        /// list of allowed parameters.
        /// </remarks>
        /// <returns>
        /// A <see cref="HashSet{T}"/> containing the names of the allowed index storage parameters,
        /// using a case-insensitive string comparer.
        /// </returns>
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

        /// <summary>
        /// Generates a comma-separated list of quoted column names for use in SQL statements.
        /// </summary>
        /// <param name="columns">The collection of column names to be quoted and concatenated.</param>
        /// <returns>A string containing the quoted column names, separated by commas.</returns>
        protected string GetColumnList(IEnumerable<string> columns)
        {
            var result = "";
            foreach (var column in columns)
            {
                result += Quoter.QuoteColumnName(column) + ",";
            }
            return result.TrimEnd(',');
        }

        /// <summary>
        /// Constructs a comma-separated list of quoted values from the provided data.
        /// </summary>
        /// <param name="data">The list of objects to be quoted and concatenated.</param>
        /// <returns>A string containing the quoted values, separated by commas.</returns>
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

        /// <summary>
        /// Generates a string representing the "OVERRIDING {SYSTEM|USER} VALUE" clause for an INSERT statement
        /// in PostgreSQL, based on the provided <see cref="InsertDataExpression"/>.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="InsertDataExpression"/> containing the data and additional features
        /// for the INSERT operation.
        /// </param>
        /// <returns>
        /// A string representing the "OVERRIDING {SYSTEM|USER} VALUE" clause if the feature is specified;
        /// otherwise, an empty string.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if the feature is specified but the current PostgreSQL version does not support
        /// "OVERRIDING {SYSTEM|USER} VALUE".
        /// </exception>
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
