#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Model;
using FluentMigrator.SqlAnywhere;

namespace FluentMigrator.Runner.Generators.SqlAnywhere
{
    public class SqlAnywhere16Generator : GenericGenerator
    {
        public SqlAnywhere16Generator()
            : base(new SqlAnywhereColumn(new SqlAnywhere16TypeMap()), new SqlAnywhereQuoter(), new EmptyDescriptionGenerator())
        {
        }

        protected SqlAnywhere16Generator(IColumn column, IDescriptionGenerator descriptionGenerator)
            : base(column, new SqlAnywhereQuoter(), descriptionGenerator)
        {
        }

        public override string CreateSchema => "CREATE SCHEMA AUTHORIZATION {0}";

        public override string DropSchema => "DROP USER {0}";

        public override string CreateTable => "{0} ({1})";

        public override string DropTable => "{0}";

        public override string RenameTable => "ALTER TABLE {0} RENAME {1}";

        public override string CreateIndex => "CREATE {0}{1}INDEX {2} ON {3}.{4} ({5}){6}";

        public override string DropIndex => "DROP INDEX {0}.{1}.{2}";

        public override string AddColumn => "{0} ADD {1}";

        public override string DropColumn => "ALTER TABLE {0} DROP {1}";

        public override string AlterColumn => "{0} ALTER {1}";

        public override string RenameColumn => "{0} RENAME {1} TO {2}";

        public override string InsertData => "INSERT INTO {0}.{1} ({2}) VALUES ({3})";

        public override string UpdateData => "{0} SET {1} WHERE {2}";

        public override string DeleteData => "DELETE FROM {0}.{1} WHERE {2}";

        public virtual string IdentityInsert => "SET IDENTITY_INSERT {0} {1}";

        public override string CreateForeignKeyConstraint => "ALTER TABLE {0}.{1} ADD CONSTRAINT {2} FOREIGN KEY ({3}) REFERENCES {4}.{5} ({6}){7}{8}";

        public override string CreateConstraint => "ALTER TABLE {0} ADD CONSTRAINT {1} {2}{3} ({4})";

        public override string DeleteConstraint => "{0} DROP CONSTRAINT {1}";

        //Not need for the nonclusted keyword as it is the default mode
        public override string GetClusterTypeString(CreateIndexExpression column)
        {
            return column.Index.IsClustered ? "CLUSTERED " : string.Empty;
        }

        protected string GetConstraintClusteringString(CreateConstraintExpression constraint)
        {
            if (!constraint.Constraint.AdditionalFeatures.TryGetValue(
                SqlAnywhereExtensions.ConstraintType, out var indexType)) return string.Empty;

            return (indexType.Equals(SqlAnywhereConstraintType.Clustered)) ? " CLUSTERED" : " NONCLUSTERED";
        }

        public override string Generate(CreateTableExpression expression)
        {
            var descriptionStatements = DescriptionGenerator.GenerateDescriptionStatements(expression);
            var createTableStatement = $"CREATE TABLE {Quoter.QuoteSchemaName(expression.SchemaName)}.{base.Generate(expression)}";
            var descriptionStatementsArray = descriptionStatements as string[] ?? descriptionStatements.ToArray();

            if (!descriptionStatementsArray.Any())
                return createTableStatement;

            return ComposeStatements(createTableStatement, descriptionStatementsArray);
        }

        public override string Generate(DeleteTableExpression expression)
        {
            return $"DROP TABLE {Quoter.QuoteSchemaName(expression.SchemaName)}.{base.Generate(expression)}";
        }

        public override string Generate(CreateColumnExpression expression)
        {
            var alterTableStatement = $"ALTER TABLE {Quoter.QuoteSchemaName(expression.SchemaName)}.{base.Generate(expression)}";
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return alterTableStatement;

            return ComposeStatements(alterTableStatement, new[] { descriptionStatement });
        }

        public override string Generate(AlterColumnExpression expression)
        {
            var alterTableStatement = $"ALTER TABLE {Quoter.QuoteSchemaName(expression.SchemaName)}.{base.Generate(expression)}";
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return alterTableStatement;

            return ComposeStatements(alterTableStatement, new[] { descriptionStatement });
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            var constraintType = (expression.Constraint.IsPrimaryKeyConstraint) ? "PRIMARY KEY" : "UNIQUE";

            var constraintClustering = GetConstraintClusteringString(expression);

            string columns = String.Join(", ", expression.Constraint.Columns.Select(x => Quoter.QuoteColumnName(x)).ToArray());

            string schemaTableName = $"{Quoter.QuoteSchemaName(expression.Constraint.SchemaName)}.{Quoter.QuoteTableName(expression.Constraint.TableName)}";

            return string.Format(CreateConstraint, schemaTableName,
                Quoter.Quote(expression.Constraint.ConstraintName),
                constraintType,
                constraintClustering,
                columns);
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            return $"ALTER TABLE {Quoter.QuoteSchemaName(expression.Constraint.SchemaName)}.{base.Generate(expression)}";
        }

