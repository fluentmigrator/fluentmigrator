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

namespace FluentMigrator.Tests.Integration.Processors.SQLite
{
    /// <remarks>
    /// SQLite has nowhere to store a description, and can't alter a column. The tests check that every other root
    /// still runs.
    /// </remarks>
    [TestFixture]
    [Category("SQLite")]
    public class SQLiteDescriptionTests : BaseDescriptionTests
    {
        protected override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.SQLite;

        protected override void ConfigureRunner(IMigrationRunnerBuilder builder) => builder.AddSQLite();

        protected override DescriptionSupport Support(DescriptionRoot root) =>
            root == DescriptionRoot.AlterColumn || root == DescriptionRoot.AlterTableAlterColumn
                ? DescriptionSupport.RootNotSupported
                : DescriptionSupport.NotWritten;

        protected override string TableDescriptionSql(string tableName) => null;

        protected override string ColumnDescriptionsSql(string tableName, string columnName) => null;
    }
}
