using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Jet
{
    public class JetGenerator : GenericGenerator
    {
        public JetGenerator()
            : this(new JetQuoter())
        {
        }

        public JetGenerator(JetQuoter quoter)
            : base(new JetColumn(), quoter, new EmptyDescriptionGenerator())
        {
        }

        public override string DropIndex { get { return "DROP INDEX {0} ON {1}"; } }

        public override string Generate(RenameTableExpression expression)
        {
            return CompatabilityMode.HandleCompatabilty("Renaming of tables is not supported for Jet");
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return CompatabilityMode.HandleCompatabilty("Renaming of columns is not supported for Jet");
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return CompatabilityMode.HandleCompatabilty("Altering of default constraints is not supported for Jet");
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            return CompatabilityMode.HandleCompatabilty("Sequences are not supported for Jet");
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return CompatabilityMode.HandleCompatabilty("Sequences are not supported for Jet");
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return CompatabilityMode.HandleCompatabilty("Default constraints are not supported");
        }
    }
}
