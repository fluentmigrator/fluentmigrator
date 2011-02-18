

namespace FluentMigrator.Tests.Unit.Generators
{
    using FluentMigrator.Runner.Generators.Jet;
    using NUnit.Framework;

    public class JetGeneratorTestBase : GeneratorTestBase
    {
        protected JetGenerator generator;

        [SetUp]
        public void SetUp()
        {
            generator = new JetGenerator();
        }
    }
}
