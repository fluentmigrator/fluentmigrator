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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    /// <summary>
    /// The SQL Server 2005 SQL generator for FluentMigrator.
    /// </summary>
    public class SqlServer2005Generator : SqlServer2000Generator
    {
        private const string ErrorMessageFilteredIndexesAreNonClusteredIndexes =
            "Filtered indexes are non-clustered indexes that have the addition of a WHERE clause. "
          + "SQL Server does not support clustered filtered indexes. "
          + "Create a non-clustered index with include columns instead to create a non-clustered covering index.";
        private static readonly HashSet<string> _supportedAdditionalFeatures = new HashSet<string>
        {
            SqlServerExtensions.IncludesList,
            SqlServerExtensions.OnlineIndex,
            SqlServerExtensions.RowGuidColumn,
            SqlServerExtensions.SchemaAuthorization,
            SqlServerExtensions.ConstraintType,
            SqlServerExtensions.UniqueConstraintFilter,
            SqlServerExtensions.UniqueConstraintIncludesList
        };

        /// <inheritdoc />
        public SqlServer2005Generator()
            : this(new SqlServer2005Quoter())
        {
        }

        /// <inheritdoc />
        public SqlServer2005Generator(
            [NotNull] SqlServer2005Quoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        /// <inheritdoc />
        public SqlServer2005Generator(
            [NotNull] SqlServer2005Quoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : this(
                new SqlServer2005Column(new SqlServer2005TypeMap(), quoter),
                quoter,
                new SqlServer2005DescriptionGenerator(),
                generatorOptions)
        {
        }

        /// <inheritdoc />
        protected SqlServer2005Generator(
            [NotNull] IColumn column,
            [NotNull] IQuoter quoter,
            [NotNull] IDescriptionGenerator descriptionGenerator,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, descriptionGenerator, generatorOptions)
        {
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.SqlServer2005;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases =>
            [GeneratorIdConstants.SqlServer2005, GeneratorIdConstants.SqlServer];

        /// <inheritdoc />
        public override string AddColumn => "ALTER TABLE {0} ADD {1}";

        /// <inheritdoc />
        public override string CreateSchema => "CREATE SCHEMA {0}{1}";

        /// <inheritdoc />
        public override string CreateIndex => "CREATE {0}{1}INDEX {2} ON {3} ({4}){5}{6}{7}";

        /// <inheritdoc />
        public override string DropIndex => "DROP INDEX {0} ON {1}{2}";

        /// <inheritdoc />
        public override string IdentityInsert => "SET IDENTITY_INSERT {0} {1}";

        /// <summary>
        /// Gets the SQL for creating a unique constraint.
        /// </summary>
        public virtual string CreateUniqueConstraint => "CREATE UNIQUE INDEX {1} ON {0} ({2}){3}{4}";

        /// <inheritdoc />
        public override string CreateForeignKeyConstraint => "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6}";

        /// <inheritdoc />
        public virtual string GetIncludeString(CreateIndexExpression expression)
        {
            return GetIncludeStringFor(expression, SqlServerExtensions.IncludesList);
        }

        /// <inheritdoc />
        public virtual string GetIncludeString(CreateConstraintExpression expression)
        {
            return GetIncludeStringFor(expression, SqlServerExtensions.UniqueConstraintIncludesList);
        }

        /// <summary>
        /// Gets the include string for additional features.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="featureKey">The feature key.</param>
        /// <returns>The include string.</returns>
        protected string GetIncludeStringFor(ISupportAdditionalFeatures expression, string featureKey)
        {
            if (!expression.TryGetAdditionalFeature<IList<IndexIncludeDefinition>>(featureKey, out var includes))
                return string.Empty;

            var indexIncludes = new string[includes?.Count ?? 0];

            if (includes != null)
            {
                for (var i = 0; i != includes.Count; i++)
                {
                    var includeDef = includes[i];
                    indexIncludes[i] = Quoter.QuoteColumnName(includeDef.Name);
                }
            }

            return includes?.Count > 0 ? " INCLUDE (" + string.Join(", ", indexIncludes) + ")" : string.Empty;
        }

        /// <inheritdoc />
        public virtual string GetFilterString(CreateIndexExpression createIndexExpression)
        {
            if (createIndexExpression.Index.TryGetAdditionalFeature<string>(SqlServerExtensions.IndexFilter, out var filter))
            {
                if (createIndexExpression.Index.IsClustered)
                    throw new Exception(ErrorMessageFilteredIndexesAreNonClusteredIndexes);

                return " WHERE " + filter;
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public virtual string GetFilterString(CreateConstraintExpression createConstraintExpression)
        {
            if (createConstraintExpression.Constraint.TryGetAdditionalFeature<string>(SqlServerExtensions.UniqueConstraintFilter, out var filter))
            {
                if (createConstraintExpression.Constraint.IsUniqueConstraint)
                {
                    if (!createConstraintExpression.Constraint.TryGetAdditionalFeature<SqlServerConstraintType>(
                            SqlServerExtensions.ConstraintType,
                            out var indexType)
                     || indexType == SqlServerConstraintType.NonClustered)
                    {
                        return " WHERE " + filter;
                    }

                    throw new Exception(ErrorMessageFilteredIndexesAreNonClusteredIndexes);
                }

                throw new InvalidOperationException(
                    "Create Constraint Expressions with Filters are only supported for Unique Indexes.");
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public virtual string GetWithOptions(ISupportAdditionalFeatures expression)
        {
            var items = new List<string>();
            var isOnline = expression.GetAdditionalFeature(SqlServerExtensions.OnlineIndex, (bool?)null);
            if (isOnline.HasValue)
            {
                items.Add($"ONLINE={(isOnline.Value ? "ON" : "OFF")}");
            }

            return string.Join(", ", items);
        }

        /// <inheritdoc />
        public override bool IsAdditionalFeatureSupported(string feature)
        {
            return _supportedAdditionalFeatures.Contains(feature)
             || base.IsAdditionalFeatureSupported(feature);
        }

        /// <inheritdoc />
        public override string Generate(CreateTableExpression expression)
        {
            var descriptionStatements = DescriptionGenerator.GenerateDescriptionStatements(expression);
            var createTableStatement = base.Generate(expression);
            var descriptionStatementsArray = descriptionStatements as string[] ?? descriptionStatements.ToArray();

            if (!descriptionStatementsArray.Any())
                return createTableStatement;

            return ComposeStatements(createTableStatement, descriptionStatementsArray);
        }

        /// <inheritdoc />
        public override string Generate(AlterTableExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return base.Generate(expression);

            return descriptionStatement;
        }

        /// <inheritdoc />
        public override string Generate(DeleteTableExpression expression)
        {
            if (expression.IfExists)
            {
                return FormatStatement("IF OBJECT_ID('{0}','U') IS NOT NULL DROP TABLE {0}", Quoter.QuoteTableName(expression.TableName, expression.SchemaName));
            }

            return FormatStatement(DropTable, Quoter.QuoteTableName(expression.TableName, expression.SchemaName));
        }

        /// <inheritdoc />
        public override string Generate(CreateColumnExpression expression)
        {
            var alterTableStatement = base.Generate(expression);
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return alterTableStatement;

            return ComposeStatements(alterTableStatement, new[] { descriptionStatement });
        }

        /// <inheritdoc />
        public override string Generate(AlterColumnExpression expression)
        {
            var alterTableStatement = base.Generate(expression);
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return alterTableStatement;

            return ComposeStatements(alterTableStatement, new[] { descriptionStatement });
        }

        /// <inheritdoc />
        public override string Generate(CreateForeignKeyExpression expression)
        {
            if (expression.ForeignKey.PrimaryColumns.Count != expression.ForeignKey.ForeignColumns.Count)
            {
                throw new ArgumentException("Number of primary columns and secondary columns must be equal");
            }

            List<string> primaryColumns = new List<string>();
            List<string> foreignColumns = new List<string>();
            foreach (var column in expression.ForeignKey.PrimaryColumns)
            {
                primaryColumns.Add(Quoter.QuoteColumnName(column));
            }

            foreach (var column in expression.ForeignKey.ForeignColumns)
            {
                foreignColumns.Add(Quoter.QuoteColumnName(column));
            }
            return FormatStatement(
                CreateForeignKeyConstraint,
                Quoter.QuoteTableName(expression.ForeignKey.ForeignTable, expression.ForeignKey.ForeignTableSchema),
                Quoter.QuoteColumnName(expression.ForeignKey.Name),
                string.Join(", ", foreignColumns.ToArray()),
                Quoter.QuoteTableName(expression.ForeignKey.PrimaryTable, expression.ForeignKey.PrimaryTableSchema),
                string.Join(", ", primaryColumns.ToArray()),
                Column.FormatCascade("DELETE", expression.ForeignKey.OnDelete),
                Column.FormatCascade("UPDATE", expression.ForeignKey.OnUpdate)
                );
        }

        /// <inheritdoc />
        public override string Generate(CreateIndexExpression expression)
        {
            string[] indexColumns = new string[expression.Index.Columns.Count];
            IndexColumnDefinition columnDef;


            for (int i = 0; i < expression.Index.Columns.Count; i++)
            {
                columnDef = expression.Index.Columns.ElementAt(i);
                if (columnDef.Direction == Direction.Ascending)
                {
                    indexColumns[i] = Quoter.QuoteColumnName(columnDef.Name) + " ASC";
                }
                else
                {
                    indexColumns[i] = Quoter.QuoteColumnName(columnDef.Name) + " DESC";
                }
            }

            return FormatStatement(
                CreateIndex,
                GetUniqueString(expression),
                GetClusterTypeString(expression),
                Quoter.QuoteIndexName(expression.Index.Name),
                Quoter.QuoteTableName(expression.Index.TableName, expression.Index.SchemaName),
                string.Join(", ", indexColumns),
                GetIncludeString(expression),
                GetFilterString(expression),
                GetWithPart(expression));
        }

        /// <inheritdoc />
        public override string Generate(DeleteIndexExpression expression)
        {
            return FormatStatement(
                DropIndex,
                Quoter.QuoteIndexName(expression.Index.Name),
                Quoter.QuoteTableName(expression.Index.TableName, expression.Index.SchemaName),
                GetWithPart(expression));
        }

        private string GetWithPart(ISupportAdditionalFeatures expression)
        {
            var withParts = GetWithOptions(expression);

            return !string.IsNullOrEmpty(withParts)
                ? $" WITH ({withParts})"
                : string.Empty;
        }

        /// <inheritdoc />
        public override string Generate(CreateConstraintExpression expression)
        {
            var filterParts = GetFilterString(expression);
            var includeParts = GetIncludeString(expression);
            var columns = string.Join(", ", expression.Constraint.Columns.Select(x => Quoter.QuoteColumnName(x)).ToArray());

            if (expression.Constraint.IsUniqueConstraint &&
                (
                    expression.Constraint.TryGetAdditionalFeature<string>(SqlServerExtensions.UniqueConstraintFilter, out _) ||
                    expression.Constraint.TryGetAdditionalFeature<IList<IndexIncludeDefinition>>(SqlServerExtensions.UniqueConstraintIncludesList, out _)
                ))
            {
                return FormatStatement(
                    CreateUniqueConstraint,
                    Quoter.QuoteTableName(expression.Constraint.TableName, expression.Constraint.SchemaName),
                    Quoter.QuoteConstraintName(expression.Constraint.ConstraintName),
                    columns,
                    includeParts,
                    filterParts
                );
            }

            var statement = GenerateCreateConstraintPart(expression);

            statement.Append(GetWithPart(expression));

            AppendSqlStatementEndToken(statement);

            return statement.ToString();
        }

        /// <inheritdoc />
        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            string sql =
                "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
                "-- get name of default constraint" + Environment.NewLine +
                "SELECT @default = name" + Environment.NewLine +
                "FROM sys.default_constraints" + Environment.NewLine +
                "WHERE parent_object_id = object_id('{0}')" + Environment.NewLine +
                "AND type = 'D'" + Environment.NewLine +
                "AND parent_column_id = (" + Environment.NewLine +
                "SELECT column_id" + Environment.NewLine +
                "FROM sys.columns" + Environment.NewLine +
                "WHERE object_id = object_id('{0}')" + Environment.NewLine +
                "AND name = '{1}'" + Environment.NewLine +
                ");" + Environment.NewLine + Environment.NewLine +
                "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                "SET @sql = N'ALTER TABLE {0} DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
                "EXEC sp_executesql @sql;";
            return string.Format(sql, Quoter.QuoteTableName(expression.TableName, expression.SchemaName), expression.ColumnName);
        }

        /// <inheritdoc />
        public override string Generate(DeleteConstraintExpression expression)
        {
            return FormatStatement("ALTER TABLE {0} DROP CONSTRAINT {1}{2}",
                Quoter.QuoteTableName(expression.Constraint.TableName, expression.Constraint.SchemaName),
                Quoter.QuoteConstraintName(expression.Constraint.ConstraintName),
                GetWithPart(expression)
            );
        }

        /// <inheritdoc />
        public override string Generate(CreateSchemaExpression expression)
        {
            string authFragment;
            if (expression.AdditionalFeatures.TryGetValue(SqlServerExtensions.SchemaAuthorization, out var authorization))
            {
                authFragment = $" AUTHORIZATION {Quoter.QuoteSchemaName((string)authorization)}";
            }
            else
            {
                authFragment = string.Empty;
            }

            return FormatStatement(CreateSchema, Quoter.QuoteSchemaName(expression.SchemaName), authFragment);
        }

        /// <inheritdoc />
        public override string Generate(DeleteSchemaExpression expression)
        {
            return FormatStatement(DropSchema, Quoter.QuoteSchemaName(expression.SchemaName));
        }

        /// <inheritdoc />
        public override string Generate(AlterSchemaExpression expression)
        {
            return FormatStatement(AlterSchema, Quoter.QuoteSchemaName(expression.DestinationSchemaName), Quoter.QuoteTableName(expression.TableName, expression.SourceSchemaName));
        }

        private string ComposeStatements(string ddlStatement, IEnumerable<string> otherStatements)
        {
            var otherStatementsArray = otherStatements.ToArray();

            var statementsBuilder = new StringBuilder();
            statementsBuilder.AppendLine(ddlStatement);
            statementsBuilder.AppendLine("GO");
            statementsBuilder.Append(string.Join("", otherStatementsArray));

            return statementsBuilder.ToString();
        }
    }
}
