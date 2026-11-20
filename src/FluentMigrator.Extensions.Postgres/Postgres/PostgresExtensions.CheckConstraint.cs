#region License
// Copyright (c) 2026, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using FluentMigrator.Builder.CheckConstraint;
using FluentMigrator.Builders.Create;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Postgres
{
    public static partial class PostgresExtensions
    {
        /// <summary>
        /// Starts building a named PostgreSQL CHECK constraint for an existing table.
        /// </summary>
        /// <param name="root">The create expression root.</param>
        /// <param name="constraintName">The constraint name without SQL quoting.</param>
        /// <returns>The next step in the fluent syntax.</returns>
        public static ICreateCheckConstraintOnTableSyntax CheckConstraint(this ICreateExpressionRoot root, string constraintName)
        {
            var context = ((IMigrationContextAccessor)root).GetMigrationContext();
            return new CreateCheckConstraintExpressionBuilder(context, constraintName);
        }
    }
}
