#region License
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
#endregion

using System.Data;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Postgres
{
    [TestFixture]
    [Category("Integration")]
    [Category("Postgres")]
    public class PostgresSecurityLabelIntegrationTests
    {
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private PostgresProcessor Processor { get; set; }

        [Test]
        public void CanApplySecurityLabelToTable()
        {
            using (var table = new PostgresTestTable(Processor, "public", "id int"))
            {
                var sql = $"SECURITY LABEL ON TABLE \"public\".\"{table.Name}\" IS 'test label';";

                Processor.Execute(sql);

                var hasLabel = SecurityLabelExists("pg_class", table.Name, "test label");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void CanApplySecurityLabelToColumn()
        {
            using (var table = new PostgresTestTable(Processor, "public", "id int"))
            {
                var sql = $"SECURITY LABEL ON COLUMN \"public\".\"{table.Name}\".\"id\" IS 'column label';";

                Processor.Execute(sql);

                var hasLabel = ColumnSecurityLabelExists(table.Name, "id", "column label");
                hasLabel.ShouldBeTrue();
            }
        }

        [Test]
        public void CanDeleteSecurityLabelFromTable()
        {
            using (var table = new PostgresTestTable(Processor, "public", "id int"))
            {
                Processor.Execute($"SECURITY LABEL ON TABLE \"public\".\"{table.Name}\" IS 'to be deleted';");

                var hasLabel = SecurityLabelExists("pg_class", table.Name, "to be deleted");
                hasLabel.ShouldBeTrue();

                Processor.Execute($"SECURITY LABEL ON TABLE \"public\".\"{table.Name}\" IS NULL;");

                hasLabel = SecurityLabelExists("pg_class", table.Name, "to be deleted");
                hasLabel.ShouldBeFalse();
            }
        }

        [Test]
        public void CanApplySecurityLabelToSchema()
        {
            const string schemaName = "test_security_label_schema";

            try
            {
                Processor.Execute($"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\";");
                Processor.Execute($"SECURITY LABEL ON SCHEMA \"{schemaName}\" IS 'schema label';");

                var hasLabel = SecurityLabelExists("pg_namespace", schemaName, "schema label");
                hasLabel.ShouldBeTrue();
            }
            finally
            {
                Processor.Execute($"DROP SCHEMA IF EXISTS \"{schemaName}\" CASCADE;");
            }
        }

        [Test]
        public void CanApplySecurityLabelToRole()
        {
            const string roleName = "test_security_label_role";

            try
            {
                Processor.Execute($"CREATE ROLE \"{roleName}\";");
                Processor.Execute($"SECURITY LABEL ON ROLE \"{roleName}\" IS 'role label';");

                var hasLabel = SecurityLabelExists("pg_authid", roleName, "role label");
                hasLabel.ShouldBeTrue();
            }
            finally
            {
                Processor.Execute($"DROP ROLE IF EXISTS \"{roleName}\";");
            }
        }

        [Test]
        public void CanApplySecurityLabelToView()
        {
            const string viewName = "test_security_label_view";

            using (var table = new PostgresTestTable(Processor, "public", "id int"))
            {
                try
                {
                    Processor.Execute($"CREATE VIEW \"public\".\"{viewName}\" AS SELECT id FROM \"public\".\"{table.Name}\";");
                    Processor.Execute($"SECURITY LABEL ON VIEW \"public\".\"{viewName}\" IS 'view label';");

                    var hasLabel = SecurityLabelExists("pg_class", viewName, "view label");
                    hasLabel.ShouldBeTrue();
                }
                finally
                {
                    Processor.Execute($"DROP VIEW IF EXISTS \"public\".\"{viewName}\";");
                }
            }
        }

        private bool SecurityLabelExists(string catalogTable, string objectName, string expectedLabel)
        {
            var query = $@"
                SELECT 1 
                FROM pg_seclabels sl
                JOIN {catalogTable} c ON sl.objoid = c.oid
                WHERE c.{{0}} = '{objectName}' AND sl.label = '{expectedLabel}'";

            var columnName = catalogTable switch
            {
                "pg_class" => "relname",
                "pg_namespace" => "nspname",
                "pg_authid" => "rolname",
                _ => "name"
            };

            return Processor.Exists(string.Format(query, columnName));
        }

        private bool ColumnSecurityLabelExists(string tableName, string columnName, string expectedLabel)
        {
            var query = $@"
                SELECT 1 
                FROM pg_seclabels sl
                JOIN pg_class c ON sl.objoid = c.oid
                JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                WHERE c.relname = '{tableName}' 
                  AND a.attname = '{columnName}'
                  AND sl.label = '{expectedLabel}'";

            return Processor.Exists(query);
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            IntegrationTestOptions.Postgres.IgnoreIfNotEnabled();

            var services = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(r => r.AddPostgres())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Postgres.ConnectionString));
            ServiceProvider = services.BuildServiceProvider();
        }

        [OneTimeTearDown]
        public void ClassTearDown()
        {
            ServiceProvider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            ServiceScope = ServiceProvider.CreateScope();
            Processor = ServiceScope.ServiceProvider.GetRequiredService<PostgresProcessor>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }
    }
}
