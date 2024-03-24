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

namespace FluentMigrator
{
    /// <summary>
    /// An explicitly non-Unicode string literal ('some string' in T-SQL)
    /// </summary>
    public class NonUnicodeString
    {
        /// <summary>
        /// Gets the value of the non-Unicode string literal
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Used for explicitly creating a non-Unicode string literal in Transact SQL
        /// </summary>
        /// <param name="value">The value of the non-Unicode string literal</param>
        public NonUnicodeString(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Overrides ToString() to return the value.
        /// </summary>
        /// <returns>
        /// The value of the non-Unicode string literal.
        /// </returns>
        public override string ToString()
        {
            return Value;
        }
    }
}
