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

using System.Collections.Generic;

using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using SqlGenerator = FluentMigrator.Runner.Generators.Generic.GenericGenerator;
using FluentMigrator.Runner.Generators.Redshift;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators;

[TestFixture]
public class UnsupportedCheckConstraintTests
{
    private static IEnumerable<SqlGenerator> Generators()
    {
        yield return new RedshiftGenerator();
        yield return new SQLiteGenerator(new SQLiteQuoter(false), new SQLiteTypeMap(false));
        yield return new SnowflakeGenerator(SnowflakeOptions.QuotingEnabled());
    }

    [TestCaseSource(nameof(Generators))]
    public void StrictModeRejectsCreateAndDrop(SqlGenerator generator)
    {
        generator.CompatibilityMode = CompatibilityMode.STRICT;
        Assert.Throws<DatabaseOperationNotSupportedException>(() => generator.Generate(new CreateCheckConstraintExpression()));
        Assert.Throws<DatabaseOperationNotSupportedException>(() => generator.Generate(new DeleteCheckConstraintExpression()));
    }

    [TestCaseSource(nameof(Generators))]
    public void LooseModeSkipsCreateAndDrop(SqlGenerator generator)
    {
        generator.CompatibilityMode = CompatibilityMode.LOOSE;
        Assert.That(generator.Generate(new CreateCheckConstraintExpression()), Is.Empty);
        Assert.That(generator.Generate(new DeleteCheckConstraintExpression()), Is.Empty);
    }
}
