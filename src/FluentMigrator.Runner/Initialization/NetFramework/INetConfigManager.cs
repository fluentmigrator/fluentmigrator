#if NETFRAMEWORK
using System.Configuration;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Initialization.NetFramework
{
    /// <summary>
    /// Understand .NET config mechanism and provides access to Configuration sections
    /// </summary>
    public interface INetConfigManager
    {
        [NotNull]
        Configuration LoadFromFile(string path);

        [NotNull]
        Configuration LoadFromMachineConfiguration();
    }
}
#endif
