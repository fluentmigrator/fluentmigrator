#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

using FluentMigrator.Runner;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Postgres
{
    /// <remarks>
    /// Descriptions are comments, read with <c>obj_description</c> and <c>col_description</c>. The quoter quotes every
    /// name, so the catalog has them as written.
    /// </remarks>
    [TestFixture]
    [Category("Postgres")]
    public class PostgresDescriptionTests : BaseDescriptionTests
    {
        protected override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.Postgres;

        protected override void ConfigureRunner(IMigrationRunnerBuilder builder) => builder.AddPostgres();

        protected override string TableDescriptionSql(string tableName) =>
            $"SELECT obj_description('\"public\".\"{tableName}\"'::regclass, 'pg_class')";

        protected override string ColumnDescriptionsSql(string tableName, string columnName) =>
            $"SELECT col_description(a.attrelid, a.attnum) FROM pg_catalog.pg_attribute a WHERE a.attrelid = '\"public\".\"{tableName}\"'::regclass AND a.attname = '{columnName}'";
    }
}
