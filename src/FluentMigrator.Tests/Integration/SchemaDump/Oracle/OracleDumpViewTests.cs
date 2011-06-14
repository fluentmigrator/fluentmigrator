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

using System.Diagnostics;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.SchemaDump.SchemaDumpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.SchemaDump.Oracle
{
   [TestFixture]
   public class OracleDumpViewTests : OracleUnitTest
   {
      [Test]
      public void GetViewDefinition()
      {
         var view = GetView("SELECT 1 As One FROM DUAL");

         view.CreateViewSql.Contains(string.Format("CREATE OR REPLACE FORCE VIEW \"{0}\".\"{1}\"", TestDbName.ToUpper(), "FOO")).ShouldBeTrue();
         view.CreateViewSql.Contains("SELECT 1 As One FROM DUAL").ShouldBeTrue();
      }

      [Test]
      public void GetLongViewDefinition()
      {
         var sql = string.Format("SELECT '{0}' As One FROM DUAL", new string('A', 2000));
         var view = GetView(sql);

         view.CreateViewSql.Contains(string.Format("CREATE OR REPLACE FORCE VIEW \"{0}\".\"{1}\"", TestDbName.ToUpper(), "FOO")).ShouldBeTrue();
         view.CreateViewSql.Contains(sql).ShouldBeTrue();
      }

      /// <summary>
      /// Creates a single column table using the spplied type and retruns its <see cref="ColumnDefinition"/>
      /// </summary>
      /// <param name="type">The Sql Server data type to apply to the column</param>
      /// <returns>The translated <see cref="ColumnDefinition"/></returns>
      private ViewDefinition GetView(string sql)
      {
         // Act
         var processor = ((OracleProcessor)new OracleProcessorFactory().Create(ConnectionString,
         new DebugAnnouncer(), new ProcessorOptions()));


         processor.Execute("CREATE VIEW Foo AS {0}", sql);

         var dumper = new OracleSchemaDumper(processor, new DebugAnnouncer());
         var views = dumper.ReadViews();

         processor.CommitTransaction();

         views.Count.ShouldBe(1);

         return views[0];
      }
   }
}
