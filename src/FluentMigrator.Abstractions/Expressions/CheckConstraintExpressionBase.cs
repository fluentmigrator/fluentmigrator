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
using System.ComponentModel.DataAnnotations;

using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions;

/// <summary>A migration operation on a named table CHECK constraint.</summary>
public abstract class CheckConstraintExpressionBase : MigrationExpressionBase, ISchemaExpression, IValidatableObject
{
    /// <summary>The target constraint and its options.</summary>
    public CheckConstraintDefinition Constraint { get; set; } = new CheckConstraintDefinition();

    string ISchemaExpression.SchemaName
    {
        get => Constraint.SchemaName;
        set => Constraint.SchemaName = value;
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Constraint == null || string.IsNullOrWhiteSpace(Constraint.Name))
        {
            yield return new ValidationResult("A CHECK constraint requires a name.");
        }

        if (Constraint == null || string.IsNullOrWhiteSpace(Constraint.TableName))
        {
            yield return new ValidationResult(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        if (Constraint != null)
        {
            foreach (var error in ValidateOperation())
            {
                yield return error;
            }
        }
    }

    /// <summary>Validates the action and its options.</summary>
    protected virtual IEnumerable<ValidationResult> ValidateOperation()
    {
        yield break;
    }

    /// <inheritdoc />
    public override string ToString() => base.ToString() + Constraint?.Name;
}
