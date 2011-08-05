using System;
using System.Configuration;
using System.IO;
using System.Xml;

namespace FluentMigrator.Runner.Initialization
{
	/// <summary>
	/// Understands the surrounding environment and delivers the proper environment settings
	/// </summary>
	internal class NetConfigManager
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
			return GetConnectionString(named, connection);
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
			else throw new ArgumentException("Could not find connection string named by " + named);
		}
	}
}
