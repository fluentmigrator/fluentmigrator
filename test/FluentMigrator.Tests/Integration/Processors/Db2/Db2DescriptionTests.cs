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
using FluentMigrator.Tests.SyntaxRoots;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Db2
{
    /// <remarks>
    /// Db2 keeps descriptions in the catalog's <c>REMARKS</c>, set with <c>COMMENT ON</c>, but FluentMigrator's
    /// generator writes none, so the tests check that nothing is stored.
    /// </remarks>
    [TestFixture]
    [Category("Db2")]
    public class Db2DescriptionTests : BaseDescriptionTests
    {
        protected override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.Db2;

        protected override void ConfigureRunner(IMigrationRunnerBuilder builder) => builder.AddDb2();

        protected override DescriptionSupport Support(DescriptionRoot root) => DescriptionSupport.NotWritten;

        protected override string TableDescriptionSql(string tableName) =>
            $"SELECT REMARKS FROM SYSCAT.TABLES WHERE TABSCHEMA = CURRENT SCHEMA AND TABNAME = UPPER('{tableName}')";

        protected override string ColumnDescriptionsSql(string tableName, string columnName) =>
            $"SELECT REMARKS FROM SYSCAT.COLUMNS WHERE TABSCHEMA = CURRENT SCHEMA AND TABNAME = UPPER('{tableName}') AND COLNAME = UPPER('{columnName}')";
    }
}
