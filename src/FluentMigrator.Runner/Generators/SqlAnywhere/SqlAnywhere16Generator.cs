#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Generators.Generic;

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

        public override string CreateSchema { get { return "CREATE SCHEMA AUTHORIZATION {0}"; } }
        public override string DropSchema { get { return "DROP USER {0}"; } }

        public override string CreateTable { get { return "{0} ({1})"; } }
        public override string DropTable { get { return "{0}"; } }
        public override string RenameTable { get { return "ALTER TABLE {0} RENAME {1}"; } }

        public override string DropIndex { get { return "DROP INDEX {1}.{0}"; } }

        public override string AddColumn { get { return "{0} ADD {1}"; } }
        public override string DropColumn { get { return "ALTER TABLE {0} DROP {1}"; } }
        public override string AlterColumn { get { return "{0} ALTER {1}"; } }
        public override string RenameColumn { get { return "{0} RENAME {1} TO {2}"; } }

        public virtual string IdentityInsert { get { return "SET IDENTITY_INSERT {0} {1}"; } }

        public override string CreateForeignKeyConstraint { get { return "ALTER TABLE {0}.{1} ADD CONSTRAINT {2} FOREIGN KEY ({3}) REFERENCES {4}.{5} ({6}){7}{8}"; } }
        public override string CreateConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} {2}{3} ({4})"; } }
        public override string DeleteConstraint { get { return "{0} DROP CONSTRAINT {1}"; } }

        //Not need for the nonclusted keyword as it is the default mode
        public override string GetClusterTypeString(CreateIndexExpression column)
        {
            return column.Index.IsClustered ? "CLUSTERED " : string.Empty;
        }

        protected string GetConstraintClusteringString(CreateConstraintExpression constraint)
        {
            object indexType;

            if (!constraint.Constraint.AdditionalFeatures.TryGetValue(
                SqlServerExtensions.ConstraintType, out indexType)) return string.Empty;

            return (indexType.Equals(SqlServerConstraintType.Clustered)) ? " CLUSTERED" : " NONCLUSTERED";
        }

        public override string Generate(CreateTableExpression expression)
        {
            var descriptionStatements = DescriptionGenerator.GenerateDescriptionStatements(expression);
            var createTableStatement = string.Format("CREATE TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), base.Generate(expression));
            var descriptionStatementsArray = descriptionStatements as string[] ?? descriptionStatements.ToArray();

            if (!descriptionStatementsArray.Any())
                return createTableStatement;

            return ComposeStatements(createTableStatement, descriptionStatementsArray);
        }

        public override string Generate(DeleteTableExpression expression)
        {
            return string.Format("DROP TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), base.Generate(expression));
        }

        public override string Generate(CreateColumnExpression expression)
        {
            var alterTableStatement = string.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), base.Generate(expression));
            var descriptionStatement = DescriptionGenerator.GenerateDescriptionStatement(expression);

            if (string.IsNullOrEmpty(descriptionStatement))
                return alterTableStatement;

            return ComposeStatements(alterTableStatement, new[] { descriptionStatement });
        }

        public override string Generate(AlterColumnExpression expression)
        {
            var alterTableStatement = string.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), base.Generate(expression));
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

            string schemaTableName = string.Format("{0}.{1}", Quoter.QuoteSchemaName(expression.Constraint.SchemaName), Quoter.QuoteTableName(expression.Constraint.TableName));

            return string.Format(CreateConstraint, schemaTableName,
                Quoter.Quote(expression.Constraint.ConstraintName),
                constraintType,
                constraintClustering,
                columns);
        }
        
        public override string Generate(DeleteConstraintExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.Constraint.SchemaName), base.Generate(expression));
        }

        public override string Generate(RenameTableExpression expression)
        {
            string oldSchemaTableName = string.Format("{0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.OldName));
            return String.Format(RenameTable, oldSchemaTableName, Quoter.QuoteTableName(expression.NewName));
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return string.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), base.Generate(expression));
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
            string schemaAndTable = string.Format("{0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName));
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
            return string.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(expression.ForeignKey.ForeignTableSchema), base.Generate(expression));
        }

        public override string Generate(InsertDataExpression expression)
        {
            if (IsUsingIdentityInsert(expression))
            {
                return string.Format("{0}; {1}; {2}",
                            string.Format(IdentityInsert, Quoter.QuoteTableName(expression.TableName), "ON"),
                            base.Generate(expression),
                            string.Format(IdentityInsert, Quoter.QuoteTableName(expression.TableName), "OFF"));
            }
            return base.Generate(expression);
        }

        protected static bool IsUsingIdentityInsert(InsertDataExpression expression)
        {
            if (expression.AdditionalFeatures.ContainsKey(SqlServerExtensions.IdentityInsert))
            {
                return (bool)expression.AdditionalFeatures[SqlServerExtensions.IdentityInsert];
            }

            return false;
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
                FormatCascade("DELETE", expression.ForeignKey.OnDelete),
                FormatCascade("UPDATE", expression.ForeignKey.OnUpdate)
                );
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
            string schemaAndTable = string.Format("{0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteTableName(expression.TableName));
            return String.Format(sql, schemaAndTable, Quoter.QuoteColumnName(expression.ColumnName));
        }

        public override bool IsAdditionalFeatureSupported(string feature)
        {
            return _supportedAdditionalFeatures.Any(x => x == feature);
        }

        private readonly IEnumerable<string> _supportedAdditionalFeatures = new List<string> 
        { 
            SqlServerExtensions.IdentityInsert,
            SqlServerExtensions.IdentitySeed, 
            SqlServerExtensions.IdentityIncrement, 
            SqlServerExtensions.ConstraintType
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