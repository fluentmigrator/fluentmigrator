#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

using System.Data.SqlClient;
using System.Data.SQLite;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Tests.Integration.Migrations;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration
{
   [TestFixture]
	public class ScriptMigrationTests : IntegrationTestBase
	{
      
      [Test]
      public void CreatesViewOnSqlServer()
      {
         ExecuteWithSqlServer(processor =>
         {
            var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), new RunnerContext(new TextWriterAnnouncer(System.Console.Out)) { Namespace = "FluentMigrator.Tests.Integration.Migrations.SqlServerScriptMigration" }, processor);

            // First check that view does not exist
            try
            {
               processor.ReadTableData(string.Empty, "Foo");
               // .. it does remove it
               runner.Down(new SqlServerScriptMigration());
            }
            catch (SqlException ex){
               // Not found .. proceed with test
               Assert.IsTrue(ex.Message.Contains("Invalid object name 'Foo'"));
            }
            
            runner.Up(new SqlServerScriptMigration());
            processor.Exists("SELECT * FROM Foo").ShouldBeTrue();

            runner.Down(new SqlServerScriptMigration());
         }, IntegrationTestOptions.SqlServer, true);
      }

      [Test]
      public void DoesNotCreatesViewOnSqlLite()
      {
         ExecuteWithSqlite(processor =>
         {
            var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), new RunnerContext(new TextWriterAnnouncer(System.Console.Out)) { Namespace = "FluentMigrator.Tests.Integration.Migrations.ScriptMigration" }, processor);

            runner.Up(new SqlServerScriptMigration());

            try
            {
               processor.Exists("SELECT * FROM Foo");
            }
            catch (SQLiteException ex)
            {
               Assert.IsTrue(ex.Message.Contains("no such table: Foo"));
            }
            
         }, IntegrationTestOptions.SqlLite);
      }
	}
}
