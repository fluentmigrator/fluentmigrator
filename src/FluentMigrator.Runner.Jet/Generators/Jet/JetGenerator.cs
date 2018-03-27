using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Jet
{
    public class JetGenerator : GenericGenerator
    {
        public JetGenerator()
            : base(new JetColumn(), new JetQuoter(), new EmptyDescriptionGenerator())
        {
        }

        public override string DropIndex { get { return "DROP INDEX {0} ON {1}"; } }

        public override string Generate(RenameTableExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Renaming of tables is not supported for Jet");
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Renaming of columns is not supported for Jet");
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Altering of default constraints is not supported for Jet");
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Sequences are not supported for Jet");
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Sequences are not supported for Jet");
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Default constraints are not supported");
        }
    }
}
