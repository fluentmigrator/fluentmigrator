using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.SchemaDump.SchemaDumpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.SchemaDump
{
   public class SchemaDumpTableTests : SqlServerUnitTest
   {
      [Test]
      [Ignore("To be tested further to returned expected type, [Grant 20110602")]
      public void SqlServerImageTypeIsBinary()
      {
         // Arrange
         IList<TableDefinition> tables;

         // Act
         using (var connection = new SqlConnection(ConnectionString))
         {
            var processor = new SqlServerProcessor(connection, new SqlServer2005Generator(), new DebugAnnouncer(), new ProcessorOptions());

            processor.Execute("CREATE TABLE Foo ( Data Image NULL )");

            Assert.IsTrue(processor.TableExists(string.Empty, "Foo"), "SqlServer");

            var dumper = new SqlServerSchemaDumper(processor, new DebugAnnouncer());
            tables = dumper.ReadDbSchema();

            processor.CommitTransaction();
         }
         

         // Assert
         tables.Count.ShouldBe(1);

         tables[0].Columns.ToList()[0].Type.ShouldBe(DbType.Binary);
      }
   }
}
