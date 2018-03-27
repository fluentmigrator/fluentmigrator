#if NET40 || NET45
using System.Configuration;

namespace FluentMigrator.Runner.Initialization.NetFramework
{
    /// <summary>
    /// Understand .NET config mechanism and provides access to Configuration sections
    /// </summary>
    internal interface INetConfigManager
    {
        Configuration LoadFromFile(string path);

        Configuration LoadFromMachineConfiguration();
    }
}
#endif
