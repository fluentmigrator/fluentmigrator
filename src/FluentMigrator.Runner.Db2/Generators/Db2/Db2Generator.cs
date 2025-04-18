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

using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.DB2
{
    public class Db2Generator : GenericGenerator
    {
        public Db2Generator()
            : this(new Db2Quoter())
        {
        }

        public Db2Generator(
            Db2Quoter quoter)
            : this(
                quoter,
                new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public Db2Generator(
            Db2Quoter quoter,
            IOptions<GeneratorOptions> generatorOptions)
            : base(new Db2Column(quoter), quoter, new EmptyDescriptionGenerator(), generatorOptions)
        {
        }

        protected override StringBuilder AppendSqlStatementEndToken(StringBuilder stringBuilder)
        {
            return stringBuilder.Append(" ");
        }

        public override string Generate(Expressions.AlterDefaultConstraintExpression expression)
        {
            return string.Format(
                "ALTER TABLE {0} ALTER COLUMN {1} SET DEFAULT {2}",
                Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                Quoter.QuoteColumnName(expression.ColumnName),
                ((Db2Column)Column).FormatAlterDefaultValue(expression.ColumnName, expression.DefaultValue));
        }

        public override string Generate(Expressions.DeleteDefaultConstraintExpression expression)
        {
            return string.Format(
                "ALTER TABLE {0} ALTER COLUMN {1} DROP DEFAULT",
                Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                Quoter.QuoteColumnName(expression.ColumnName));
        }

        public override string Generate(Expressions.RenameTableExpression expression)
        {
            return string.Format(
                "RENAME TABLE {0} TO {1}",
                Quoter.QuoteTableName(expression.OldName, expression.SchemaName),
                Quoter.QuoteTableName(expression.NewName));
        }

        public override string Generate(Expressions.DeleteColumnExpression expression)
        {
            var builder = new StringBuilder();
            if (expression.ColumnNames.Count == 0 || string.IsNullOrEmpty(expression.ColumnNames.First()))
            {
                return string.Empty;
            }

            builder.AppendFormat("ALTER TABLE {0}", Quoter.QuoteTableName(expression.TableName, expression.SchemaName));
            foreach (var column in expression.ColumnNames)
            {
                builder.AppendFormat(" DROP COLUMN {0}", Quoter.QuoteColumnName(column));
            }

            return builder.ToString();
        }

        public override string Generate(Expressions.CreateColumnExpression expression)
        {
            expression.Column.AdditionalFeatures.Add(new KeyValuePair<string, object>("IsCreateColumn", true));

            return string.Format(
                "ALTER TABLE {0} ADD COLUMN {1}",
                Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                Column.Generate(expression.Column));
        }

        public override string Generate(Expressions.CreateForeignKeyExpression expression)
        {
            if (expression.ForeignKey.PrimaryColumns.Count != expression.ForeignKey.ForeignColumns.Count)
            {
                throw new ArgumentException("Number of primary columns and secondary columns must be equal");
            }

            var keyName = string.IsNullOrEmpty(expression.ForeignKey.Name)
                ? Column.GenerateForeignKeyName(expression.ForeignKey)
                : expression.ForeignKey.Name;
            var keyWithSchema = Quoter.QuoteConstraintName(keyName, expression.ForeignKey.ForeignTableSchema);

            var primaryColumns = expression.ForeignKey.PrimaryColumns.Aggregate(new StringBuilder(), (acc, col) =>
            {
                var separator = acc.Length == 0 ? string.Empty : ", ";
                return acc.AppendFormat("{0}{1}", separator, Quoter.QuoteColumnName(col));
            });

            var foreignColumns = expression.ForeignKey.ForeignColumns.Aggregate(new StringBuilder(), (acc, col) =>
            {
                var separator = acc.Length == 0 ? string.Empty : ", ";
                return acc.AppendFormat("{0}{1}", separator, Quoter.QuoteColumnName(col));
            });

            return string.Format(
                "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}",
                Quoter.QuoteTableName(expression.ForeignKey.ForeignTable, expression.ForeignKey.ForeignTableSchema),
                keyWithSchema,
                foreignColumns,
                Quoter.QuoteTableName(expression.ForeignKey.PrimaryTable, expression.ForeignKey.PrimaryTableSchema),
                primaryColumns,
                Column.FormatCascade("DELETE", expression.ForeignKey.OnDelete));
        }

        public override string Generate(Expressions.CreateConstraintExpression expression)
        {
            var constraintName = Quoter.QuoteConstraintName(expression.Constraint.ConstraintName, expression.Constraint.SchemaName);

            var constraintType = expression.Constraint.IsPrimaryKeyConstraint ? "PRIMARY KEY" : "UNIQUE";
            var quotedNames = expression.Constraint.Columns.Select(q => Quoter.QuoteColumnName(q));
            var columnList = string.Join(", ", quotedNames.ToArray());

            return string.Format(
                "ALTER TABLE {0} ADD CONSTRAINT {1} {2} ({3})",
                Quoter.QuoteTableName(expression.Constraint.TableName, expression.Constraint.SchemaName),
                constraintName,
                constraintType,
                columnList);
        }

        public override string Generate(Expressions.CreateIndexExpression expression)
        {
            var indexWithSchema = Quoter.QuoteIndexName(expression.Index.Name, expression.Index.SchemaName);

            var columnList = expression.Index.Columns.Aggregate(new StringBuilder(), (item, itemToo) =>
            {
                var accumulator = item.Length == 0 ? string.Empty : ", ";
                var direction = itemToo.Direction == Direction.Ascending ? string.Empty : " DESC";

                return item.AppendFormat("{0}{1}{2}", accumulator, Quoter.QuoteColumnName(itemToo.Name), direction);
            });

            return string.Format(
                "CREATE {0}INDEX {1} ON {2} ({3})",
                expression.Index.IsUnique ? "UNIQUE " : string.Empty,
                indexWithSchema,
                Quoter.QuoteTableName(expression.Index.TableName, expression.Index.SchemaName),
                columnList);
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.DB2;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases => new List<string> { GeneratorIdConstants.DB2 };

        public override string Generate(Expressions.CreateSchemaExpression expression)
        {
            return string.Format("CREATE SCHEMA {0}", Quoter.QuoteSchemaName(expression.SchemaName));
        }

        public override string Generate(Expressions.DeleteTableExpression expression)
        {
            if (expression.IfExists)
            {
                if (expression.SchemaName == null)
                {
                    return CompatibilityMode.HandleCompatibility("Db2 needs schema name to safely handle if exists");
                }
                return
                    $"IF( EXISTS(SELECT 1 FROM SYSCAT.TABLES WHERE TABSCHEMA = '{Quoter.QuoteSchemaName(expression.SchemaName)}' AND TABNAME = '{Quoter.QuoteTableName(expression.TableName)}')) THEN DROP TABLE {Quoter.QuoteTableName(expression.TableName, expression.SchemaName)} END IF";
            }

            return $"DROP TABLE {Quoter.QuoteTableName(expression.TableName, expression.SchemaName)}";
        }

        public override string Generate(Expressions.DeleteIndexExpression expression)
        {
            var indexWithSchema = Quoter.QuoteIndexName(expression.Index.Name, expression.Index.SchemaName);
            return string.Format("DROP INDEX {0}", indexWithSchema);
        }

        public override string Generate(Expressions.DeleteSchemaExpression expression)
        {
            return string.Format("DROP SCHEMA {0} RESTRICT", Quoter.QuoteSchemaName(expression.SchemaName));
        }

        public override string Generate(Expressions.DeleteConstraintExpression expression)
        {
            var constraintName = Quoter.QuoteConstraintName(expression.Constraint.ConstraintName, expression.Constraint.SchemaName);

            return string.Format(
                "ALTER TABLE {0} DROP CONSTRAINT {1}",
                Quoter.QuoteTableName(expression.Constraint.TableName, expression.Constraint.SchemaName),
                constraintName);
        }

        public override string Generate(Expressions.DeleteForeignKeyExpression expression)
        {
            var constraintName = Quoter.QuoteConstraintName(expression.ForeignKey.Name, expression.ForeignKey.ForeignTableSchema);

            return string.Format(
                "ALTER TABLE {0} DROP FOREIGN KEY {1}",
                Quoter.QuoteTableName(expression.ForeignKey.ForeignTable, expression.ForeignKey.ForeignTableSchema),
                constraintName);
        }

        public override string Generate(Expressions.RenameColumnExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("This feature not directly supported by most versions of DB2.");
        }

        public override string Generate(Expressions.InsertDataExpression expression)
        {
            var sb = new StringBuilder();
            foreach (var row in expression.Rows)
            {
                var columnList = row.Aggregate(new StringBuilder(), (acc, rowVal) =>
                {
                    var accumulator = acc.Length == 0 ? string.Empty : ", ";
                    return acc.AppendFormat("{0}{1}", accumulator, Quoter.QuoteColumnName(rowVal.Key));
                });

                var dataList = row.Aggregate(new StringBuilder(), (acc, rowVal) =>
                {
                    var accumulator = acc.Length == 0 ? string.Empty : ", ";
                    return acc.AppendFormat("{0}{1}", accumulator, Quoter.QuoteValue(rowVal.Value));
                });

                var separator = sb.Length == 0 ? string.Empty : " ";

                sb.AppendFormat(
                    "{0}INSERT INTO {1} ({2}) VALUES ({3})",
                    separator,
                    Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                    columnList,
                    dataList);
            }

            return sb.ToString();
        }

        public override string Generate(Expressions.CreateTableExpression expression)
        {
            return string.Format("CREATE TABLE {0} ({1})", Quoter.QuoteTableName(expression.TableName, expression.SchemaName), Column.Generate(expression.Columns, expression.TableName));
        }

        public override string Generate(Expressions.AlterColumnExpression expression)
        {
            try
            {
                // throws an exception of an attempt is made to alter an identity column, as it is not supported by most version of DB2.
                return string.Format("ALTER TABLE {0} {1}", Quoter.QuoteTableName(expression.TableName, expression.SchemaName), ((Db2Column)Column).GenerateAlterClause(expression.Column));
            }
            catch (NotSupportedException e)
            {
                return CompatibilityMode.HandleCompatibility(e.Message);
            }
        }
        public override string Generate(Expressions.AlterSchemaExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("This feature not directly supported by most versions of DB2.");
        }
    }
}
