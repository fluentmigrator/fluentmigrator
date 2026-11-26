#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using System;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Oracle.ManagedDataAccess.Client;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Oracle.OracleManaged
{
    [TestFixture]
    [Category("OracleManaged")]
    public class OracleManagedConstraintTests : OracleConstraintTestsBase
    {
        [Test]
        public void CheckConstraintWithoutValidationEnforcesNewRowsOnly()
        {
            using var table = new OracleTestTable("T_" + Guid.NewGuid().ToString("N").Substring(0, 16).ToUpperInvariant(), Processor, null, "quantity int");
            var constraintName = "CK_" + table.Name;
            Processor.Execute($"INSERT INTO {table.Name} (quantity) VALUES (-1)");

            Processor.Process(new CreateCheckConstraintExpression
            {
                Constraint = { TableName = table.Name, ConstraintName = constraintName, Condition = "quantity > 0", NoCheck = true },
            });

            Processor.Execute($"INSERT INTO {table.Name} (quantity) VALUES (1)");
            var exception = Should.Throw<Exception>(() => Processor.Execute($"INSERT INTO {table.Name} (quantity) VALUES (-1)"));
            exception.InnerException.ShouldBeOfType<OracleException>().Number.ShouldBe(2290);
        }

        [Test]
        public void ReplacingCheckConstraintEnforcesTheNewConditionWithQuotedLiterals()
        {
            using var table = new OracleTestTable("T_" + Guid.NewGuid().ToString("N").Substring(0, 16).ToUpperInvariant(), Processor, null, "code varchar2(20)");
            var constraintName = "CK_" + table.Name;
            Processor.Process(new CreateCheckConstraintExpression
            {
                Constraint = { TableName = table.Name, ConstraintName = constraintName, Condition = "code = 'Don''t;stop'" },
            });

            Processor.Process(new AlterCheckConstraintExpression
            {
                Action = CheckConstraintAction.ChangeCondition,
                Constraint = { TableName = table.Name, ConstraintName = constraintName, Condition = "code <> 'Don''t;stop'" },
            });

            Processor.Execute($"INSERT INTO {table.Name} (code) VALUES ('Continue')");
            var exception = Should.Throw<Exception>(() => Processor.Execute($"INSERT INTO {table.Name} (code) VALUES ('Don''t;stop')"));
            exception.InnerException.ShouldBeOfType<OracleException>().Number.ShouldBe(2290);
        }

        /// <inheritdoc />
        protected override IServiceCollection AddOracleServices(IServiceCollection services)
        {
            return services.ConfigureRunner(r => r.AddOracleManaged());
        }
    }
}
