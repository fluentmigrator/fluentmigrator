#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

using System.Linq;

using FluentMigrator.Infrastructure;

namespace FluentMigrator
{
    /// <summary>
    /// A migration base class that will automatically generate the down expressions
    /// </summary>
    /// <remarks>
    /// This only works for some expressions like CREATE TABLE, but not for DROP TABLE.
    /// </remarks>
    public abstract class AutoReversingMigration : MigrationBase
    {
        /// <inheritdoc />
        public sealed override void Down()
        {
        }

        /// <inheritdoc />
        public override void GetDownExpressions(IMigrationContext context)
        {
            GetUpExpressions(context);
            context.Expressions = context.Expressions.Select(e => e.Reverse()).Reverse().ToList();
        }
    }
}
