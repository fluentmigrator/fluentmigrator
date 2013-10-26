using FluentMigrator.Runner.Helpers;

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
using System.Data;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Runner.Generators.MySql;

namespace FluentMigrator.Runner.Processors.MySql
{
    public class MySqlProcessor : GenericProcessorBase
    {
        readonly MySqlQuoter quoter = new MySqlQuoter();

        public override string DatabaseType
        {
            get { return "MySql"; }
        }

        public MySqlProcessor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options, IDbFactory factory)
            : base(connection, factory, generator, announcer, options)
        {
        }

        public override bool SchemaExists(string schemaName)
        {
            return true;
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            return Exists(@"select table_name from information_schema.tables 
                            where table_schema = SCHEMA() and table_name='{0}'", FormatHelper.FormatSqlEscape(tableName));
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            const string sql = @"select column_name from information_schema.columns
                            where table_schema = SCHEMA() and table_name='{0}'
                            and column_name='{1}'";
            return Exists(sql, FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(columnName));
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            const string sql = @"select constraint_name from information_schema.table_constraints
                            where table_schema = SCHEMA() and table_name='{0}'
                            and constraint_name='{1}'";
            return Exists(sql, FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(constraintName));
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            const string sql = @"select index_name from information_schema.statistics
                            where table_schema = SCHEMA() and table_name='{0}'
                            and index_name='{1}'";
            return Exists(sql, FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(indexName));
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return false;
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            string defaultValueAsString = string.Format("%{0}%", FormatHelper.FormatSqlEscape(defaultValue.ToString()));
            return Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = SCHEMA() AND TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}' AND COLUMN_DEFAULT LIKE '{2}'",
               FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(columnName), defaultValueAsString);
        }

        public override void Execute(string template, params object[] args)
        {
            if (Options.PreviewOnly)
            {
                return;
            }

            EnsureConnectionIsOpen();

            using (var command = Factory.CreateCommand(String.Format(template, args), Connection))
            {
                command.CommandTimeout = Options.Timeout;
                command.ExecuteNonQuery();
            }
        }

        public override bool Exists(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = Factory.CreateCommand(String.Format(template, args), Connection))
            {
                command.CommandTimeout = Options.Timeout;
                using (var reader = command.ExecuteReader())
                {
                    try
                    {
                        return reader.Read();
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read("select * from {0}", quoter.QuoteTableName(tableName));
        }

        public override DataSet Read(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            var ds = new DataSet();
            using (var command = Factory.CreateCommand(String.Format(template, args), Connection))
            {
                command.CommandTimeout = Options.Timeout;

                var adapter = Factory.CreateDataAdapter(command);
                adapter.Fill(ds);
                return ds;
            }
        }

        protected override void Process(string sql)
        {
            Announcer.Sql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            EnsureConnectionIsOpen();

            using (var command = Factory.CreateCommand(sql, Connection))
            {
                command.CommandTimeout = Options.Timeout;
                command.ExecuteNonQuery();
            }
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            Announcer.Say("Performing DB Operation");

            if (Options.PreviewOnly)
                return;

            EnsureConnectionIsOpen();

            if (expression.Operation != null)
                expression.Operation(Connection, null);
        }

        public override void Process(Expressions.RenameColumnExpression expression)
        {
            string columnDefinitionSql = string.Format(@"
SELECT CONCAT(
          CAST(COLUMN_TYPE AS CHAR),
          IF(ISNULL(CHARACTER_SET_NAME),
             '',
             CONCAT(' CHARACTER SET ', CHARACTER_SET_NAME)),
          IF(ISNULL(COLLATION_NAME),
             '',
             CONCAT(' COLLATE ', COLLATION_NAME)),
          ' ',
          IF(IS_NULLABLE = 'NO', 'NOT NULL ', ''),
          IF(IS_NULLABLE = 'NO' AND COLUMN_DEFAULT IS NULL,
             '',
             CONCAT('DEFAULT ', QUOTE(COLUMN_DEFAULT), ' ')),
          UPPER(extra))
  FROM INFORMATION_SCHEMA.COLUMNS
 WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", FormatHelper.FormatSqlEscape(expression.TableName), FormatHelper.FormatSqlEscape(expression.OldName));

            var columnDefinition = Read(columnDefinitionSql).Tables[0].Rows[0].Field<string>(0);

            Process(Generator.Generate(expression) + columnDefinition);
        }
    }
}