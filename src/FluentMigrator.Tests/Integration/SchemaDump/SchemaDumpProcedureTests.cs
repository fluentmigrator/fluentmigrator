using System;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.SchemaDump.SchemaDumpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.SchemaDump
{
   [TestFixture]
   public class SchemaDumpProcedureTests : SqlServerUnitTest
   {
      [Test]
      public void CanCreateProcedure()
      {
         // Arrange

         // Act
         var definition = CreateProcedure("CREATE PROCEDURE Foo AS SELECT GETDATE()");


         // Assert
         definition.Name.ShouldBe("Foo");
         definition.Sql.Contains("CREATE PROCEDURE Foo AS SELECT GETDATE()").ShouldBeTrue();
      }

      [Test]
      public void CanCreateProcedureWithMoreThanOneLineOfText()
      {
         // Arrange
         var sql = string.Format("CREATE PROCEDURE Foo AS SELECT '{0}'",
                                 new String('A', 5000));

         // Act
         var definition = CreateProcedure(sql);


         // Assert
         definition.Name.ShouldBe("Foo");
         definition.Sql.Contains(sql).ShouldBeTrue();
      }

      private ProcedureDefinition CreateProcedure(string sql)
      {
         var processor = (SqlServerProcessor)new SqlServerProcessorFactory().Create(ConnectionString,
         new DebugAnnouncer(), new ProcessorOptions());

         processor.Execute(sql);

         var dumper = new SqlServerSchemaDumper(processor, new DebugAnnouncer());
         var procedures = dumper.ReadProcedures();

         processor.CommitTransaction();

         procedures.Count.ShouldBe(1);

         return procedures[0];
      }
   }
}
