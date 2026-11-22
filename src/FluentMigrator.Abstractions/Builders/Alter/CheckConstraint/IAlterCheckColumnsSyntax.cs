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

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Alter.CheckConstraint;

/// <summary>Specifies the columns used to name the CHECK constraint.</summary>
public interface IAlterCheckColumnsSyntax : IFluentSyntax
{
    /// <summary>Names the constraint using one column.</summary>
    IAlterCheckActionSyntax Column(string columnName);

    /// <summary>Names the constraint using the columns in the supplied order.</summary>
    IAlterCheckActionSyntax Columns(params string[] columnNames);
}
