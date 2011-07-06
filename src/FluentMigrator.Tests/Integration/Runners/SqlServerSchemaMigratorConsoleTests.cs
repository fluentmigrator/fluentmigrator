#region License
// 
// Copyright (c) 2011, Grant Archibald
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
using System.Data.SqlClient;
using System.IO;
using FluentSchemaMigrator.Console;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Runners
{
    public class SqlServerSchemaMigratorConsoleTests : SqlServerUnitTest
    {
        private string _testFolder;

        private SqlServerUnitTest _sqlTarget;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _testFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testFolder);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            if (_sqlTarget != null )
                _sqlTarget.TearDown();

            Directory.Delete(_testFolder,true);
        }

        [Test]
        public void MustInitializeConsoleWithDatabaseArgument()
        {
            new SchemaMigratorConsole("/connection", ConnectionString);
            Assert.That(Environment.ExitCode == 1);
        }

        [Test]
        public void ErrorExitCodeIfInvalidArgument()
        {
            new SchemaMigratorConsole("/foobar");
            Assert.That(Environment.ExitCode == 1);
        }

        [Test]
        public void MustInitializeConsoleWithConnectionArgument()
        {
            new SchemaMigratorConsole("/db", "SqlServer");
            Assert.That(Environment.ExitCode == 1);
        }

        [Test]
        public void ErrorExitCodeIfInvalidConnectionString()
        {
            new SchemaMigratorConsole(
                "/db", "SqlServer",
                "/connection", "INVALID",
                "/workingdirectory", _testFolder);
            Assert.That(Environment.ExitCode == 1);
        }

        [Test]
        public void ErrorExitCodeIfUnsupportedDatabaseType()
        {
            new SchemaMigratorConsole(
                "/db", "Jet",
                "/connection", "INVALID",
                "/workingdirectory", _testFolder);
            Assert.That(Environment.ExitCode == 1);
        }


        [Test]
        public void CanGenerateEmptyDatabase()
        {
            new SchemaMigratorConsole(
                "/db", "SqlServer",
                "/connection", ConnectionString,
                "/workingdirectory", _testFolder);
            Assert.That(Environment.ExitCode == 0);

            Directory.Exists(Path.Combine(_testFolder, "Migrations")).ShouldBeTrue();

            Directory.GetFiles(Path.Combine(_testFolder, "Migrations")).Length.ShouldBe(0);
        }

        [Test]
        public void CanGenerateSingleTable()
        {

            ExecuteSql("CREATE TABLE Foo ( Id Int )");

            new SchemaMigratorConsole(
                "/db", "SqlServer",
                "/connection", ConnectionString,
                "/workingdirectory", _testFolder);
            Assert.That(Environment.ExitCode == 0);

            Directory.Exists(Path.Combine(_testFolder, "Migrations")).ShouldBeTrue();

            Directory.GetFiles(Path.Combine(_testFolder, "Migrations")).Length.ShouldBe(1);

            var migrationsFolder = Path.Combine(_testFolder, "Migrations");
            File.Exists(Path.Combine(migrationsFolder, "BaseMigration_1_Foo.cs")).ShouldBeTrue();
        }

        [Test]
        public void CanMigrateSingleTable()
        {
            _sqlTarget = new SqlServerUnitTest();
            _sqlTarget.Setup();

            ExecuteSql("CREATE TABLE Foo ( Id Int )");

            new SchemaMigratorConsole(
                "/db", "SqlServer",
                "/connection", ConnectionString,
                "/destdb", "SqlServer",
                "/destconnection", _sqlTarget.ConnectionString,
                "/workingdirectory", _testFolder);

            Assert.That(Environment.ExitCode == 0);
        }

        private void ExecuteSql(string sql, params SqlParameter[] parameters)
        {
            using ( var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using ( var cmd = new SqlCommand(sql,conn))
                {
                    foreach (var sqlParameter in parameters)
                    {
                        cmd.Parameters.Add(sqlParameter);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
