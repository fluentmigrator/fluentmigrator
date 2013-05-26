using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Jet;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Jet
{
    [TestFixture]
    public class JetGeneratorTests
    {
        protected JetGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new JetGenerator();
        }

        [Test]
        public void CanAlterSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanCreateSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanDropSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new DeleteSchemaExpression()));
        }

        [Test]
        public void CanRenameColumnInStrictMode()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public void CanRenameTableInStrictMode()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }
    }
}
