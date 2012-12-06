using System;
using System.Runtime.Serialization;

namespace FluentMigrator.Runner.Generators
{
    [Serializable]
    public class DatabaseOperationNotSupportedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:DatabaseOperationNotSupportedException"/> class.
        /// </summary>
        public DatabaseOperationNotSupportedException(string message) : base(message) { }

        public DatabaseOperationNotSupportedException(string message, Exception innerException) : base(message, innerException) { }

        public DatabaseOperationNotSupportedException() : base() { }

        protected DatabaseOperationNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}