using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.Oracle
{
    public class Oracle12CGenerator : OracleGenerator
    {
        public Oracle12CGenerator()
            : this(false)
        {
        }

        public Oracle12CGenerator(bool useQuotedIdentifiers)
            : this(GetQuoter(useQuotedIdentifiers))
        {
        }

        public Oracle12CGenerator(
            [NotNull] OracleQuoterBase quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public Oracle12CGenerator(
            [NotNull] OracleQuoterBase quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new Oracle12CColumn(quoter), quoter, new OracleDescriptionGenerator(), generatorOptions)
        {
        }
    }
}
