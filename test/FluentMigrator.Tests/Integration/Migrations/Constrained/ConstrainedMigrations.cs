#region License
//
// Copyright (c) 2019, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion
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
