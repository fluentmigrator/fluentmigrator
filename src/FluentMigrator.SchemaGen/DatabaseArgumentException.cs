using System;
using System.Runtime.Serialization;

namespace FluentMigrator.SchemaGen
{
    [Serializable]
    public class DatabaseArgumentException : Exception
    {
        public DatabaseArgumentException()
        {
        }

        public DatabaseArgumentException(string message) : base(message)
        {
        }

        public DatabaseArgumentException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DatabaseArgumentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}