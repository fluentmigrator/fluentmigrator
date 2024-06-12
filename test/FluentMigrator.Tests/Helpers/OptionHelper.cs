using System.Linq;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Tests.Helpers
{
    internal static class OptionHelper
    {
        internal static IOptionsSnapshot<T> Get<T>() where T : class => new OptionsManager<T>(
            new OptionsFactory<T>(
                Enumerable.Empty<IConfigureOptions<T>>(),
                Enumerable.Empty<IPostConfigureOptions<T>>()));
    }
}
