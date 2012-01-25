using System;
using System.Runtime.Serialization;

namespace FluentMigrator.Runner.Generators
{
    /// <summary>The exception thrown when a SQL command is not supported by the underlying database.</summary>
    public class DatabaseOperationNotSupportedException : Exception
    {
        /// <summary>Initialize a new <see cref="DatabaseOperationNotSupportedException"/> instance.</summary>
        /// <param name="message">A message describing the incompatibility.</param>
        public DatabaseOperationNotSupportedException(string message) : base(message) { }

        /// <summary>Initialize a new <see cref="DatabaseOperationNotSupportedException"/> instance.</summary>
        /// <param name="message">A message describing the incompatibility.</param>
        /// <param name="innerException">The exception that is the cause of this exception.</param>
        public DatabaseOperationNotSupportedException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>Initialize a new <see cref="DatabaseOperationNotSupportedException"/> instance.</summary>
        public DatabaseOperationNotSupportedException() : base() { }

        /// <summary>Initialize a new <see cref="DatabaseOperationNotSupportedException"/> instance.</summary>
        public DatabaseOperationNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}