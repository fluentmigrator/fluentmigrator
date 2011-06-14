using System;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.SchemaDump.SchemaDumpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.SchemaDump
{
   [TestFixture]
   public class SchemaDumpFunctionsTests : SqlServerUnitTest
   {
      [Test]
      public void CanCreateFunction()
      {
         // Arrange

         // Act
         var definition = CreateFunction("CREATE FUNCTION Foo() RETURNS int BEGIN RETURN 1; END");


         // Assert
         definition.Name.ShouldBe("Foo");
         definition.Sql.Contains("CREATE FUNCTION Foo() RETURNS int BEGIN RETURN 1; END").ShouldBeTrue();
      }

      [Test]
      public void CanCreateFunctionWithMoreThanOneLineOfText()
      {
         // Arrange
         var sql = string.Format("CREATE FUNCTION Foo() RETURNS varchar(max) BEGIN RETURN '{0}'; END",
                                 new String('A', 5000));

         // Act
         var definition = CreateFunction(sql);


         // Assert
         definition.Name.ShouldBe("Foo");
         definition.Sql.Contains(sql).ShouldBeTrue();
      }

      private FunctionDefinition CreateFunction(string sql)
      {
         var processor = ((SqlServerProcessor)new SqlServerProcessorFactory().Create(ConnectionString,
         new DebugAnnouncer(), new ProcessorOptions()));

         processor.Execute(sql);

         var dumper = new SqlServerSchemaDumper(processor, new DebugAnnouncer());
         var functions = dumper.ReadFunctions();

         processor.CommitTransaction();

         functions.Count.ShouldBe(1);

         return functions[0];
      }
   }
}
