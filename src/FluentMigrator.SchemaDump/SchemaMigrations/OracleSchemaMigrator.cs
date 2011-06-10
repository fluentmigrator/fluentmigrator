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

using System.Data;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.SchemaDump.SchemaDumpers;

namespace FluentMigrator.SchemaDump.SchemaMigrations
{
   /// <summary>
   /// Migrates the schema items from Oracle to another supprted database type
   /// </summary>
   /// <example>
   /// <code>
   /// var migrator = new OracleSchemaMigrator(new TextWriterAnnouncer(Console.Out));
   /// var context = new SchemaMigrationContext {
   ///    FromConnectionString = ToConnectionString = "Uid=Bar;Pwd=Bar;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SID=XE)))"
   ///    , ToDatabaseType = DatabaseType.Oracle
   ///    , ToConnectionString = "Uid=Foo;Pwd=Foo;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SID=XE)))"
   ///    , MigrationsDirectory = @".\Migrations"
   /// };
   /// migrator.Migrate(context);
   /// </code>
   /// </example>
   public class OracleSchemaMigrator : BaseSchemaMigrator
   {
      public OracleSchemaMigrator(IAnnouncer announcer)
         : base(announcer)
      {
      }

      protected override ISchemaDumper GetSchemaDumper(IMigrationProcessor processor)
      {
         return new OracleSchemaDumper((OracleProcessor)processor, Announcer);
      }

      protected override IMigrationProcessor GetProcessor(IDbConnection connection)
      {
         return new OracleProcessor(connection, new OracleGenerator(), Announcer,
                                       new ProcessorOptions());
      }

      protected override IDbConnection GetConnection(SchemaMigrationContext context)
      {
         return OracleFactory.GetOpenConnection(context.FromConnectionString);
      }
   }
}