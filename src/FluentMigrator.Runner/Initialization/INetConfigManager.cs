using System.Configuration;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Understand .NET config mechanism and provides access to Configuration sections
    /// </summary>
    public interface INetConfigManager
    {
        Configuration LoadFromFile(string path);

        Configuration LoadFromMachineConfiguration();
    }
}
