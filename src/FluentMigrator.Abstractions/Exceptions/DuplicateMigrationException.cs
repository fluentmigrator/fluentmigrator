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

using System;
using System.Runtime.Serialization;

namespace FluentMigrator.Exceptions
{
    /// <summary>
    /// An exception that is thrown when more than one migration with the same version was found
    /// </summary>
    [Serializable]
    public class DuplicateMigrationException : FluentMigratorException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateMigrationException"/> class.
        /// </summary>
        public DuplicateMigrationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateMigrationException"/> class.
        /// </summary>
        /// <param name="message">The exception message</param>
        public DuplicateMigrationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateMigrationException"/> class.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public DuplicateMigrationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateMigrationException"/> class.
        /// </summary>
        /// <param name="info">The serialization information</param>
        /// <param name="context">The streaming context</param>
#if NET
        [Obsolete("Formatter-based serialization is obsolete")]
#endif
        public DuplicateMigrationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
