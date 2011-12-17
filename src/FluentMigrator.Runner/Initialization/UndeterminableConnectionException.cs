using System;

namespace FluentMigrator.Runner.Initialization
{
    public class UndeterminableConnectionException : ApplicationException
    {
        public UndeterminableConnectionException(string message) : base(message) { }
    }
}