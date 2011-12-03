using System;

namespace FluentMigrator.Runner.Initialization
{
    public class ProcessorFactoryNotFoundException: ApplicationException
    {
         public ProcessorFactoryNotFoundException(string message): base(message) {}
    }
}