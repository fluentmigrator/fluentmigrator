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

using FluentMigrator.Exceptions;

namespace FluentMigrator.Runner.Generators
{
    /// <summary>
    /// Extension methods for <see cref="CompatibilityMode"/>
    /// </summary>
    public static class CompatibilityModeExtension
    {
        /// <summary>
        /// Handles unsupported generator features according to a compatibility mode.
        /// </summary>
        /// <param name="mode">The compatibility mode</param>
        /// <param name="message">The exception message (if any gets thrown)</param>
        /// <returns>The string to be returned (if no exception was thrown)</returns>
        /// <exception cref="DatabaseOperationNotSupportedException">The exception to be thrown</exception>
        public static string HandleCompatibility(this CompatibilityMode mode, string message)
        {
            if (CompatibilityMode.STRICT == mode)
            {
                throw new DatabaseOperationNotSupportedException(message);
            }

            return string.Empty;
        }
    }
}
