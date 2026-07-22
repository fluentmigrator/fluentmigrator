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
    /// The base exception class for FluentMigrator exceptions
    /// </summary>
    [Serializable]
    public abstract class FluentMigratorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigratorException"/> class.
        /// </summary>
        protected FluentMigratorException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigratorException"/> class.
        /// </summary>
        /// <param name="message">The exception message</param>
        protected FluentMigratorException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigratorException"/> class.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        protected FluentMigratorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigratorException"/> class.
        /// </summary>
        /// <param name="info">The serialization information</param>
        /// <param name="context">The streaming context</param>
#if NET
        [Obsolete("Formatter-based serialization is obsolete")]
#endif
        protected FluentMigratorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
