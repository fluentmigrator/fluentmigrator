using System;
using System.Configuration;
using System.Linq;

using NUnit.Framework;

namespace FluentMigrator.T4.Tests
{
    [TestFixture]
    [Category("Integration")]
    public class GeneratorTests
    {
        [Test]
        public void TestLoadPrimaryKeys()
        {
            var codeGenerator = GetCodeGenerator();

            var loadTables = codeGenerator.LoadTables();
            foreach (var loadTable in loadTables)
            {
                Console.WriteLine(loadTable);
            }
        }

        private static CodeGenerator GetCodeGenerator()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestConnection"];
            var codeGenerator = new CodeGenerator(connectionString.ConnectionString, connectionString.ProviderName, Console.Out, x => Console.WriteLine("WARNING: " + x));
            return codeGenerator;
        }

        [Test]
        public void TestLoadForeignKeys()
        {
            var codeGenerator = GetCodeGenerator();

            var loadTables = codeGenerator.LoadTables();
            foreach (var loadTable in loadTables)
            {
                Console.WriteLine(loadTable.ForeignKeys);
            }
        }
    }
}
