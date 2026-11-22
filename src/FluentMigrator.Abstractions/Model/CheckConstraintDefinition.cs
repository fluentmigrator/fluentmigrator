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

using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;

namespace FluentMigrator.Model;

/// <summary>A named CHECK constraint on a table.</summary>
public sealed class CheckConstraintDefinition : ISupportAdditionalFeatures
{
    /// <summary>The constraint name.</summary>
    public string ConstraintName { get; set; }

    /// <summary>The columns used to name the constraint, in order.</summary>
    public List<string> Columns { get; } = new List<string>();

    /// <summary>The table containing the constraint.</summary>
    public string TableName { get; set; }

    /// <summary>The schema containing the table.</summary>
    public string SchemaName { get; set; }

    /// <summary>The SQL predicate that rows must satisfy.</summary>
    public string Condition { get; set; }

    /// <summary>Whether to skip validation of existing rows.</summary>
    public bool NoCheck { get; set; }

    /// <summary>Whether the constraint is enforced when created.</summary>
    public bool Enabled { get; set; } = true;

    /// <inheritdoc />
    public IDictionary<string, object> AdditionalFeatures { get; } = new Dictionary<string, object>();

    /// <summary>Copies the constraint definition.</summary>
    public CheckConstraintDefinition Clone()
    {
        var clone = new CheckConstraintDefinition
        {
            ConstraintName = ConstraintName,
            TableName = TableName,
            SchemaName = SchemaName,
            Condition = Condition,
            NoCheck = NoCheck,
            Enabled = Enabled,
        };

        clone.Columns.AddRange(Columns);
        AdditionalFeatures.CloneTo(clone.AdditionalFeatures);
        return clone;
    }
}
