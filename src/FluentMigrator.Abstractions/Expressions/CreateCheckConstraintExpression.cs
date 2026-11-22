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

using FluentMigrator.Model;

namespace FluentMigrator.Expressions;

/// <summary>Adds a named CHECK constraint to an existing table.</summary>
public sealed class CreateCheckConstraintExpression : CheckConstraintExpressionBase
{
    /// <inheritdoc />
    public override void ExecuteWith(IMigrationProcessor processor) => processor.Process(this);

    /// <inheritdoc />
    public override IMigrationExpression Reverse()
    {
        var identity = new CheckConstraintDefinition
        {
            Name = Constraint.Name,
            TableName = Constraint.TableName,
            SchemaName = Constraint.SchemaName
        };
        identity.Columns.AddRange(Constraint.Columns);
        if (string.IsNullOrEmpty(identity.Name))
        {
            // Auto-reversing migrations run conventions after reversal. A custom name may use the predicate.
            identity.Condition = Constraint.Condition;
        }

        return new DeleteCheckConstraintExpression { Constraint = identity };
    }

    /// <inheritdoc />
    protected override IEnumerable<ValidationResult> ValidateOperation()
    {
        if (string.IsNullOrWhiteSpace(Constraint.Condition))
        {
            yield return new ValidationResult("A CHECK constraint requires a SQL condition.");
        }
    }
}
