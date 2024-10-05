#region License
//
// Copyright (c) 2019, Fluent Migrator Project
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

namespace FluentMigrator.Runner.Constraints
{
    /// <summary>
    /// Can be used to apply conditions when migrations will be run.
    /// </summary>
    public class MigrationConstraintAttribute : Attribute
    {
        private readonly Func<MigrationConstraintContext, bool> _predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationConstraintAttribute"/> class.
        /// </summary>
        /// <param name="predicate">Predicate that determines whether this migration should be run in given context <see cref="MigrationConstraintAttribute"/>.</param>
        public MigrationConstraintAttribute(Func<MigrationConstraintContext, bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate), "Predicate must not be null");
        }
        /// <summary>
        /// Determines whether the migration having this attribute should be run under given <paramref name="context">migration context</paramref>.
        /// </summary>
        /// <param name="context">Contextual information that can be used to determine whether this migration should be run.</param>
        /// <returns>True if migration should be run under given <paramref name="context">migration context</paramref>.</returns>
        public bool ShouldRun(MigrationConstraintContext context)
        {
            return _predicate(context);
        }
    }
}