        public override string Generate(RenameTableExpression expression)
        {
            string oldSchemaTableName = $"{Quoter.QuoteSchemaName(expression.SchemaName)}.{Quoter.QuoteTableName(expression.OldName)}";
            return String.Format(RenameTable, oldSchemaTableName, Quoter.QuoteTableName(expression.NewName));
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return $"ALTER TABLE {Quoter.QuoteSchemaName(expression.SchemaName)}.{base.Generate(expression)}";
            //return String.Format(RenameColumn, alterTableStatement, Quoter.QuoteColumnName(Quoter.QuoteCommand(expression.OldName)), Quoter.QuoteCommand(expression.NewName));
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            // Create an ALTER TABLE statement for each column to be deleted
            var builder = new StringBuilder();

            foreach (string column in expression.ColumnNames)
                BuildDelete(expression, column, builder);

            return builder.ToString();
        }

        protected virtual void BuildDelete(DeleteColumnExpression expression, string columnName, StringBuilder builder)
        {
            string schemaAndTable = $"{Quoter.QuoteSchemaName(expression.SchemaName)}.{Quoter.QuoteTableName(expression.TableName)}";
            builder.AppendLine(string.Format(DropColumn, schemaAndTable, Quoter.QuoteColumnName(columnName)));
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            // before we alter a default constraint on a column, we have to drop any default value constraints in SQL Anywhere
            var builder = new StringBuilder();
            var deleteDefault = Generate(new DeleteDefaultConstraintExpression
            {
                ColumnName = expression.ColumnName,
                SchemaName = expression.SchemaName,
                TableName = expression.TableName
            }) + ";";
            builder.AppendLine(deleteDefault);

            builder.Append(String.Format("-- create alter table command to create new default constraint as string and run it" + Environment.NewLine + "ALTER TABLE {0}.{1} ALTER {2} DEFAULT {3};",
                Quoter.QuoteSchemaName(expression.SchemaName),
                Quoter.QuoteTableName(expression.TableName),
                Quoter.QuoteColumnName(expression.ColumnName),
                Quoter.QuoteValue(expression.DefaultValue)));

            return builder.ToString();
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            return $"ALTER TABLE {Quoter.QuoteSchemaName(expression.ForeignKey.ForeignTableSchema)}.{base.Generate(expression)}";
        }

        public override string Generate(InsertDataExpression expression)
        {
            List<string> columnNames = new List<string>();
            List<string> columnValues = new List<string>();
            List<string> insertStrings = new List<string>();

            foreach (InsertionDataDefinition row in expression.Rows)
            {
                columnNames.Clear();
                columnValues.Clear();
                foreach (KeyValuePair<string, object> item in row)
                {
                    columnNames.Add(Quoter.QuoteColumnName(item.Key));
                    columnValues.Add(Quoter.QuoteValue(item.Value));
                }

                string columns = String.Join(", ", columnNames.ToArray());
                string values = String.Join(", ", columnValues.ToArray());
                insertStrings.Add(String.Format(InsertData
                    , Quoter.QuoteSchemaName(expression.SchemaName)
                    , Quoter.QuoteTableName(expression.TableName)
                    , columns
                    , values));
            }

            return String.Join("; ", insertStrings.ToArray());
        }

        public override string Generate(UpdateDataExpression expression)
        {
            // TODO: Uncomment line below and delete custom implementation once pull request #672 (https://github.com/schambers/fluentmigrator/pull/672)
            //       has been accepted into the master branch.
            //return string.Format("UPDATE {0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), base.Generate(expression));

            List<string> updateItems = new List<string>();
            List<string> whereClauses = new List<string>();

            foreach (var item in expression.Set)
            {
                updateItems.Add($"{Quoter.QuoteColumnName(item.Key)} = {Quoter.QuoteValue(item.Value)}");
            }

            if (expression.IsAllRows)
            {
                whereClauses.Add("1 = 1");
            }
            else
            {
                foreach (var item in expression.Where)
                {
                    whereClauses.Add($"{Quoter.QuoteColumnName(item.Key)} {((item.Value == null || item.Value == DBNull.Value) ? "IS" : "=")} {Quoter.QuoteValue(item.Value)}");
                }
            }
            var genericGeneratorSql = String.Format(UpdateData, Quoter.QuoteTableName(expression.TableName), String.Join(", ", updateItems.ToArray()), String.Join(" AND ", whereClauses.ToArray()));
            return $"UPDATE {Quoter.QuoteSchemaName(expression.SchemaName)}.{genericGeneratorSql}";
        }

