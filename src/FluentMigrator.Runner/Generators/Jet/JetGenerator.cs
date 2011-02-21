

namespace FluentMigrator.Runner.Generators.Jet
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.Base;
    using FluentMigrator.Runner.Generators.Generic;

	public class JetGenerator : GenericGenerator
	{
		public JetGenerator() : base(new JetColumn(), new JetQuoter())
		{
		}

        public override string DropIndex { get { return "DROP INDEX {0} ON {1}"; } }

		public override string Generate(RenameTableExpression expression)
		{
            throw new DatabaseOperationNotSupportedExecption();
		}

		public override string Generate(RenameColumnExpression expression)
		{
            throw new DatabaseOperationNotSupportedExecption();
		}

		public override string Generate(AlterDefaultConstraintExpression expression)
		{
            throw new DatabaseOperationNotSupportedExecption();
		}
	}
}
