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
   public class OracleDumpFunctionsTests : OracleUnitTest
   {
      [Test]
      public void CanCreateFunction()
      {
         // Arrange

         // Act
         var definition = CreateFunction("CREATE FUNCTION Foo AS SELECT 1 FROM DUAL");


         // Assert
         definition.Name.ShouldBe("FOO");
         definition.Sql.Contains("CREATE OR REPLACE FUNCTION").ShouldBeTrue();
         definition.Sql.Contains("SELECT 1 FROM DUAL").ShouldBeTrue();
      }

      private FunctionDefinition CreateFunction(string sql)
      {
         var processor = ((OracleProcessor)new OracleProcessorFactory().Create(ConnectionString,
         new DebugAnnouncer(), new ProcessorOptions()));

         processor.Execute(sql);

         var dumper = new OracleSchemaDumper(processor, new DebugAnnouncer());
         var functions = dumper.ReadFunctions();

         processor.CommitTransaction();

         functions.Count.ShouldBe(1);

         return functions[0];
      }
   }
}
