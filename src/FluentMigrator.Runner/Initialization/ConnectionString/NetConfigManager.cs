using System;
using System.Configuration;
using System.IO;
using System.Xml;

namespace FluentMigrator.Runner.Initialization.ConnectionString
{
    /// <summary>
    /// Understands the surrounding environment and delivers the proper environment settings
    /// </summary>
    public class NetConfigManager
    {

        public NetConfigManager(string configPath, string targetAssemblyPath)
        {
            ConfigPath = configPath;
            AssemblyPath = targetAssemblyPath;
        }

        private string ConfigPath { get; set; }
        private string AssemblyPath { get; set; }

        public string GetConnectionString(string named)
        {
            if (!string.IsNullOrEmpty(ConfigPath))
            {
                if (!File.Exists(ConfigPath))
                    throw new FileNotFoundException("The config file specified could not be found", ConfigPath);

                var cs = GetConnectionStringFromPath(named, ConfigPath);
                if (cs != null)
                    return cs;
                else throw new ArgumentException("Couldn't find connection string in app config even though it was specified");
            }

            if (!string.IsNullOrEmpty(AssemblyPath))
            {
                var conf = ConfigurationManager.OpenExeConfiguration(AssemblyPath);
                if (conf != null && conf.ConnectionStrings != null && conf.ConnectionStrings.ConnectionStrings != null)
                {
                    var cs = GetConnectionString(named, conf.ConnectionStrings.ConnectionStrings[named]);
                    if (cs != null)
                        return cs;
                }
            }

            // if all above failed, just use .NET's native mechanism, which includes `machine.config`
            var connection = ConfigurationManager.ConnectionStrings[named];
            var ret = GetConnectionString(named, connection);

            if (ret == null)
                throw new ArgumentException("Couldn't find the connection string in app.config or machine.config. Try specifying a path to connection string explicitly");

            return ret;

            //TODO Cambiar esto por algo más testable, hace falta un configsource
            //El problema es que el ConnectionStrings solo se podría testar en integración con la dificultad de adecuar el entorno para ello
        }

        private static string GetConnectionStringFromPath(string named, string path)
        {
            if (!File.Exists(path))
                return null;

            Console.WriteLine(string.Format("Using config '{0}'", path));
            var c = new XmlDocument();
            c.Load(path);
            var xpath = string.Format("//connectionStrings/add[@name='{0}']/@connectionString", named);
            var attr = c.SelectSingleNode(xpath) as XmlAttribute;

            return (attr != null && !string.IsNullOrEmpty(attr.Value)) ? attr.Value : null;
        }

        private static string GetConnectionString(string named, ConnectionStringSettings connection)
        {
            if (connection != null && !string.IsNullOrEmpty(connection.ConnectionString))
                return connection.ConnectionString;
            else return null;
        }
    }
}
