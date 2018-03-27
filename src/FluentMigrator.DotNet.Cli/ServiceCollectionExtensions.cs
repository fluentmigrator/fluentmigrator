using FluentMigrator.Runner.Processors;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.DotNet.Cli
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProcessorFactory<T>(this IServiceCollection serviceCollection)
            where T : class, IMigrationProcessorFactory
        {
            serviceCollection.AddSingleton<IMigrationProcessorFactory, T>();
            return serviceCollection;
        }
    }
}
