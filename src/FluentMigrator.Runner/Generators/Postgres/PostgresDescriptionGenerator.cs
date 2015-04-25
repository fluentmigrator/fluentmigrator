using System;
using FluentMigrator.Runner.Generators.PostgresBase;

namespace FluentMigrator.Runner.Generators.Postgres
{
    public class PostgresDescriptionGenerator : PostgresBaseDescriptionGenerator
    {
        public PostgresDescriptionGenerator() : base (new PostgresQuoter())
        {
        }
    }
}

