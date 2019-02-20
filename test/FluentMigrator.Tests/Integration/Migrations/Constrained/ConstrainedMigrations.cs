using FluentMigrator.Runner.Constraints;

namespace FluentMigrator.Tests.Integration.Migrations.Constrained
{
    namespace Constraints
    {
        [Migration(1)]
        public class Step1Migration : Migration
        {
            public override void Up() { }

            public override void Down() { }
        }

        [Migration(2)]
        [CurrentVersionMigrationConstraint(1)]
        public class Step2Migration : Migration
        {
            public override void Up() { }

            public override void Down() { }
        }

        [Migration(3)]
        [CurrentVersionMigrationConstraint(1)]
        public class Step2Migration2 : Migration
        {
            public override void Up() { }

            public override void Down() { }
        }
    }
    namespace ConstraintsMultiple
    {
        public class AlwaysFalseConstraint : MigrationConstraintAttribute
        {
            public AlwaysFalseConstraint() : base(opts => false)
            {

            }
        }
        public class AlwaysTrueConstraint : MigrationConstraintAttribute
        {
            public AlwaysTrueConstraint() : base(opts => true)
            {

            }
        }
        [Migration(1)]
        [AlwaysTrueConstraint]
        [AlwaysFalseConstraint]
        public class MultipleConstraintsMigration : Migration
        {
            public override void Up() { }

            public override void Down() { }
        }
    }

    namespace ConstraintsSuccess
    {
        [Migration(1)]
        [ConstraintsMultiple.AlwaysTrueConstraint]
        public class ConstrainedMigrationSuccess : Migration
        {
            public override void Up() { }

            public override void Down() { }
        }
    }
}
