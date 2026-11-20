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

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;

using Microsoft.Extensions.DependencyInjection;

using MySql.Data.MySqlClient;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.MySql;

[TestFixture]
[Category("Integration")]
[Category("MySql")]
public class MySqlCheckConstraintIntegrationTests
{
    [Test]
    public void SuccessfulReplacementEnforcesTheNewPredicate()
    {
        WithPositiveQuantityCheck((processor, constraint) =>
        {
            ReplaceWithQuantityGreaterThanOne(processor, constraint);

            processor.Execute($"INSERT INTO {constraint.TableName} VALUES (2)");
            AssertCheckViolation(() => processor.Execute($"INSERT INTO {constraint.TableName} VALUES (1)"), constraint.Name);
        });
    }

    [Test]
    public void FailedReplacementKeepsEnforcingTheOriginalPredicate()
    {
        WithPositiveQuantityCheck((processor, constraint) =>
        {
            processor.Execute($"INSERT INTO {constraint.TableName} VALUES (1)");
            AssertCheckViolation(() => ReplaceWithQuantityGreaterThanOne(processor, constraint), constraint.Name);

            processor.Execute($"INSERT INTO {constraint.TableName} VALUES (1)");
            AssertCheckViolation(() => processor.Execute($"INSERT INTO {constraint.TableName} VALUES (0)"), constraint.Name);
        });
    }

    private static void ReplaceWithQuantityGreaterThanOne(IMigrationProcessor processor, CheckConstraintDefinition constraint)
    {
        var replacement = constraint.Clone();
        replacement.Condition = "qty > 1";
        processor.Process(new AlterCheckConstraintExpression
        {
            Action = CheckConstraintAction.ChangeCondition,
            Constraint = replacement
        });
    }

    private static void AssertCheckViolation(TestDelegate action, string constraintName)
    {
        const int mySqlCheckViolation = 3819;
        const int mariaDbCheckViolation = 4025;
        var error = Assert.Throws<MySqlException>(action);
        Assert.That(error.Number, Is.AnyOf(mySqlCheckViolation, mariaDbCheckViolation));
        Assert.That(error.Message, Does.Contain(constraintName));
    }

    private static void WithPositiveQuantityCheck(Action<IMigrationProcessor, CheckConstraintDefinition> test)
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
            test(processor, definition);
        }
        finally
        {
            processor.Execute($"DROP TABLE {table}");
        }
    }
}
