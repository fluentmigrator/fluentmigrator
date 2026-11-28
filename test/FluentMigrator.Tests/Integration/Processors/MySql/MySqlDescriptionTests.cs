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

namespace FluentMigrator.Tests.Integration.Processors.MySql
{
    /// <remarks>
    /// Descriptions are the table's and the column's <c>COMMENT</c>, read from <c>information_schema</c>.
    /// </remarks>
    [TestFixture]
    [Category("MySql")]
    public class MySqlDescriptionTests : BaseDescriptionTests
    {
        protected override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.MySql;

        protected override void ConfigureRunner(IMigrationRunnerBuilder builder) => builder.AddMySql8();

        protected override string TableDescriptionSql(string tableName) =>
            $"SELECT TABLE_COMMENT FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{tableName}'";

        protected override string ColumnDescriptionsSql(string tableName, string columnName) =>
            $"SELECT COLUMN_COMMENT FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}'";
    }
}
