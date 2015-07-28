using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Generators.PostgresBase;

namespace FluentMigrator.Runner.Generators.Redshift
{
    /// <summary>
    /// almost copied from OracleDescriptionGenerator,
    /// modified for escaping table description
    /// </summary>
    public class RedshiftDescriptionGenerator : PostgresBaseDescriptionGenerator
    {
        public RedshiftDescriptionGenerator() : base(new RedshiftQuoter()) 
        {
        }
    }
}
