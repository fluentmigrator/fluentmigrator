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

namespace FluentMigrator.Runner.Processors.DotConnectOracle
{
    public class DotConnectOracleProcessor : GenericProcessorBase
    {
        public override string DatabaseType
        {
            get { return "Oracle"; }
        }

        public DotConnectOracleProcessor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options, DotConnectOracleDbFactory factory)
            : base(connection, factory, generator, announcer, options)
        {
        }

        public override bool SchemaExists(string schemaName)
        {
            if (schemaName == null)
                throw new ArgumentNullException("schemaName");

            if (schemaName.Length == 0)
                return false;

            return Exists("SELECT 1 FROM ALL_USERS WHERE USERNAME = '{0}'", schemaName.ToUpper());
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException("tableName");

            if (tableName.Length == 0)
                return false;

            if (string.IsNullOrEmpty(schemaName))
                return Exists("SELECT 1 FROM USER_TABLES WHERE TABLE_NAME = '{0}'", tableName.ToUpper());

            return Exists("SELECT 1 FROM ALL_TABLES WHERE OWNER = '{0}' AND TABLE_NAME = '{1}'", schemaName.ToUpper(), tableName.ToUpper());
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            if (columnName.Length == 0 || tableName.Length == 0)
                return false;

            if (string.IsNullOrEmpty(schemaName))
                return Exists("SELECT 1 FROM USER_TAB_COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", tableName.ToUpper(), columnName.ToUpper());

            return Exists("SELECT 1 FROM ALL_TAB_COLUMNS WHERE OWNER = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}'", schemaName.ToUpper(), tableName.ToUpper(), columnName.ToUpper());
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (constraintName == null)
                throw new ArgumentNullException("constraintName");

            //In Oracle DB constraint name is unique within the schema, so the table name is not used in the query

            if (constraintName.Length == 0)
                return false;

            if (String.IsNullOrEmpty(schemaName))
                return Exists("SELECT 1 FROM USER_CONSTRAINTS WHERE CONSTRAINT_NAME = '{0}'", constraintName.ToUpper());

            return Exists("SELECT 1 FROM ALL_CONSTRAINTS WHERE OWNER = '{0}' AND CONSTRAINT_NAME = '{1}'", schemaName.ToUpper(), constraintName.ToUpper());
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (indexName == null)
                throw new ArgumentNullException("indexName");

            //In Oracle DB index name is unique within the schema, so the table name is not used in the query

            if (indexName.Length == 0)
                return false;

            if (String.IsNullOrEmpty(schemaName))
                return Exists("SELECT 1 FROM USER_INDEXES WHERE INDEX_NAME = '{0}'", indexName.ToUpper());

            return Exists("SELECT 1 FROM ALL_INDEXES WHERE OWNER = '{0}' AND INDEX_NAME = '{1}'", schemaName.ToUpper(), indexName.ToUpper());
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return false;
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            return false;
        }

        public override void Execute(string template, params object[] args)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            EnsureConnectionIsOpen();

            using (var command = Factory.CreateCommand(String.Format(template, args), Connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public override bool Exists(string template, params object[] args)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            EnsureConnectionIsOpen();

            using (var command = Factory.CreateCommand(String.Format(template, args), Connection))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException("tableName");

            if (String.IsNullOrEmpty(schemaName))
                return Read("SELECT * FROM {0}", tableName.ToUpper());

            return Read("SELECT * FROM {0}.{1}", schemaName.ToUpper(), tableName.ToUpper());
        }

        public override DataSet Read(string template, params object[] args)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            EnsureConnectionIsOpen();

            var result = new DataSet();
            using (var command = Factory.CreateCommand(String.Format(template, args), Connection))
            {
                var adapter = Factory.CreateDataAdapter(command);
                adapter.Fill(result);
                return result;
            }
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            EnsureConnectionIsOpen();

            if (expression.Operation != null)
                expression.Operation(Connection, null);
        }

        protected override void Process(string sql)
        {
            Announcer.Sql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            EnsureConnectionIsOpen();

            using (var command = Factory.CreateCommand(sql, Connection))
                command.ExecuteNonQuery();
        }
    }
}