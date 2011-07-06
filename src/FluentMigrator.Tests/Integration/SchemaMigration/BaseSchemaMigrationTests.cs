using FluentMigrator.Expressions;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.SchemaMigration
{
   public class BaseSchemaMigrationTests
   {
      protected OracleUnitTest OracleContext { get; set; }

      /// <summary>
      /// Asserts that a given oracle table have been migrated
      /// </summary>
      /// <param name="createTables">The tables to be checked</param>
      protected void AssertOracleTablesExist(params CreateTableExpression[] createTables)
      {
         if (createTables == null)
            return;
         var oracleProcessor = new OracleProcessorFactory().Create(OracleContext.ConnectionString, new NullAnnouncer(),
                                                                   new ProcessorOptions());

         foreach (var create in createTables)
            Assert.IsTrue(oracleProcessor.TableExists(string.Empty, create.TableName), "Oracle");
      }

      /// <summary>
      /// Asserts that a given oracle table has not been migrated
      /// </summary>
      /// <param name="createTables">The tables to be checked</param>
      public void AssertOracleTablesDoNotExist(params CreateTableExpression[] createTables)
      {
         if (createTables == null)
            return;
         var oracleProcessor = new OracleProcessorFactory().Create(OracleContext.ConnectionString, new NullAnnouncer(),
                                                                   new ProcessorOptions());

         foreach (var create in createTables)
            Assert.IsFalse(oracleProcessor.TableExists(string.Empty, create.TableName), "Oracle");
      }
   }
}