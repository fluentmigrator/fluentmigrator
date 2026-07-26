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
using FluentMigrator.Validation;

namespace FluentMigrator.Tests.Aot;

/// <summary>
/// AOT smoke tests for expression validation.
/// Verifies that <c>[Required]</c> attributes (preserved via ILLink.Descriptors.xml)
/// and <see cref="System.ComponentModel.DataAnnotations.IValidatableObject"/> validation
/// work correctly in a trimmed/AOT environment.
/// </summary>
public class ValidationTests
{
    private DefaultMigrationExpressionValidator _validator = null!;

    [Before(Test)]
    public void SetUp()
    {
        _validator = new DefaultMigrationExpressionValidator(serviceProvider: null);
    }

    [Test]
    [DisplayName("[Required] attribute validation works in AOT")]
    public async Task RequiredAttributeValidationCatchesMissingTableName()
    {
        var expression = new CreateTableExpression { TableName = null! };
        expression.Columns.Add(new ColumnDefinition { Name = "Col1", Type = DbType.String });

        var results = _validator.Validate(expression).ToList();

        await Assert.That(results).IsNotEmpty();
    }

    [Test]
    [DisplayName("IValidatableObject validation works in AOT")]
    public async Task ValidatableObjectCatchesMissingColumns()
    {
        var expression = new CreateIndexExpression
        {
            Index = new IndexDefinition
            {
                Name = "IX_Test",
                TableName = "TestTable"
            }
        };

        var results = _validator.Validate(expression).ToList();

        await Assert.That(results).IsNotEmpty();
    }

    [Test]
    [DisplayName("Valid expression passes validation in AOT")]
    public async Task ValidExpressionReturnsNoErrors()
    {
        var expression = new CreateTableExpression { TableName = "Users" };
        expression.Columns.Add(new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsPrimaryKey = true });
        expression.Columns.Add(new ColumnDefinition { Name = "Name", Type = DbType.String });

        var results = _validator.Validate(expression).ToList();

        await Assert.That(results).IsEmpty();
    }
}
