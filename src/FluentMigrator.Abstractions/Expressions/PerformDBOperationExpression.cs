#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using System.ComponentModel.DataAnnotations;
using System.Data;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression that allows the execution of DB operations
    /// </summary>
    public class PerformDBOperationExpression : MigrationExpressionBase
    {
        /// <summary>
        /// Gets or sets the operation to be executed for a given database connection
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.OperationCannotBeNull))]
        public Action<IDbConnection, IDbTransaction> Operation { get; set; }

        /// <summary>
        /// Gets or sets the description of the operation for logging purposes
        /// </summary>
        public string Description { get; set; }

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }
    }
}
