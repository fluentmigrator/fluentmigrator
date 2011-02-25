

namespace FluentMigrator.Tests.Unit.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using FluentMigrator.Runner.Generators.Generic;

    public class GenericGeneratorTestBase : GeneratorTestBase
    {
        protected GenericGenerator SUT = default(GenericGenerator);

        [SetUp]
        public void Setup()
        {
            SUT = new GenericGeneratorImplementor();
        }
    }
}
