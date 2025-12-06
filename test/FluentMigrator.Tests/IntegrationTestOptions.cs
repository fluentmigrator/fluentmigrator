#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Configuration;

using NUnit.Framework;

namespace FluentMigrator.Tests
{
    public static class IntegrationTestOptions
    {
        private static readonly ISet<string> _platformIdentifiers;

        static IntegrationTestOptions()
        {
            var asmPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var config = new ConfigurationBuilder()
                .SetBasePath(asmPath)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets("FluentMigrator.Tests")
                // You can specify env vars to override appsettings values, to enable local containers for example:
                //  * FM_TestConnectionStrings__SqlServer2016__IsEnabled=true
                //  * FM_TestConnectionStrings__SqlServer2016__ContainerEnabled=true
                .AddEnvironmentVariables("FM_")
                .Build();

            DatabaseServers = config
                .GetSection("TestConnectionStrings")
                .Get<IReadOnlyDictionary<string, DatabaseServerOptions>>();

            // Set the name for each DatabaseServers entries
            foreach (var pair in DatabaseServers)
            {
                pair.Value.Name = pair.Key;
            }

            if (Environment.Is64BitProcess)
            {
                _platformIdentifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "amd64", "x64", "x86-64"
                };
            }
            else
            {
                _platformIdentifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "x86", "x86-32", "x32"
                };
            }
        }

        private static IReadOnlyDictionary<string, DatabaseServerOptions> DatabaseServers { get;}

        public static DatabaseServerOptions SqlServer2005 => GetOptions(ProcessorIdConstants.SqlServer2005);

        public static DatabaseServerOptions SqlServer2008 => GetOptions(ProcessorIdConstants.SqlServer2008);

        public static DatabaseServerOptions SqlServer2012 => GetOptions(ProcessorIdConstants.SqlServer2012);

        public static DatabaseServerOptions SqlServer2014 => GetOptions(ProcessorIdConstants.SqlServer2014);

        public static DatabaseServerOptions SqlServer2016 => GetOptions(ProcessorIdConstants.SqlServer2016);

        public static DatabaseServerOptions Jet => GetOptions(ProcessorIdConstants.Jet);

        // ReSharper disable once InconsistentNaming
        public static DatabaseServerOptions SQLite => GetOptions(ProcessorIdConstants.SQLite).ReplaceConnectionStringDataDirectory();

        public static DatabaseServerOptions MySql => GetOptions(ProcessorIdConstants.MySql);

        public static DatabaseServerOptions Postgres => GetOptions(ProcessorIdConstants.Postgres);

        public static DatabaseServerOptions Firebird => GetOptions(ProcessorIdConstants.Firebird).GetOptionsForPlatform();

        public static DatabaseServerOptions Oracle => GetOptions(ProcessorIdConstants.Oracle);

        public static DatabaseServerOptions Db2 => Environment.Is64BitProcess ? GetOptions(ProcessorIdConstants.DB2) : DatabaseServerOptions.Empty;

        public static DatabaseServerOptions Db2ISeries => GetOptions(ProcessorIdConstants.Db2ISeries);

        public static DatabaseServerOptions Hana => Environment.Is64BitProcess ? GetOptions(ProcessorIdConstants.Hana) : DatabaseServerOptions.Empty;

        public static DatabaseServerOptions Snowflake => GetOptions("Snowflake");

        public class DatabaseServerOptions
        {
            private ISet<string> _supportedPlatforms;
            private string _supportedPlatformsValue;
            private string _originalConnectionString;

            public static DatabaseServerOptions Empty { get; } = new() { IsEnabled = false };

            [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Set by JSON serializer")]
            public string Name { get; set; }

            [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Set by JSON serializer")]
            public string ConnectionString { get; set; }

            [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Set by JSON serializer")]
            public bool IsEnabled { get; set; }

            [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Set by JSON serializer")]
            public bool ContainerEnabled { get; set; }

            [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Set by JSON serializer")]
            public string SupportedPlatforms
            {
                get => _supportedPlatformsValue;
                set
                {
                    _supportedPlatformsValue = value;
                    var items = value.Split(',', ';')
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim());
                    _supportedPlatforms = new HashSet<string>(items, StringComparer.OrdinalIgnoreCase);
                }
            }

            public DatabaseServerOptions GetOptionsForPlatform()
            {
                return GetOptionsForPlatform(_platformIdentifiers);
            }

            private DatabaseServerOptions GetOptionsForPlatform(ISet<string> platforms)
            {
                if (_supportedPlatforms == null || _supportedPlatforms.Count == 0 || !IsEnabled)
                    return this;
                if (_supportedPlatforms.Any(platforms.Contains))
                    return this;
                return Empty;
            }

            public DatabaseServerOptions ReplaceConnectionStringDataDirectory()
            {
                if (string.IsNullOrWhiteSpace(_originalConnectionString))
                    _originalConnectionString = ConnectionString;

                ConnectionString = _originalConnectionString
                    .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString());

                return this;
            }

            public void IgnoreIfNotEnabled()
            {
                if (!IsEnabled)
                {
                    Assert.Ignore($"{Name} is not enabled. To enable this test, go to appsettings.json and set TestConnectionStrings:{Name}:IsEnabled to true, or set environment variable FM_TestConnectionStrings__{Name}__IsEnabled=true.");
                }
            }
        }

        private static DatabaseServerOptions GetOptions(string key)
        {
            if (DatabaseServers.TryGetValue(key, out var options))
            {
                return options;
            }

            return DatabaseServerOptions.Empty;
        }
    }
}
