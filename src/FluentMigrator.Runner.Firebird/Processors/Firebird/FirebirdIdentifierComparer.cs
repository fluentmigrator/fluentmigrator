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

using System;
using System.Collections.Generic;

using FluentMigrator.Generation;

namespace FluentMigrator.Runner.Processors.Firebird
{
    /// <summary>
    /// A custom <see cref="IEqualityComparer{T}"/> for Firebird identifiers
    /// </summary>
    public class FirebirdIdentifierComparer : IEqualityComparer<string>
    {
        private readonly IQuoter _quoter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebirdIdentifierComparer"/> class.
        /// </summary>
        /// <param name="quoter">The quoted used to unquote the quoted identifiers</param>
        public FirebirdIdentifierComparer(IQuoter quoter)
        {
            _quoter = quoter;
        }

        /// <inheritdoc />
        public bool Equals(string x, string y)
        {
            return string.Equals(NormalizeIdentifier(x), NormalizeIdentifier(y), StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public int GetHashCode(string obj)
        {
            return NormalizeIdentifier(obj).GetHashCode();
        }

        /// <summary>
        /// Normalizes the identifier the same way as Firebird
        /// </summary>
        /// <param name="identifier">The identifier to normalize</param>
        /// <returns>The normalized identifier</returns>
        private string NormalizeIdentifier(string identifier)
        {
            var result = _quoter.UnQuote(identifier);
            if (result != identifier)
                return result;

            return result.ToUpperInvariant();
        }
    }
}
