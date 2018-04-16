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

using System;
using System.Collections.Generic;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// The base class for migration expressions
    /// </summary>
    public abstract class MigrationExpressionBase : IMigrationExpression
    {
        /// <inheritdoc />
        public abstract void ExecuteWith(IMigrationProcessor processor);

        /// <inheritdoc />
        public abstract void CollectValidationErrors(ICollection<string> errors);

        /// <inheritdoc />
        public virtual IMigrationExpression Reverse()
        {
            throw new NotSupportedException(String.Format("The {0} cannot be automatically reversed", GetType().Name));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return GetType().Name.Replace("Expression", "") + " ";
        }
    }
}
