#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using FluentMigrator.Expressions;

using JetBrains.Annotations;

namespace FluentMigrator.Validation
{
    /// <summary>
    /// Interface for a migration expression validator
    /// </summary>
    public interface IMigrationExpressionValidator
    {
        /// <summary>
        /// Validates the given migration expression
        /// </summary>
        /// <param name="expression">The migration expression to validate</param>
        /// <returns>The validation results</returns>
        [NotNull, ItemNotNull]
        IEnumerable<ValidationResult> Validate([NotNull] IMigrationExpression expression);
    }
}
