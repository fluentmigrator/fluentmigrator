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
using System.Runtime.Serialization;

using FluentMigrator.Exceptions;

namespace FluentMigrator.Runner.Exceptions
{
    /// <summary>
    /// Represents the base exception class for exceptions that occur during the execution of FluentMigrator runners.
    /// </summary>
    /// <remarks>
    /// This exception serves as a specialized base class for exceptions that are specific to the FluentMigrator runner.
    /// It extends the <see cref="FluentMigrator.Exceptions.FluentMigratorException"/> class and provides constructors
    /// for various exception scenarios, including serialization support.
    /// </remarks>
    public class RunnerException : FluentMigratorException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunnerException"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is protected and is intended to be used by derived classes.
        /// </remarks>
        protected RunnerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunnerException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <remarks>
        /// This constructor allows the creation of a <see cref="RunnerException"/> instance with a custom error message,
        /// providing additional context about the error encountered during the execution of a FluentMigrator runner.
        /// </remarks>
        protected RunnerException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunnerException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        /// <remarks>
        /// This constructor is used to create an exception instance with a custom error message and an inner exception
        /// to provide additional context about the cause of the error.
        /// </remarks>
        protected RunnerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunnerException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <remarks>
        /// This constructor is used during deserialization to reconstitute the exception object transmitted over a stream.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is <c>null</c>.</exception>
        /// <exception cref="SerializationException">Thrown when the class name is <c>null</c> or <see cref="Exception.HResult"/> is zero.</exception>
#if NET
        [Obsolete("Formatter-based serialization is obsolete")]
#endif
        protected RunnerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
