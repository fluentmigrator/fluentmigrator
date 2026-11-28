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

using System;
using System.Linq;

using FluentMigrator.Runner;
using FluentMigrator.Tests.SyntaxRoots;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Db2
{
    /// <remarks>
    /// <para>
    /// Db2 keeps descriptions in the catalog's <c>REMARKS</c>, set with <c>COMMENT ON</c>, but FluentMigrator's
    /// generator writes none, so the tests check that nothing is stored.
    /// </para>
    /// <para>
    /// They run on a developer machine only, with <c>Invoke-Db2Tests.ps1</c> next to this project, which accepts IBM's
    /// license terms for the Db2 image and driver, and are skipped on a build server.
    /// </para>
    /// </remarks>
    [TestFixture]
    [Category("Db2")]
    public class Db2DescriptionTests : BaseDescriptionTests
    {
        [OneTimeSetUp]
        public void SkipOnABuildServer()
        {
            if (IsBuildServer())
            {
                Assert.Ignore("Db2's description tests run on a developer machine only: see test/FluentMigrator.Tests/Invoke-Db2Tests.ps1.");
            }
        }

        /// <summary>
        /// GitHub Actions sets <c>GITHUB_ACTIONS</c>, Azure Pipelines <c>TF_BUILD</c>, and most CI systems <c>CI</c>.
        /// </summary>
        private static bool IsBuildServer() =>
            new[] { "GITHUB_ACTIONS", "TF_BUILD", "CI" }
                .Select(Environment.GetEnvironmentVariable)
                .Any(value => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase));

        protected override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.Db2;

        protected override void ConfigureRunner(IMigrationRunnerBuilder builder) => builder.AddDb2();

        protected override DescriptionSupport Support(DescriptionRoot root) => DescriptionSupport.NotWritten;

        /// <summary>
        /// Db2 changes a column's nullability with <c>ALTER COLUMN ... SET NOT NULL</c>, but the generator appends
        /// <c>NOT NULL</c> to <c>SET DATA TYPE</c>, which Db2 rejects with SQL0104N.
        /// </summary>
        protected override KnownError KnownFailure(DescriptionRoot root) =>
            root == DescriptionRoot.AlterColumn || root == DescriptionRoot.AlterTableAlterColumn
                ? new KnownError("SQL0104N", "Db2's generator writes ALTER COLUMN ... SET DATA TYPE ... NOT NULL, which Db2 rejects (#2476)")
                : null;

        protected override string TableDescriptionSql(string tableName) =>
            $"SELECT REMARKS FROM SYSCAT.TABLES WHERE TABSCHEMA = CURRENT SCHEMA AND TABNAME = UPPER('{tableName}')";

        protected override string ColumnDescriptionsSql(string tableName, string columnName) =>
            $"SELECT REMARKS FROM SYSCAT.COLUMNS WHERE TABSCHEMA = CURRENT SCHEMA AND TABNAME = UPPER('{tableName}') AND COLNAME = UPPER('{columnName}')";
    }
}
