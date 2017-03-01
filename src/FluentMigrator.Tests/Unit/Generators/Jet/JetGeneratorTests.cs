using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Jet;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.Jet
{
    public class JetGeneratorTests
    {
        protected JetGenerator Generator;

        public JetGeneratorTests()
        {
            Generator = new JetGenerator();
        }

        [Fact]
        public void CanAlterSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Fact]
        public void CanCreateSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Fact]
        public void CanDropSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new DeleteSchemaExpression()));
        }

        [Fact]
        public void CanRenameColumnInStrictMode()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Fact]
        public void CanRenameTableInStrictMode()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }
    }
}
