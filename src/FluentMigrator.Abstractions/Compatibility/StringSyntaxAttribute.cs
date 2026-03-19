#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

#if !NET7_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Specifies the syntax used in a string.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class StringSyntaxAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringSyntaxAttribute"/> class
        /// with the identifier of the syntax used.
        /// </summary>
        /// <param name="syntax">The syntax identifier.</param>
        public StringSyntaxAttribute(string syntax)
        {
            Syntax = syntax;
            Arguments = Array.Empty<object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringSyntaxAttribute"/> class
        /// with the identifier of the syntax used.
        /// </summary>
        /// <param name="syntax">The syntax identifier.</param>
        /// <param name="arguments">Optional arguments associated with the specific syntax employed.</param>
        public StringSyntaxAttribute(string syntax, params object[] arguments)
        {
            Syntax = syntax;
            Arguments = arguments;
        }

        /// <summary>
        /// Gets the identifier of the syntax used.
        /// </summary>
        public string Syntax { get; }

        /// <summary>
        /// Gets optional arguments associated with the specific syntax employed.
        /// </summary>
        public object[] Arguments { get; }

        /// <summary>
        /// The syntax identifier for strings containing SQL.
        /// </summary>
        public const string Sql = "sql";
    }
}
#endif
