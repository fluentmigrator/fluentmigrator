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

        public static DatabaseServerOptions SqlServer2005 => DatabaseServers["SqlServer2005"];

        public static DatabaseServerOptions SqlServer2008 => DatabaseServers["SqlServer2008"];

        public static DatabaseServerOptions SqlServer2012 => DatabaseServers["SqlServer2012"];

        public static DatabaseServerOptions SqlServer2014 => DatabaseServers["SqlServer2014"];

        public static DatabaseServerOptions SqlServerCe => DatabaseServers["SqlServerCe"];

        public static DatabaseServerOptions Jet => DatabaseServers["Jet"];

        public static DatabaseServerOptions SqlLite => DatabaseServers["SQLite"];

        public static DatabaseServerOptions MySql => DatabaseServers["MySql"];

        public static DatabaseServerOptions Postgres => DatabaseServers["Postgres"];

        public static DatabaseServerOptions Firebird => DatabaseServers["Firebird"];

        public static DatabaseServerOptions Oracle => DatabaseServers["Oracle"];

        public static DatabaseServerOptions Db2 => DatabaseServers["Db2"];

        public static DatabaseServerOptions Hana => DatabaseServers["Hana"];

        public static bool AnyServerTypesEnabled
            => Db2.IsEnabled
             || Firebird.IsEnabled
             || Hana.IsEnabled
             || Jet.IsEnabled
             || MySql.IsEnabled
             || Oracle.IsEnabled
             || Postgres.IsEnabled
             || SqlLite.IsEnabled
             || SqlServer2005.IsEnabled
             || SqlServer2008.IsEnabled
             || SqlServer2012.IsEnabled
             || SqlServer2014.IsEnabled
             || SqlServerCe.IsEnabled;

        public class DatabaseServerOptions
        {
            public string ConnectionString { get; set; }
            public bool IsEnabled { get; set; } = true;
        }
    }
}
