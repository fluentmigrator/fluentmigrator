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

namespace FluentMigrator.Tests.Integration.Processors.SqlServer.SqlServer2016
{
    /// <remarks>
    /// Descriptions are extended properties. Every property of the column is read, because <c>Alter.Column</c> stores
    /// each additional description as an <c>MS_&lt;name&gt;</c> property of its own, while the create roots put them all
    /// in <c>MS_Description</c>.
    /// </remarks>
    [TestFixture]
    [Category("SqlServer2016")]
    public class SqlServer2016DescriptionTests : BaseDescriptionTests
    {
        protected override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.SqlServer2016;

        protected override void ConfigureRunner(IMigrationRunnerBuilder builder) => builder.AddSqlServer2016();

        protected override string TableDescriptionSql(string tableName) =>
            $"SELECT CAST(value AS nvarchar(4000)) FROM sys.fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'dbo', N'TABLE', N'{tableName}', NULL, NULL)";

        protected override string ColumnDescriptionsSql(string tableName, string columnName) =>
            $"SELECT CAST(value AS nvarchar(4000)) FROM sys.fn_listextendedproperty(NULL, N'SCHEMA', N'dbo', N'TABLE', N'{tableName}', N'COLUMN', N'{columnName}')";
    }
}
