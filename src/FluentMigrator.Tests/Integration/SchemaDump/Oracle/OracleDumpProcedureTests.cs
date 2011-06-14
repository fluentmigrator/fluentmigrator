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
   public class OracleDumpProcedureTests : OracleUnitTest
   {
      [Test]
      public void CanCreateProcedure()
      {
         // Arrange

         // Act
         var definition = CreateProcedure("CREATE PROCEDURE Foo AS SELECT 1 FROM DUAL");


         // Assert
         definition.Name.ShouldBe("FOO");
         definition.Sql.Contains("CREATE OR REPLACE PROCEDURE").ShouldBeTrue();
         definition.Sql.Contains("SELECT 1 FROM DUAL").ShouldBeTrue();
      }

      private ProcedureDefinition CreateProcedure(string sql)
      {
         var processor = ((OracleProcessor)new OracleProcessorFactory().Create(ConnectionString,
         new DebugAnnouncer(), new ProcessorOptions()));

         processor.Execute(sql);

         var dumper = new OracleSchemaDumper(processor, new DebugAnnouncer());
         var procedures = dumper.ReadProcedures();

         processor.CommitTransaction();

         procedures.Count.ShouldBe(1);

         return procedures[0];
      }
   }
}
