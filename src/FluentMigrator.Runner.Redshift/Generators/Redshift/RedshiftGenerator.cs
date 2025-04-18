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
using FluentMigrator.Runner.Generators.Generic;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.Redshift
{
    public class RedshiftGenerator : GenericGenerator
    {
        public override string UpdateData { get { return "UPDATE {0} SET {1} WHERE {2};"; } }
        public override string DeleteData { get { return "DELETE FROM {0} WHERE {1};"; } }

        public RedshiftGenerator()
            : this(new RedshiftQuoter())
        {
        }

        public RedshiftGenerator(
            [NotNull] RedshiftQuoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public RedshiftGenerator(
            [NotNull] RedshiftQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new RedshiftColumn(), quoter, new RedshiftDescriptionGenerator(), generatorOptions)
        {
        }

        protected override StringBuilder AppendSqlStatementEndToken(StringBuilder stringBuilder)
        {
            return stringBuilder.Append(" ");
        }

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

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.Redshift;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases => new List<string> { GeneratorIdConstants.Redshift };

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
            if (expression.Columns.Any(x => x.Expression != null))
            {
                CompatibilityMode.HandleCompatibility("Computed columns are not supported");
            }
            var createStatement = new StringBuilder();
            var tableName = Quoter.Quote(expression.TableName);
            createStatement.AppendFormat("CREATE TABLE {0} ({1})", Quoter.QuoteTableName(expression.TableName, expression.SchemaName), Column.Generate(expression.Columns, tableName));
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
            if (expression.Column.Expression != null)
            {
                CompatibilityMode.HandleCompatibility("Computed columns are not supported");
            }
            var alterStatement = new StringBuilder();
            alterStatement.AppendFormat("ALTER TABLE {0} {1};", Quoter.QuoteTableName(expression.TableName, expression.SchemaName), ((RedshiftColumn)Column).GenerateAlterClauses(expression.Column));
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
            if (expression.Column.Expression != null)
            {
                CompatibilityMode.HandleCompatibility("Computed columns are not supported");
            }
            var createStatement = new StringBuilder();
            createStatement.AppendFormat("ALTER TABLE {0} ADD {1};", Quoter.QuoteTableName(expression.TableName, expression.SchemaName), Column.Generate(expression.Column));
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);
            if (!string.IsNullOrEmpty(descriptionStatement))
            {
                createStatement.Append(";");
                createStatement.Append(descriptionStatement);
            }
            return createStatement.ToString();
        }

        public override string Generate(DeleteTableExpression expression)
        {
            return $"DROP TABLE{(expression.IfExists ? " IF EXISTS" : "")} {Quoter.QuoteTableName(expression.TableName, expression.SchemaName)};";
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string columnName in expression.ColumnNames) {
                if (expression.ColumnNames.First() != columnName) builder.AppendLine("");
                builder.AppendFormat("ALTER TABLE {0} DROP COLUMN {1};",
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
            return string.Format(
                "ALTER TABLE {0} DROP CONSTRAINT {1};",
                Quoter.QuoteTableName(expression.ForeignKey.ForeignTable, expression.ForeignKey.ForeignTableSchema),
                Quoter.Quote(expression.ForeignKey.Name));
        }

        public override string Generate(CreateIndexExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Indices not supported");
        }

        public override string Generate(DeleteIndexExpression expression)
        {

            return CompatibilityMode.HandleCompatibility("Indices not supported");
        }

        public override string Generate(RenameTableExpression expression)
        {
            return string.Format(
                "ALTER TABLE {0} RENAME TO {1};",
                Quoter.QuoteTableName(expression.OldName, expression.SchemaName),
                Quoter.Quote(expression.NewName));
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return string.Format(
                "ALTER TABLE {0} RENAME COLUMN {1} TO {2};",
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
                ((RedshiftColumn)Column).FormatAlterDefaultValue(expression.ColumnName, expression.DefaultValue));
        }

        public override string Generate(AlterSchemaExpression expression)
        {
            return string.Format(
                "ALTER TABLE {0} SET SCHEMA {1};",
                Quoter.QuoteTableName(expression.TableName, expression.SourceSchemaName),
                Quoter.QuoteSchemaName(expression.DestinationSchemaName));
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return string.Format("ALTER TABLE {0} ALTER {1} DROP DEFAULT;", Quoter.QuoteTableName(expression.TableName, expression.SchemaName), Quoter.Quote(expression.ColumnName));
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            return string.Format(
                "ALTER TABLE {0} DROP CONSTRAINT {1};",
                Quoter.QuoteTableName(expression.Constraint.TableName, expression.Constraint.SchemaName),
                Quoter.Quote(expression.Constraint.ConstraintName));
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
            return CompatibilityMode.HandleCompatibility("Sequences not supported");
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Sequences not supported");
        }
    }
}
