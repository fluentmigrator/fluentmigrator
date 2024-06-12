using Microsoft.Extensions.Logging;

namespace FluentMigrator.Tests.Helpers
{
    internal class LoggerHelper
    {
        internal static ILogger<T> Get<T>() => new Logger<T>(new LoggerFactory());
    }
}
