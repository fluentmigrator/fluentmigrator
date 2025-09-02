#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

namespace FluentMigrator.Runner.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when a migration contains validation errors.
    /// </summary>
    /// <remarks>
    /// This exception is used to indicate that a migration has failed validation due to one or more errors.
    /// The specific migration and the associated validation errors are included in the exception details.
    /// </remarks>
    public class InvalidMigrationException : RunnerException
    {
        private readonly IMigration _migration;
        private readonly string _errors;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMigrationException"/> class with the specified migration and validation errors.
        /// </summary>
        /// <param name="migration">The migration that caused the exception.</param>
        /// <param name="errors">The validation errors associated with the migration.</param>
        /// <remarks>
        /// This constructor is used to create an exception instance when a migration fails validation.
        /// The <paramref name="migration"/> parameter identifies the specific migration, and the <paramref name="errors"/> parameter
        /// provides details about the validation errors that occurred.
        /// </remarks>
        public InvalidMigrationException(IMigration migration, string errors)
        {
            _migration = migration;
            _errors = errors;
        }

        /// <inheritdoc />
        public override string Message =>
            $"The migration {_migration.GetType().Name} contained the following Validation Error(s): {_errors}";
    }
}
