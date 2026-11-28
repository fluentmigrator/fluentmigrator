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

namespace FluentMigrator.Tests.Integration.Processors.Oracle.OracleManaged
{
    /// <remarks>
    /// Descriptions are comments, read from <c>USER_TAB_COMMENTS</c> and <c>USER_COL_COMMENTS</c>. The quoter leaves
    /// these names unquoted, so the dictionary has them in upper case.
    /// </remarks>
    [TestFixture]
    [Category("OracleManaged")]
    public class OracleManagedDescriptionTests : BaseDescriptionTests
    {
        protected override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.Oracle;

        protected override void ConfigureRunner(IMigrationRunnerBuilder builder) => builder.AddOracleManaged();

        protected override string TableDescriptionSql(string tableName) =>
            $"SELECT COMMENTS FROM USER_TAB_COMMENTS WHERE TABLE_NAME = UPPER('{tableName}')";

        protected override string ColumnDescriptionsSql(string tableName, string columnName) =>
            $"SELECT COMMENTS FROM USER_COL_COMMENTS WHERE TABLE_NAME = UPPER('{tableName}') AND COLUMN_NAME = UPPER('{columnName}')";
    }
}
