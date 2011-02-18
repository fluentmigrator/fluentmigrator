

using NUnit.Framework;
using FluentMigrator.Runner.Generators.MySql;
namespace FluentMigrator.Tests.Unit.Generators
{

    public class MySqlGeneratorTestBase : GeneratorTestBase
    {
        protected MySqlGenerator generator;

        [SetUp]
        public void SetUp()
        {
            generator = new MySqlGenerator();
        }
    }
}
