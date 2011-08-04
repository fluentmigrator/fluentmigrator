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
		private string configPath;

		public NetConfigManager(string configPath)
		{
			ConfigPath = configPath;
		}

		public string ConfigPath
		{
			get
			{
				if (!string.IsNullOrEmpty(configPath))
					return configPath;
				else
					// returning null will force the usage of the machine config that is currently in memory
					return null;
			}
			set { configPath = value; }
		}

		public string GetConnectionString(string named)
		{
			var path = ConfigPath;
			if (path != null)
			{
				if (!File.Exists(path))
					throw new FileNotFoundException("The config file specified could not be found", path);

				Console.WriteLine(string.Format("Using config '{0}'", path));
				var c = new XmlDocument();
				c.Load(path);
				var xpath = string.Format("//connectionStrings/add[@name='{0}']/@connectionString", named);
				var attr = c.SelectSingleNode(xpath) as XmlAttribute;

				if (attr != null && !string.IsNullOrEmpty(attr.Value))
					return attr.Value;
				else throw new ArgumentException("Could not find connection string named by " + named);
			}
			else
			{
				var connection = ConfigurationManager.ConnectionStrings[named];
				return GetConnectionString(named, connection);
			} 
			
		}

		private static string GetConnectionString(string named, ConnectionStringSettings connection)
		{
			if (connection != null && !string.IsNullOrEmpty(connection.ConnectionString))
				return connection.ConnectionString;
			else throw new ArgumentException("Could not find connection string named by " + named);
		}
	}
}
