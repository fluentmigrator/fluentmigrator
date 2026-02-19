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
        /// <summary>
        /// Loads a .NET configuration file from the specified path.
        /// </summary>
        /// <param name="path">The path to the configuration file.</param>
        /// <returns>The loaded <see cref="System.Configuration.Configuration"/> object.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the specified <paramref name="path"/> is null, empty, or does not exist.
        /// </exception>
        /// <remarks>
        /// If the provided path does not have a ".config" extension, it will be appended automatically.
        /// </remarks>
        [NotNull]
        Configuration LoadFromFile(string path);

        /// <summary>
        /// Loads the machine-wide configuration file (Machine.config) for the current .NET Framework installation.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Configuration.Configuration"/> object representing the machine-wide configuration settings.
        /// </returns>
        /// <remarks>
        /// This method provides access to the global configuration settings defined in the Machine.config file.
        /// It is typically used when application-specific configuration files do not contain the required settings.
        /// </remarks>
        [NotNull]
        Configuration LoadFromMachineConfiguration();
    }
}
#endif
