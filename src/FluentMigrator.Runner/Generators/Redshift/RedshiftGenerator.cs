using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Generators.PostgresBase;

namespace FluentMigrator.Runner.Generators.Redshift
{
    public class RedshiftGenerator : PostgresBaseGenerator
    {
        public RedshiftGenerator()
            : base(new RedshiftColumn(), new RedshiftQuoter(), new RedshiftDescriptionGenerator())
        {
        }

        public override string Generate(CreateIndexExpression expression)
        {
            return string.Empty;
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            return string.Empty;
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            return string.Empty;
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return string.Empty;
        }
    }
}
