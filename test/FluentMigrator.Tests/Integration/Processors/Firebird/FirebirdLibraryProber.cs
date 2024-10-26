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
using System.Linq;

using FirebirdSql.Data.FirebirdClient;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    public class FirebirdLibraryProber
    {
        private readonly object _sync = new object();
        private readonly IReadOnlyCollection<string> _clientLibraries;
        private Exception _probeException;
        private bool _foundClientLibrary;
        private string _clientLibrary;

        public FirebirdLibraryProber()
            : this(DefaultLibraries)
        {
        }

        public FirebirdLibraryProber(IEnumerable<string> clientLibraries)
        {
            _clientLibraries = clientLibraries.ToList();
        }

        public static IEnumerable<string> DefaultLibraries
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    yield return "libfbclient.dylib";
                    yield return "fbclient";
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    // Try V3.0 first
                    yield return "/opt/firebird/lib/libfbclient.so.3";
                    yield return "/opt/firebird/lib/libfbclient.so";
                    yield return "libfbclient.so.3.0";
                    yield return "libfbclient.so.3";

                    // Now probe for 2.5
                    yield return "/usr/lib/x86_64-linux-gnu/libfbembed.so.2.5";
                    yield return "/usr/lib/x86_64-linux-gnu/libfbclient.so.2";
                    yield return "libfbembed.so.2.5";
                    yield return "fbembed";
                    yield return "libfbclient.so.2";

                    // Last chance
                    yield return "fbclient";
                }
                else
                {
                    // Windows-style OS?
                    yield return "fbembed.dll";
                    yield return "gds.dll";
                    yield return "fbclient.dll";
                }
            }
        }

        public string CreateDb(FbConnectionStringBuilder csb)
        {
            if (!_foundClientLibrary)
            {
                lock (_sync)
                {
                    if (!_foundClientLibrary)
                    {
                        var clientLibs = new List<string>();
                        if (!string.IsNullOrEmpty(csb.ClientLibrary))
                        {
                            clientLibs.Add(csb.ClientLibrary);
                        }

                        clientLibs.AddRange(_clientLibraries);

                        foreach (var clientLibrary in clientLibs)
                        {
                            csb.ClientLibrary = clientLibrary;
                            try
                            {
                                FbConnection.CreateDatabase(csb.ConnectionString, overwrite: true);
                                _clientLibrary = clientLibrary;
                                _foundClientLibrary = true;
                                return csb.ConnectionString;
                            }
                            catch (Exception ex)
                            {
                                _probeException = ex;
                            }
                        }

                        _foundClientLibrary = true;
                    }
                }
            }

            if (_clientLibrary != null)
            {
                csb.ClientLibrary = _clientLibrary;
                FbConnection.CreateDatabase(csb.ConnectionString, overwrite: true);
                return csb.ConnectionString;
            }

            throw new Exception("Failed to find suitable client library", _probeException);
        }
    }
}
