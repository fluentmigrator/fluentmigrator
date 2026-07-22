#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.SQLite;

namespace FluentMigrator.Tests.Aot;

/// <summary>
/// AOT smoke tests for SQL generation.
/// Verifies that the SQLite generator can produce SQL in a trimmed/AOT environment.
/// </summary>
public class SqlGenerationTests
{
    private SQLiteGenerator _generator = null!;

    [Before(Test)]
    public void SetUp()
    {
        _generator = new SQLiteGenerator();
    }

    [Test]
    [DisplayName("SQLite CREATE TABLE generation works in AOT")]
    public async Task GenerateCreateTable()
    {
        var expression = new CreateTableExpression { TableName = "Users" };
        expression.Columns.Add(new ColumnDefinition
        {
            Name = "Id",
            Type = DbType.Int32,
            IsPrimaryKey = true,
            IsIdentity = true,
        });
        expression.Columns.Add(new ColumnDefinition { Name = "Name", Type = DbType.String });

        var sql = _generator.Generate(expression);

        await Assert.That(sql).Contains("CREATE TABLE");
        await Assert.That(sql).Contains("\"Users\"");
    }

    [Test]
    [DisplayName("SQLite INSERT generation works in AOT")]
    public async Task GenerateInsertData()
    {
        var expression = new InsertDataExpression { TableName = "Users" };
        expression.Rows.Add(new InsertionDataDefinition
        {
            new("Name", "Alice"),
        });

        var sql = _generator.Generate(expression);

        await Assert.That(sql).Contains("INSERT INTO");
        await Assert.That(sql).Contains("\"Users\"");
    }

    [Test]
    [DisplayName("SQLite CREATE INDEX generation works in AOT")]
    public async Task GenerateCreateIndex()
    {
        var expression = new CreateIndexExpression
        {
            Index = new IndexDefinition
            {
                Name = "IX_Users_Email",
                TableName = "Users",
                Columns =
                {
                    new IndexColumnDefinition { Name = "Email", Direction = Direction.Ascending },
                },
            }
        };

        var sql = _generator.Generate(expression);

        await Assert.That(sql).Contains("CREATE INDEX");
    }
}
