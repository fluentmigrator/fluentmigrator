using FluentMigrator.Runner.Generators.PostgresBase;
using System;

namespace FluentMigrator.Tests.Unit.Generators.PostgresBase
{
    public class TestPostgresGeneratorBase : PostgresBaseGenerator 
    {
        public TestPostgresGeneratorBase() 
            : base(
                new PostgresBaseColumn(new PostgresTypeMap(), new PostgresBaseQuoter()), 
                new PostgresBaseQuoter(), 
                new PostgresBaseDescriptionGenerator(new PostgresBaseQuoter())
            ) 
        { 
        }
     
    }
}

    