        public override string Generate(DeleteDataExpression expression)
        {
            var deleteItems = new List<string>();


            if (expression.IsAllRows)
            {
                deleteItems.Add(string.Format(DeleteData, Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName), "1 = 1"));
            }
            else
            {
                foreach (var row in expression.Rows)
                {
                    var whereClauses = new List<string>();
                    foreach (KeyValuePair<string, object> item in row)
                    {
                        whereClauses.Add($"{Quoter.QuoteColumnName(item.Key)} {((item.Value == null || item.Value == DBNull.Value) ? "IS" : "=")} {Quoter.QuoteValue(item.Value)}");
                    }

                    deleteItems.Add(string.Format(DeleteData, Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName), String.Join(" AND ", whereClauses.ToArray())));
                }
            }

            return String.Join("; ", deleteItems.ToArray());
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
                Quoter.QuoteSchemaName(expression.ForeignKey.ForeignTableSchema),
                Quoter.QuoteTableName(expression.ForeignKey.ForeignTable),
                Quoter.QuoteColumnName(expression.ForeignKey.Name),
                String.Join(", ", foreignColumns.ToArray()),
                Quoter.QuoteSchemaName(expression.ForeignKey.PrimaryTableSchema),
                Quoter.QuoteTableName(expression.ForeignKey.PrimaryTable),
                String.Join(", ", primaryColumns.ToArray()),
                Column.FormatCascade("DELETE", expression.ForeignKey.OnDelete),
                Column.FormatCascade("UPDATE", expression.ForeignKey.OnUpdate)
                );
        }

        public override string Generate(CreateIndexExpression expression)
        {
            string[] indexColumns = new string[expression.Index.Columns.Count];

            for (int i = 0; i < expression.Index.Columns.Count; i++)
            {
                var columnDef = expression.Index.Columns.ElementAt(i);
                if (columnDef.Direction == Direction.Ascending)
                {
                    indexColumns[i] = Quoter.QuoteColumnName(columnDef.Name) + " ASC";
                }
                else
                {
                    indexColumns[i] = Quoter.QuoteColumnName(columnDef.Name) + " DESC";
                }
            }

            return String.Format(CreateIndex
                , GetUniqueString(expression)
                , GetClusterTypeString(expression)
                , Quoter.QuoteIndexName(expression.Index.Name)
                , Quoter.QuoteSchemaName(expression.Index.SchemaName)
                , Quoter.QuoteTableName(expression.Index.TableName)
                , string.Join(", ", indexColumns)
                , GetWithNullsDistinctString(expression.Index));
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            return String.Format(DropIndex, Quoter.QuoteSchemaName(expression.Index.SchemaName), Quoter.QuoteTableName(expression.Index.TableName), Quoter.QuoteIndexName(expression.Index.Name));
        }

        public override string Generate(AlterSchemaExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("AlterSchema is not supported in SqlAnywhere");
        }

        public override string Generate(CreateSchemaExpression expression)
        {
            return String.Format(CreateSchema, Quoter.QuoteSchemaName(expression.SchemaName));
        }

        public override string Generate(DeleteSchemaExpression expression)
        {
            return String.Format(DropSchema, Quoter.QuoteSchemaName(expression.SchemaName));
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Sequences are not supported in SqlAnywhere");
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Sequences are not supported in SqlAnywhere");
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            string sql = "ALTER TABLE {0} ALTER {1} DROP DEFAULT";
            string schemaAndTable = $"{Quoter.QuoteSchemaName(expression.SchemaName)}.{Quoter.QuoteTableName(expression.TableName)}";
            return String.Format(sql, schemaAndTable, Quoter.QuoteColumnName(expression.ColumnName));
        }

        public override bool IsAdditionalFeatureSupported(string feature)
        {
            return _supportedAdditionalFeatures.Any(x => x == feature);
        }

        protected virtual string GetWithNullsDistinctString(IndexDefinition index)
        {
            var indexNullsDistinct = index.GetAdditionalFeature(SqlAnywhereExtensions.WithNullsDistinct, (bool?)null);

            if (indexNullsDistinct == null)
                return string.Empty;

            if (indexNullsDistinct.Value)
            {
                return " WITH NULLS DISTINCT";
            }

            return " WITH NULLS NOT DISTINCT";
        }

        private readonly IEnumerable<string> _supportedAdditionalFeatures = new List<string>
        {
            SqlAnywhereExtensions.ConstraintType,
            SqlAnywhereExtensions.SchemaPassword,
            SqlAnywhereExtensions.WithNullsDistinct,
        };

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
