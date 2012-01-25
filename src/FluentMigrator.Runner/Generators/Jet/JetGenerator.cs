using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Jet
{
    public class JetGenerator : GenericGenerator
    {
        public JetGenerator()
            : base(new JetColumn(), new JetQuoter()) { }

        public override string DropIndex { get { return "DROP INDEX {0} ON {1}"; } }

        public override string Generate(RenameTableExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("MySQL does not support renaming tables.");
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("MySQL does not support renaming columns.");
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("MySQL does not support altering default constraints.");
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("MySQL does not support sequences.");
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("MySQL does not support sequences.");
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("MySQL does not support default constraints.");
        }
    }
}
