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
		private string version;

		public NetConfigManager(string version)
		{
			this.version = version;
		}

		internal string ConfigPath
		{
			get
			{
				if (version == null)
					return null;
				else
				{
					bool is64bit = IntPtr.Size == 8;
					string _64 = is64bit? "64" : "";
					string windir = Environment.GetEnvironmentVariable("windir");
					switch(version)
					{
						case "2":
						case "2.0":
						case "2.0.50727":
							return NullifyIfNotExists(string.Format(@"{1}\Microsoft.NET\Framework{0}\v2.0.50727\CONFIG\machine.config", _64, windir));

						case "3":
						case "3.0":
							return NullifyIfNotExists(string.Format(@"{1}\Microsoft.NET\Framework{0}\v3.0\CONFIG\machine.config", _64, windir));

						case "3.5":
							return NullifyIfNotExists(string.Format(@"{1}\Microsoft.NET\Framework{0}\v3.5\CONFIG\machine.config", _64, windir));

						case "4":
						case "4.0":
						case "4.0.30319":
							return NullifyIfNotExists(string.Format(@"{1}\Microsoft.NET\Framework{0}\v4.0.30319\CONFIG\machine.config", _64, windir));

						default:
							throw new ArgumentException(string.Format("Couldn't deceipher .NET version {0}", version));
					}
				}
			}
		}

		private string NullifyIfNotExists(string path)
		{
			if (File.Exists(path))
				return path;
			else return null;
		}

		public string GetConnectionString(string named)
		{
			var path = ConfigPath;
			if (path != null)
			{
				Console.WriteLine("Using config " + path);
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
