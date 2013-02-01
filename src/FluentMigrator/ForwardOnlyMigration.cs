

namespace FluentMigrator
{
    using System;

    public abstract class ForwardOnlyMigration : Migration
    {
        public sealed override void Down()
        {
            throw new InvalidOperationException("Only forward migration is supported");
        }
    }
}
