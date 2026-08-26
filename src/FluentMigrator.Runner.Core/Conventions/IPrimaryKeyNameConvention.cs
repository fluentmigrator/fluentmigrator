#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Conventions
{
    /// <summary>
    /// A convention that provides the default name of a primary key
    /// </summary>
    /// <remarks>
    /// The convention is applied both to primary keys declared as part of a column definition
    /// (<see cref="IColumnsExpression"/>, e.g. <c>Create.Table(...).WithColumn(...).PrimaryKey()</c>)
    /// and to primary keys declared as a standalone constraint
    /// (<see cref="IConstraintExpression"/>, e.g. <c>Create.PrimaryKey().OnTable(...)</c>),
    /// so that both produce the same constraint name.
    /// </remarks>
    public interface IPrimaryKeyNameConvention : IColumnsConvention, IConstraintConvention
    {
    }
}
