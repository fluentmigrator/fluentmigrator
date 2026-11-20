#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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
using System.Data.Common;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.MySql;

[TestFixture]
[Category("Integration")]
[Category("MySql")]
public class MySqlCheckConstraintIntegrationTests
{
    [Test]
    public void FailedReplacementKeepsEnforcingTheOriginalPredicate()
    {
        IntegrationTestOptions.MySql.IgnoreIfNotEnabled();
        using var services = ServiceCollectionExtensions.CreateServices()
            .ConfigureRunner(builder => builder.AddMySql8().WithGlobalConnectionString(IntegrationTestOptions.MySql.ConnectionString))
            .BuildServiceProvider();
        using var scope = services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IMigrationProcessor>();
        var table = "items_" + Guid.NewGuid().ToString("N");
        var definition = new CheckConstraintDefinition { Name = "ck_" + table, TableName = table, Condition = "qty > 0" };

        processor.Execute($"CREATE TABLE {table} (qty INT)");
        try
        {
            processor.Process(new CreateCheckConstraintExpression { Constraint = definition });
            processor.Execute($"INSERT INTO {table} VALUES (1)");
            var replacement = definition.Clone();
            replacement.Condition = "qty > 1";
            var error = Assert.Catch<DbException>(() => processor.Process(new AlterCheckConstraintExpression
            {
                Action = CheckConstraintAction.ChangeCondition,
                Constraint = replacement
            }));
            Assert.That(error.Message, Does.Contain(definition.Name));

            var violation = Assert.Catch<DbException>(() => processor.Execute($"INSERT INTO {table} VALUES (0)"));
            Assert.That(violation.Message, Does.Contain(definition.Name));
        }
        finally
        {
            processor.Execute($"DROP TABLE {table}");
        }
    }
}
