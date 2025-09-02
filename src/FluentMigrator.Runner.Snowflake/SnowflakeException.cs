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

using FluentMigrator.Exceptions;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Represents an exception specific to Snowflake operations within FluentMigrator.
    /// </summary>
    /// <remarks>
    /// This exception is thrown when an error occurs during the execution of Snowflake-related SQL operations.
    /// </remarks>
    public class SnowflakeException : FluentMigratorException
    {
        /// <inheritdoc />
        public SnowflakeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
