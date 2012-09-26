using System;
using System.Runtime.Serialization;

namespace FluentMigrator.Runner.Initialization
{
    [Serializable]
    public class UndeterminableConnectionException : ApplicationException
    {
        public UndeterminableConnectionException() { }

        public UndeterminableConnectionException(string message) : base(message) { }

        public UndeterminableConnectionException(string message, Exception innerException) : base(message, innerException) { }

        protected UndeterminableConnectionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}