#region License
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Microsoft.Extensions.Configuration;

namespace FluentMigrator.Tests
{
    public static class IntegrationTestOptions
    {
        static IntegrationTestOptions()
        {
            var asmPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var config = new ConfigurationBuilder()
                .SetBasePath(asmPath)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets("FluentMigrator.Tests")
                .Build();
            Configuration = config;
            DatabaseServers = config
                .GetSection("TestConnectionStrings")
                .Get<IReadOnlyDictionary<string, DatabaseServerOptions>>();
        }

        public static IConfigurationRoot Configuration { get; }

        public static IReadOnlyDictionary<string, DatabaseServerOptions> DatabaseServers { get;}

        public static DatabaseServerOptions SqlServer2005 => GetOptions("SqlServer2005");

        public static DatabaseServerOptions SqlServer2008 => GetOptions("SqlServer2008");

        public static DatabaseServerOptions SqlServer2012 => GetOptions("SqlServer2012");

        public static DatabaseServerOptions SqlServer2014 => GetOptions("SqlServer2014");

        public static DatabaseServerOptions SqlServer2016 => GetOptions("SqlServer2016");

        public static DatabaseServerOptions SqlServerCe => GetOptions("SqlServerCe");

        public static DatabaseServerOptions SqlAnywhere16 => GetOptions("SqlAnywhere16");

        public static DatabaseServerOptions Jet => GetOptions("Jet");

        public static DatabaseServerOptions SqlLite => GetOptions("SQLite");

        public static DatabaseServerOptions MySql => GetOptions("MySql");

        public static DatabaseServerOptions Postgres => GetOptions("Postgres");

        public static DatabaseServerOptions Firebird => GetOptions("Firebird");

        public static DatabaseServerOptions Oracle => GetOptions("Oracle");

        public static DatabaseServerOptions Db2 => GetOptions("Db2");

        public static DatabaseServerOptions Hana => GetOptions("Hana");

        public class DatabaseServerOptions
        {
            public static DatabaseServerOptions Empty { get; } = new DatabaseServerOptions() { IsEnabled = false };

            public string ConnectionString { get; set; }
            public bool IsEnabled { get; set; } = true;
        }

        private static DatabaseServerOptions GetOptions(string key)
        {
            if (DatabaseServers.TryGetValue(key, out var options))
                return options;
            return DatabaseServerOptions.Empty;
        }
    }
}
