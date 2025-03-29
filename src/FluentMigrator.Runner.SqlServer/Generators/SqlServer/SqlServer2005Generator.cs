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
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.SqlServer;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.SqlServer
{
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

        public SqlServer2005Generator()
            : this(new SqlServer2005Quoter())
        {
        }

        public SqlServer2005Generator(
            [NotNull] SqlServer2005Quoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

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


        public override string AddColumn { get { return "ALTER TABLE {0} ADD {1}"; } }

        public override string CreateSchema { get { return "CREATE SCHEMA {0}{1}"; } }

        public override string CreateIndex { get { return "CREATE {0}{1}INDEX {2} ON {3} ({4}){5}{6}{7}"; } }
        public override string DropIndex { get { return "DROP INDEX {0} ON {1}{2}"; } }

        public override string IdentityInsert { get { return "SET IDENTITY_INSERT {0} {1}"; } }

        public virtual string CreateUniqueConstraint { get { return "CREATE UNIQUE INDEX {1} ON {0} ({2}){3}{4}"; } }

        public override string CreateForeignKeyConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6}"; } }

        public virtual string GetIncludeString(CreateIndexExpression expression)
        {
            return GetIncludeStringFor(expression, SqlServerExtensions.IncludesList);
        }

        public virtual string GetIncludeString(CreateConstraintExpression expression)
        {
            return GetIncludeStringFor(expression, SqlServerExtensions.UniqueConstraintIncludesList);
        }

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

        public override bool IsAdditionalFeatureSupported(string feature)
        {
            return _supportedAdditionalFeatures.Contains(feature)
             || base.IsAdditionalFeatureSupported(feature);
        }

        public override string Generate(CreateTableExpression expression)
        {
            var descriptionStatements = DescriptionGenerator.GenerateDescriptionStatements(expression);
            var createTableStatement = base.Generate(expression);
            var descriptionStatementsArray = descriptionStatements as string[] ?? descriptionStatements.ToArray();

            if (!descriptionStatementsArray.Any())
                return createTableStatement;

            return ComposeStatements(createTableStatement, descriptionStatementsArray);
        }

        public override string Generate(AlterTableExpression expression)
        {
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return base.Generate(expression);

            return descriptionStatement;
        }

        public override string Generate(DeleteTableExpression expression)
        {
            if (expression.IfExists)
            {
                return string.Format("IF OBJECT_ID('{0}','U') IS NOT NULL DROP TABLE {0}", Quoter.QuoteTableName(expression.TableName, expression.SchemaName));

            }

            return $"DROP TABLE {Quoter.QuoteTableName(expression.TableName, expression.SchemaName)}";
        }

        public override string Generate(CreateColumnExpression expression)
        {
            var alterTableStatement = base.Generate(expression);
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return alterTableStatement;

            return ComposeStatements(alterTableStatement, new[] { descriptionStatement });
        }

        public override string Generate(AlterColumnExpression expression)
        {
            var alterTableStatement = base.Generate(expression);
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return alterTableStatement;

            return ComposeStatements(alterTableStatement, new[] { descriptionStatement });
        }

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
            return string.Format(
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

            var withParts = GetWithOptions(expression);
            var withPart = !string.IsNullOrEmpty(withParts)
                ? $" WITH ({withParts})"
                : string.Empty;

            var result = string.Format(
                CreateIndex,
                GetUniqueString(expression),
                GetClusterTypeString(expression),
                Quoter.QuoteIndexName(expression.Index.Name),
                Quoter.QuoteTableName(expression.Index.TableName, expression.Index.SchemaName),
                string.Join(", ", indexColumns),
                GetIncludeString(expression),
                GetFilterString(expression),
                withPart);

            return result;
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            var withParts = GetWithOptions(expression);
            var withPart = !string.IsNullOrEmpty(withParts)
                ? $" WITH ({withParts})"
                : string.Empty;

            return string.Format(
                DropIndex,
                Quoter.QuoteIndexName(expression.Index.Name),
                Quoter.QuoteTableName(expression.Index.TableName, expression.Index.SchemaName),
                withPart);
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            var filterParts = GetFilterString(expression);
            var includeParts = GetIncludeString(expression);
            var columns = string.Join(", ", expression.Constraint.Columns.Select(x => Quoter.QuoteColumnName(x)).ToArray());

            if (expression.Constraint.IsUniqueConstraint)
                if (expression.Constraint.TryGetAdditionalFeature<string>(SqlServerExtensions.UniqueConstraintFilter, out _) ||
                    expression.Constraint.TryGetAdditionalFeature<IList<IndexIncludeDefinition>>(SqlServerExtensions.UniqueConstraintIncludesList, out _))
                    return
                        string.Format(
                            CreateUniqueConstraint,
                            Quoter.QuoteTableName(expression.Constraint.TableName, expression.Constraint.SchemaName),
                            Quoter.QuoteConstraintName(expression.Constraint.ConstraintName),
                            columns,
                            includeParts,
                            filterParts
                        );
            
            var withParts = GetWithOptions(expression);
            var withPart = !string.IsNullOrEmpty(withParts)
                ? $" WITH ({withParts})"
                : string.Empty;

            return $"{base.Generate(expression)}{filterParts}{withPart}";
        }

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

        public override string Generate(DeleteConstraintExpression expression)
        {
            var withParts = GetWithOptions(expression);
            var withPart = !string.IsNullOrEmpty(withParts)
                ? $" WITH ({withParts})"
                : string.Empty;

            return $"{base.Generate(expression)}{withPart}";
        }

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

            return string.Format(CreateSchema, Quoter.QuoteSchemaName(expression.SchemaName), authFragment);
        }

        public override string Generate(DeleteSchemaExpression expression)
        {
            return string.Format(DropSchema, Quoter.QuoteSchemaName(expression.SchemaName));
        }

        public override string Generate(AlterSchemaExpression expression)
        {
            return string.Format(AlterSchema, Quoter.QuoteSchemaName(expression.DestinationSchemaName), Quoter.QuoteTableName(expression.TableName, expression.SourceSchemaName));
        }

        private string ComposeStatements(string ddlStatement, IEnumerable<string> otherStatements)
        {
            var otherStatementsArray = otherStatements.ToArray();

            var statementsBuilder = new StringBuilder();
            statementsBuilder.AppendLine(ddlStatement);
            statementsBuilder.AppendLine("GO");
            statementsBuilder.AppendLine(string.Join(";", otherStatementsArray));

            return statementsBuilder.ToString();
        }
    }
}
