using System;
using System.Runtime.Serialization;

namespace FluentMigrator.Runner.Generators
{
    public class DatabaseOperationNotSupportedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OperationNotSupportedException"/> class.
        /// </summary>
        public DatabaseOperationNotSupportedException(string message) : base(message) { }

        public DatabaseOperationNotSupportedException(string message, Exception innerException) : base(message, innerException) { }

        public DatabaseOperationNotSupportedException() : base() { }

        public DatabaseOperationNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}