using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.SchemaDump.SchemaDumpers;
using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.SchemaDump
{
   public class SchemaDumpViewTests : SqlServerUnitTest
   {
      [Test]
      public void CanReadSingleView()
      {
         // Arrange
         var create = new CreateTableExpression
                              {
                                 TableName = "Foo",
                                 Columns =
                                    new List<ColumnDefinition> {new ColumnDefinition {Name = "Id", Type = DbType.Int32}}
                              };

         IList<ViewDefinition> views;
         

         // Act
         using (var connection = new SqlConnection(ConnectionString))
         {
            var processor = new SqlServerProcessor(connection, new SqlServer2005Generator(), new DebugAnnouncer(), new ProcessorOptions());

            processor.Process(create);

            Assert.IsTrue(processor.TableExists(string.Empty, create.TableName), "SqlServer");

            processor.Execute("CREATE VIEW FooView AS SELECT Id FROM Foo");

            var dumper = new SqlServerSchemaDumper(processor, new DebugAnnouncer());
            views = dumper.ReadViewSchema();

            processor.CommitTransaction();
         }
         

         // Assert

         Assert.AreEqual(1, views.Count);

         Assert.AreEqual("CREATE VIEW FooView AS SELECT Id FROM Foo", views[0].CreateViewSql);
      }

      [Test]
      public void CanReadMultipleViews()
      {
         // Arrange
         var create = new CreateTableExpression
         {
            TableName = "Foo",
            Columns =
               new List<ColumnDefinition> { new ColumnDefinition { Name = "Id", Type = DbType.Int32 } }
         };

         IList<ViewDefinition> views;


         // Act
         using (var connection = new SqlConnection(ConnectionString))
         {
            var processor = new SqlServerProcessor(connection, new SqlServer2005Generator(), new DebugAnnouncer(), new ProcessorOptions());

            processor.Process(create);

            Assert.IsTrue(processor.TableExists(string.Empty, create.TableName), "SqlServer");

            processor.Execute("CREATE VIEW FooViewC AS SELECT Id FROM Foo");
            processor.Execute("CREATE VIEW FooViewB AS SELECT Id FROM Foo");
            processor.Execute("CREATE VIEW FooViewA AS SELECT Id FROM Foo");

            var dumper = new SqlServerSchemaDumper(processor, new DebugAnnouncer());
            views = dumper.ReadViewSchema();

            processor.CommitTransaction();
         }


         // Assert

         Assert.AreEqual(3, views.Count);

         Assert.AreEqual("FooViewA", views[0].Name);
         Assert.AreEqual("FooViewB", views[1].Name);
         Assert.AreEqual("FooViewC", views[2].Name);
      }

      [Test]
      public void CanReadLongViewDefinition()
      {
         // Arrange
         var create = new CreateTableExpression
         {
            TableName = "Foo",
            Columns =
               new List<ColumnDefinition> { new ColumnDefinition { Name = "Id", Type = DbType.Int32 } }
         };

         IList<ViewDefinition> views;

         var createSql = new StringBuilder();
         createSql.Append("CREATE VIEW FooView As SELECT Id,");
         createSql.Append("'");
         createSql.Append(new string('A', 3000));
         createSql.Append("'");
         createSql.Append(" As LongText1,");
         createSql.Append("'");
         createSql.Append(new string('B', 3000));
         createSql.Append("'");
         createSql.Append(" As LongText2");
         createSql.Append(" FROM Foo");

         // Act
         using (var connection = new SqlConnection(ConnectionString))
         {
            var processor = new SqlServerProcessor(connection, new SqlServer2005Generator(), new DebugAnnouncer(), new ProcessorOptions());

            processor.Process(create);

            Assert.IsTrue(processor.TableExists(string.Empty, create.TableName), "SqlServer");

            processor.Execute(createSql.ToString());

            var dumper = new SqlServerSchemaDumper(processor, new DebugAnnouncer());
            views = dumper.ReadViewSchema();

            processor.CommitTransaction();
         }


         // Assert

         Assert.AreEqual(1, views.Count);
      }

   }
}
