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

namespace FluentMigrator
{
    /// <summary>
    /// Insert text with unicode data. Primarily for Sql Server, it prefixes the string with 'N
    /// </summary>
    [Obsolete("Use normal CLR strings instead, as they will be formatted to SQL Server Unicode strings")]
    public class ExplicitUnicodeString
    {
        /// <summary>
        /// Gets the text of the unicode string literal
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Insert text with unicode data. Primarily for Sql Server, it prefixes the string with 'N
        /// </summary>
        /// <param name="text">Unicode string</param>
        public ExplicitUnicodeString(string text)
        {
            Text = text;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Text;
        }
    }
}
