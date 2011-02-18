
namespace FluentMigrator.Runner.Generators
{
    using System;
    using System.Runtime.Serialization;


    public class DatabaseOperationNotSupportedExecption : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OperationNotSupportedExecption"/> class.
        /// </summary>
        public DatabaseOperationNotSupportedExecption(string message) : base(message) { }


        public DatabaseOperationNotSupportedExecption(string message, Exception innerException) : base(message, innerException) { }

        public DatabaseOperationNotSupportedExecption() : base() { }

        public DatabaseOperationNotSupportedExecption(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
