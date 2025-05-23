#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using FluentMigrator.SqlServer;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    public class SqlServer2000Generator : GenericGenerator
    {
        public SqlServer2000Generator()
            : this(new SqlServer2000Quoter())
        {
        }

        public SqlServer2000Generator(
            [NotNull] SqlServer2000Quoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public SqlServer2000Generator(
            [NotNull] SqlServer2000Quoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : this(new SqlServer2000Column(new SqlServer2000TypeMap(), quoter), quoter, new EmptyDescriptionGenerator(), generatorOptions)
        {
        }

        protected SqlServer2000Generator(
            [NotNull] IColumn column,
            [NotNull] IQuoter quoter,
            [NotNull] IDescriptionGenerator descriptionGenerator,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, descriptionGenerator, generatorOptions)
        {
        }

        public override string DropTableIfExists { get { return "IF OBJECT_ID('{0}') IS NOT NULL DROP TABLE {0}"; } }

        public override string RenameTable { get { return "sp_rename {0}, {1}"; } }

        public override string RenameColumn { get { return "sp_rename {0}, {1}"; } }

        public override string DropIndex { get { return "DROP INDEX {1}.{0}"; } }

        public override string AddColumn { get { return "ALTER TABLE {0} ADD {1}"; } }

        public virtual string IdentityInsert { get { return "SET IDENTITY_INSERT {0} {1}"; } }

        public override string CreateConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} {2}{3} ({4})"; } }

        //Not need for the nonclusted keyword as it is the default mode
        public override string GetClusterTypeString(CreateIndexExpression column)
        {
            return column.Index.IsClustered ? "CLUSTERED " : string.Empty;
        }

        protected virtual string GetConstraintClusteringString(CreateConstraintExpression constraint)
        {
            object indexType;

            if (!constraint.Constraint.AdditionalFeatures.TryGetValue(
                SqlServerExtensions.ConstraintType, out indexType)) return string.Empty;

            return (indexType.Equals(SqlServerConstraintType.Clustered)) ? " CLUSTERED" : " NONCLUSTERED";
        }

        protected StringBuilder GenerateCreateConstraintPart(CreateConstraintExpression expression)
        {
            var constraintType = (expression.Constraint.IsPrimaryKeyConstraint) ? "PRIMARY KEY" : "UNIQUE";

            var constraintClustering = GetConstraintClusteringString(expression);

            string columns = string.Join(", ", expression.Constraint.Columns.Select(x => Quoter.QuoteColumnName(x)).ToArray());

            return new StringBuilder().AppendFormat(
                CreateConstraint,
                Quoter.QuoteTableName(expression.Constraint.TableName, expression.Constraint.SchemaName),
                Quoter.Quote(expression.Constraint.ConstraintName),
                constraintType,
                constraintClustering,
                columns);
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            var statement = GenerateCreateConstraintPart(expression);

            return AppendSqlStatementEndToken(statement).ToString();
        }

        public override string Generate(RenameTableExpression expression)
        {
            var sourceParam = Quoter.QuoteValue(Quoter.QuoteTableName(expression.OldName, expression.SchemaName));
            var destinationParam = Quoter.QuoteValue(expression.NewName);
            return FormatStatement(RenameTable, sourceParam, destinationParam);
        }

        public override string Generate(RenameColumnExpression expression)
        {
            var tableName = Quoter.QuoteTableName(expression.TableName, expression.SchemaName);
            var columnName = Quoter.QuoteColumnName(expression.OldName);
            var sourceParam = Quoter.QuoteValue($"{tableName}.{columnName}");
            var destinationParam = Quoter.QuoteValue(expression.NewName);
            return FormatStatement(RenameColumn, sourceParam, destinationParam);
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.SqlServer2000;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases =>
            [GeneratorIdConstants.SqlServer2000, GeneratorIdConstants.SqlServer];

        public override string Generate(DeleteColumnExpression expression)
        {
            // before we drop a column, we have to drop any default value constraints in SQL Server
            var builder = new StringBuilder();

            foreach (string column in expression.ColumnNames)
            {
                if (expression.ColumnNames.First() != column) builder.AppendLine("GO");
                BuildDelete(expression, column, builder);
            }

            return builder.ToString();
        }

        protected virtual void BuildDelete(DeleteColumnExpression expression, string columnName, StringBuilder builder)
        {
            builder.AppendLine(
                Generate(
                    new DeleteDefaultConstraintExpression
                    {
                        ColumnName = columnName,
                        SchemaName = expression.SchemaName,
                        TableName = expression.TableName
                    }));

            builder.AppendLine();

            builder.AppendLine(FormatStatement("-- now we can finally drop column" + Environment.NewLine + "ALTER TABLE {0} DROP COLUMN {1}",
                                         Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                                         Quoter.QuoteColumnName(columnName)));
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            // before we alter a default constraint on a column, we have to drop any default value constraints in SQL Server
            var builder = new StringBuilder();

            builder.AppendLine(Generate(new DeleteDefaultConstraintExpression
                                            {
                                                ColumnName = expression.ColumnName,
                                                SchemaName = expression.SchemaName,
                                                TableName = expression.TableName
                                            }));

            builder.AppendLine();

            builder.AppendFormat("-- create alter table command to create new default constraint as string and run it" + Environment.NewLine +"ALTER TABLE {0} WITH NOCHECK ADD CONSTRAINT {3} DEFAULT({2}) FOR {1}",
                Quoter.QuoteTableName(expression.TableName, expression.SchemaName),
                Quoter.QuoteColumnName(expression.ColumnName),
                SqlServer2000Column.FormatDefaultValue(expression.DefaultValue, Quoter),
                Quoter.QuoteConstraintName(SqlServer2000Column.GetDefaultConstraintName(expression.TableName, expression.ColumnName)));

            AppendSqlStatementEndToken(builder);

            return builder.ToString();
        }

        public override string Generate(InsertDataExpression expression)
        {
            if (IsUsingIdentityInsert(expression))
            {
                return
                    FormatStatement(IdentityInsert, Quoter.QuoteTableName(expression.TableName, expression.SchemaName), "ON") +
                    base.Generate(expression) +
                    FormatStatement(IdentityInsert, Quoter.QuoteTableName(expression.TableName, expression.SchemaName), "OFF");
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

        public override string Generate(CreateSequenceExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Sequences are not supported in SqlServer2000");
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Sequences are not supported in SqlServer2000");
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            string sql =
                "DECLARE @default sysname, @sql nvarchar(4000);" + Environment.NewLine + Environment.NewLine +
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
    }
}
