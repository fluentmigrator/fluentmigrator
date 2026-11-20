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
using System;
using System.IO;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Oracle.ManagedDataAccess.Client;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [Category("Integration")]
    public abstract class OracleConstraintTestsBase : BaseConstraintTests
    {
        // Oracle Schemas are different from other RDBMS
        private string SchemaName =>  new OracleConnectionStringBuilder(Processor.Connection.ConnectionString).UserID;

        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private OracleProcessorBase Processor { get; set; }

        [Test]
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            using (var table = new OracleTestTable(Processor, null, "id int"))
            {
                table.WithUniqueConstraintOn("ID", "UC'id");
                Processor.ConstraintExists(null, table.Name, "UC'id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new OracleTestTable("Test'Table", Processor, null, "id int"))
            {
                table.WithUniqueConstraintOn("ID");
                Processor.ConstraintExists(null, table.Name, "UC_id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new OracleTestTable(Processor, null, "id int"))
            {
                table.WithUniqueConstraintOn("ID");
                Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            using (var table = new OracleTestTable(Processor, SchemaName, "id int"))
            {
                table.WithUniqueConstraintOn("ID");
                Processor.ConstraintExists(SchemaName, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ConstraintExists(SchemaName, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            using (var table = new OracleTestTable(Processor, null, "id int"))
            {
                table.WithUniqueConstraintOn("ID");
                Processor.ConstraintExists(null, table.Name, "UC_id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            using (var table = new OracleTestTable(Processor, SchemaName, "id int"))
            {
                table.WithUniqueConstraintOn("ID");
                Processor.ConstraintExists(SchemaName, table.Name, "UC_id").ShouldBeTrue();
            }
        }

        [Test]
        public void CheckConstraintWithoutValidationEnforcesNewRowsOnly()
        {
            using var table = new OracleTestTable("T_" + Guid.NewGuid().ToString("N").Substring(0, 16).ToUpperInvariant(), Processor, null, "quantity int");
            var constraintName = "CK_" + table.Name;
            Processor.Execute($"INSERT INTO {table.Name} (quantity) VALUES (-1)");

            Processor.Process(new CreateCheckConstraintExpression
            {
                Constraint = { TableName = table.Name, Name = constraintName, Condition = "quantity > 0", NoCheck = true },
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
                Constraint = { TableName = table.Name, Name = constraintName, Condition = "code = 'Don''t;stop'" },
            });

            Processor.Process(new AlterCheckConstraintExpression
            {
                Action = CheckConstraintAction.ChangeCondition,
                Constraint = { TableName = table.Name, Name = constraintName, Condition = "code <> 'Don''t;stop'" },
            });

            Processor.Execute($"INSERT INTO {table.Name} (code) VALUES ('Continue')");
            var exception = Should.Throw<Exception>(() => Processor.Execute($"INSERT INTO {table.Name} (code) VALUES ('Don''t;stop')"));
            exception.InnerException.ShouldBeOfType<OracleException>().Number.ShouldBe(2290);
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            IntegrationTestOptions.Oracle.IgnoreIfNotEnabled();

            var serivces = AddOracleServices(ServiceCollectionExtensions.CreateServices())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Oracle.ConnectionString));
            ServiceProvider = serivces.BuildServiceProvider();
        }

        [OneTimeTearDown]
        public void ClassTearDown()
        {
            ServiceProvider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            ServiceScope = ServiceProvider.CreateScope();
            Processor = ServiceScope.ServiceProvider.GetRequiredService<OracleProcessorBase>();

            OracleTestUtils.CheckNativeOracleDataAccess(Processor);
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }

        protected abstract IServiceCollection AddOracleServices(IServiceCollection services);
    }
}
