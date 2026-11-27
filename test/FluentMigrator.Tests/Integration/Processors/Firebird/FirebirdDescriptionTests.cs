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

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    /// <remarks>
    /// Firebird keeps descriptions in <c>RDB$DESCRIPTION</c>, set with <c>COMMENT ON</c>, but FluentMigrator's
    /// generator writes none, so the tests check that nothing is stored. The tests run on a temporary database, as the
    /// other Firebird integration tests do.
    /// </remarks>
    [TestFixture]
    [Category("Firebird")]
    public class FirebirdDescriptionTests : BaseDescriptionTests
    {
        private readonly FirebirdLibraryProber _prober = new FirebirdLibraryProber();

        private TemporaryDatabase _temporaryDatabase;

        protected override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.Firebird;

        protected override void ConfigureRunner(IMigrationRunnerBuilder builder) => builder.AddFirebird();

        protected override DescriptionSupport Support(DescriptionRoot root) => DescriptionSupport.NotWritten;

        protected override string OpenDatabase() => _temporaryDatabase.ConnectionString;

        [OneTimeSetUp]
        public void CreateTheTemporaryDatabase() => _temporaryDatabase = new TemporaryDatabase(ServerOptions, _prober);

        [OneTimeTearDown]
        public void DropTheTemporaryDatabase() => _temporaryDatabase?.Dispose();

        protected override string TableDescriptionSql(string tableName) =>
            $"SELECT CAST(RDB$DESCRIPTION AS VARCHAR(4000)) FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = UPPER('{tableName}')";

        protected override string ColumnDescriptionsSql(string tableName, string columnName) =>
            $"SELECT CAST(RDB$DESCRIPTION AS VARCHAR(4000)) FROM RDB$RELATION_FIELDS WHERE RDB$RELATION_NAME = UPPER('{tableName}') AND RDB$FIELD_NAME = UPPER('{columnName}')";
    }
}
