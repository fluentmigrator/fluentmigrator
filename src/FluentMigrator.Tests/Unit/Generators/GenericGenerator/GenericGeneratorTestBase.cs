using FluentMigrator.Runner.Generators.Generic;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators
{
    public class GenericGeneratorTestBase : GeneratorTestBase
    {
        [SetUp]
        public void Setup()
        {
            SUT = new GenericGeneratorImplementor();
        }

        protected GenericGenerator SUT = default(GenericGenerator);
    }
}