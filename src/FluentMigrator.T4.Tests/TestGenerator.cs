using System;
using System.Configuration;
using System.Linq;

using NUnit.Framework;

namespace FluentMigrator.T4.Tests
{
    [TestFixture]
    public class TestGenerator
    {
        [Test]
        public void TestPrimaryKeys()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestConnection"];
            var codeGenerator = new CodeGenerator(connectionString.ConnectionString, connectionString.ProviderName, Console.Out, x => Console.WriteLine("WARNING: " + x));

            var loadTables = codeGenerator.LoadTables();
            foreach (var loadTable in loadTables)
            {
                Console.WriteLine(loadTable);
            }
        }
        
        [Test]
        public void TestForeignKeys()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestConnection"];
            var codeGenerator = new CodeGenerator(connectionString.ConnectionString, connectionString.ProviderName, Console.Out, x => Console.WriteLine("WARNING: " + x));

            var loadTables = codeGenerator.LoadTables();
            foreach (var loadTable in loadTables)
            {
                Console.WriteLine(loadTable.FKeys);
            }
        }
    }
}
