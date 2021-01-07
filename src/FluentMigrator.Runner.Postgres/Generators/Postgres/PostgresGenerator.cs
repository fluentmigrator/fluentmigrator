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
using System.Linq;
using System.Text;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Postgres;
using FluentMigrator.Runner.Generators.Generic;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.Postgres
{
    public class PostgresGenerator : GenericGenerator
    {
        public PostgresGenerator(
            [NotNull] PostgresQuoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public PostgresGenerator(
            [NotNull] PostgresQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new PostgresColumn(quoter, new PostgresTypeMap()), quoter, new PostgresDescriptionGenerator(quoter), generatorOptions)
        {
        }

        protected PostgresGenerator(
            [NotNull] PostgresQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions,
            ITypeMap typeMap)
            : base(new PostgresColumn(quoter, typeMap), quoter, new PostgresDescriptionGenerator(quoter), generatorOptions)
        {
        }

        protected PostgresGenerator(
            [NotNull] IColumn column,
            [NotNull] PostgresQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, new PostgresDescriptionGenerator(quoter), generatorOptions)
        {
        }

        public override string CreateTable { get { return "CREATE TABLE {0} ({1})"; } }
        public override string DropTable { get { return "DROP TABLE {0};"; } }

        public override string AddColumn { get { return "ALTER TABLE {0} ADD {1};"; } }
        public override string DropColumn { get { return "ALTER TABLE {0} DROP COLUMN {1};"; } }
        public override string AlterColumn { get { return "ALTER TABLE {0} {1};"; } }
        public override string RenameColumn { get { return "ALTER TABLE {0} RENAME COLUMN {1} TO {2};"; } }

        public override string Generate(AlterTableExpression expression)
        {
            var alterStatement = new StringBuilder();
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);
            alterStatement.Append(base.Generate(expression));
            if (string.IsNullOrEmpty(descriptionStatement))
            {
                alterStatement.Append(descriptionStatement);
            }
            return alterStatement.ToString();
        }

        public override string Generate(CreateSchemaExpression expression)
        {
            return string.Format("CREATE SCHEMA {0};", Quoter.QuoteSchemaName(expression.SchemaName));
        }

        public override string Generate(DeleteSchemaExpression expression)
        {
            return string.Format("DROP SCHEMA {0};", Quoter.QuoteSchemaName(expression.SchemaName));
        }

        public override string Generate(CreateTableExpression expression)
        {
            var createStatement = new StringBuilder();
            createStatement.AppendFormat(
                CreateTable,
                Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                Column.Generate(expression.Columns, Quoter.Quote(expression.TableName)));
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatements(expression)
                ?.ToList();
            createStatement.Append(";");

            if (descriptionStatement != null && descriptionStatement.Count != 0)
            {
                createStatement.Append(string.Join(";", descriptionStatement.ToArray()));
                createStatement.Append(";");
            }
            return createStatement.ToString();
        }

        public override string Generate(AlterColumnExpression expression)
        {
            var alterStatement = new StringBuilder();
            alterStatement.AppendFormat(
                AlterColumn,
                Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                ((PostgresColumn)Column).GenerateAlterClauses(expression.Column));
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);
            if (!string.IsNullOrEmpty(descriptionStatement))
            {
                alterStatement.Append(";");
                alterStatement.Append(descriptionStatement);
            }
            return alterStatement.ToString();
        }

        public override string Generate(CreateColumnExpression expression)
        {
            var createStatement = new StringBuilder();
            createStatement.AppendFormat(base.Generate(expression));

            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);
            if (!string.IsNullOrEmpty(descriptionStatement))
            {
                createStatement.Append(";");
                createStatement.Append(descriptionStatement);
            }

            return createStatement.ToString();
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string columnName in expression.ColumnNames)
            {
                if (expression.ColumnNames.First() != columnName) builder.AppendLine("");
                builder.AppendFormat(DropColumn,
                    Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                    Quoter.QuoteColumnName(columnName));
            }
            return builder.ToString();
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            var primaryColumns = GetColumnList(expression.ForeignKey.PrimaryColumns);
            var foreignColumns = GetColumnList(expression.ForeignKey.ForeignColumns);

            const string sql = "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6};";

            return string.Format(sql,
                Quoter.QuoteTableName(expression.ForeignKey.ForeignTable, expression.ForeignKey.ForeignTableSchema),
                Quoter.Quote(expression.ForeignKey.Name),
                foreignColumns,
                Quoter.QuoteTableName(expression.ForeignKey.PrimaryTable, expression.ForeignKey.PrimaryTableSchema),
                primaryColumns,
                Column.FormatCascade("DELETE", expression.ForeignKey.OnDelete),
                Column.FormatCascade("UPDATE", expression.ForeignKey.OnUpdate)
            );
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            return string.Format("ALTER TABLE {0} DROP CONSTRAINT {1};",
                Quoter.QuoteTableName(expression.ForeignKey.ForeignTable, expression.ForeignKey.ForeignTableSchema),
                Quoter.Quote(expression.ForeignKey.Name));
        }


        protected virtual string GetIncludeString(CreateIndexExpression column)
        {
            var includes = column.GetAdditionalFeature<IList<PostgresIndexIncludeDefinition>>(PostgresExtensions.IncludesList);

            if (includes == null || includes.Count == 0)
            {
                return string.Empty;
            }

            throw new NotSupportedException("The current version doesn't support include index. Please use Postgres 11.");
        }

        protected virtual Algorithm GetIndexMethod(CreateIndexExpression expression)
        {
            var algorithm = expression.GetAdditionalFeature<PostgresIndexAlgorithmDefinition>(PostgresExtensions.IndexAlgorithm);
            if (algorithm == null)
            {
                return Algorithm.BTree;
            }

            return algorithm.Algorithm;
        }

        protected virtual string GetFilter(CreateIndexExpression expression)
        {
            var filter = expression.Index.GetAdditionalFeature<string>(PostgresExtensions.IndexFilter);
            if (!string.IsNullOrWhiteSpace(filter))
            {
                return " WHERE " + filter;
            }

            return string.Empty;
        }

        protected virtual string GetAsConcurrently(CreateIndexExpression expression)
        {
            var asConcurrently = expression.GetAdditionalFeature<PostgresIndexConcurrentlyDefinition>(PostgresExtensions.Concurrently);

            if (asConcurrently == null || !asConcurrently.IsConcurrently)
            {
                return string.Empty;
            }

            return " CONCURRENTLY";
        }

        protected virtual string GetAsOnly(CreateIndexExpression expression)
        {
            var asOnly = expression.GetAdditionalFeature<PostgresIndexOnlyDefinition>(PostgresExtensions.Only);

            if (asOnly == null || !asOnly.IsOnly)
            {
                return string.Empty;
            }

            throw new NotSupportedException("The current version doesn't support ONLY. Please use Postgres 11 or higher.");
        }

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
                .Append(GetFilter(expression))
                .Append(";");

            return result.ToString();
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            var quotedSchema = Quoter.QuoteSchemaName(expression.Index.SchemaName);
            var quotedIndex = Quoter.QuoteIndexName(expression.Index.Name);
            var indexName = string.IsNullOrEmpty(quotedSchema) ? quotedIndex : $"{quotedSchema}.{quotedIndex}";
            return string.Format("DROP INDEX {0};", indexName);
        }

        public override string Generate(RenameTableExpression expression)
        {
            return string.Format("ALTER TABLE {0} RENAME TO {1};", Quoter.QuoteTableName(expression.OldName, expression.SchemaName), Quoter.Quote(expression.NewName));
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return string.Format(
                RenameColumn,
                Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                Quoter.QuoteColumnName(expression.OldName),
                Quoter.QuoteColumnName(expression.NewName));
        }

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
                result.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2});", Quoter.QuoteTableName(expression.TableName, expression.SchemaName), columns, data);
            }
            return result.ToString();
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return string.Format(
                "ALTER TABLE {0} ALTER {1} DROP DEFAULT, ALTER {1} {2};",
                Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                Quoter.QuoteColumnName(expression.ColumnName),
                ((PostgresColumn)Column).FormatAlterDefaultValue(expression.ColumnName, expression.DefaultValue));
        }

        public override string Generate(DeleteDataExpression expression)
        {
            var result = new StringBuilder();

            if (expression.IsAllRows)
            {
                result.AppendFormat("DELETE FROM {0};", Quoter.QuoteTableName(expression.TableName, expression.SchemaName));
            }
            else
            {
                foreach (var row in expression.Rows)
                {
                    var where = string.Empty;
                    var i = 0;

                    foreach (var item in row)
                    {
                        if (i != 0)
                        {
                            where += " AND ";
                        }

                        var op = item.Value == null || item.Value == DBNull.Value ? "IS" : "=";
                        where += string.Format("{0} {1} {2}", Quoter.QuoteColumnName(item.Key), op, Quoter.QuoteValue(item.Value));
                        i++;
                    }

                    result.AppendFormat("DELETE FROM {0} WHERE {1};", Quoter.QuoteTableName(expression.TableName, expression.SchemaName), where);
                }
            }

            return result.ToString();
        }

        public override string Generate(UpdateDataExpression expression)
        {
            var updateItems = new List<string>();
            var whereClauses = new List<string>();

            foreach (var item in expression.Set)
            {
                updateItems.Add(string.Format("{0} = {1}", Quoter.QuoteColumnName(item.Key), Quoter.QuoteValue(item.Value)));
            }

            if (expression.IsAllRows)
            {
                whereClauses.Add("1 = 1");
            }
            else
            {
                foreach (var item in expression.Where)
                {
                    var op = item.Value == null || item.Value == DBNull.Value ? "IS" : "=";
                    whereClauses.Add(string.Format("{0} {1} {2}", Quoter.QuoteColumnName(item.Key),
                                                   op, Quoter.QuoteValue(item.Value)));
                }
            }

            return string.Format(
                "UPDATE {0} SET {1} WHERE {2};",
                Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                string.Join(", ", updateItems.ToArray()),
                string.Join(" AND ", whereClauses.ToArray()));
        }

        public override string Generate(AlterSchemaExpression expression)
        {
            return string.Format("ALTER TABLE {0} SET SCHEMA {1};", Quoter.QuoteTableName(expression.TableName, expression.SourceSchemaName), Quoter.QuoteSchemaName(expression.DestinationSchemaName));
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return string.Format("ALTER TABLE {0} ALTER {1} DROP DEFAULT;", Quoter.QuoteTableName(expression.TableName, expression.SchemaName), Quoter.Quote(expression.ColumnName));
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            return string.Format("ALTER TABLE {0} DROP CONSTRAINT {1};", Quoter.QuoteTableName(expression.Constraint.TableName, expression.Constraint.SchemaName), Quoter.Quote(expression.Constraint.ConstraintName));
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            var constraintType = (expression.Constraint.IsPrimaryKeyConstraint) ? "PRIMARY KEY" : "UNIQUE";

            string[] columns = new string[expression.Constraint.Columns.Count];

            for (int i = 0; i < expression.Constraint.Columns.Count; i++)
            {
                columns[i] = Quoter.QuoteColumnName(expression.Constraint.Columns.ElementAt(i));
            }

            return string.Format(
                "ALTER TABLE {0} ADD CONSTRAINT {1} {2} ({3});",
                Quoter.QuoteTableName(expression.Constraint.TableName, expression.Constraint.SchemaName),
                Quoter.QuoteConstraintName(expression.Constraint.ConstraintName),
                constraintType,
                string.Join(", ", columns));
        }

        protected string GetColumnList(IEnumerable<string> columns)
        {
            var result = "";
            foreach (var column in columns)
            {
                result += Quoter.QuoteColumnName(column) + ",";
            }
            return result.TrimEnd(',');
        }

        protected string GetDataList(List<object> data)
        {
            var result = "";
            foreach (var column in data)
            {
                result += Quoter.QuoteValue(column) + ",";
            }
            return result.TrimEnd(',');
        }

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
                    return CompatibilityMode.HandleCompatibilty("Cache size must be greater than 1; if you intended to disable caching, set Cache to null.");
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

            return string.Format("{0};", result.ToString());
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return string.Format("{0};", base.Generate(expression));
        }
    }
}
