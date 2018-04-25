#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System.Collections.Generic;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.Runner.VersionTableInfo;

namespace FluentMigrator.Tests.Unit
{
    public class TestVersionLoader : IVersionLoader
    {
        private readonly IVersionTableMetaData _versionTableMetaData;

        public TestVersionLoader(IMigrationRunner runner, IVersionTableMetaData versionTableMetaData)
        {
            _versionTableMetaData = versionTableMetaData;
            Runner = runner;
            VersionInfo = new VersionInfo();
            Versions = new List<long>();
        }

        public bool AlreadyCreatedVersionSchema { get; set; }

        public bool AlreadyCreatedVersionTable { get; set; }

        public void DeleteVersion(long version)
        {
            Versions.Remove(version);
        }

        public IVersionTableMetaData GetVersionTableMetaData()
        {
            return _versionTableMetaData;
        }

        public void LoadVersionInfo()
        {
            VersionInfo = new VersionInfo();

            foreach (var version in Versions)
            {
                VersionInfo.AddAppliedMigration(version);
            }

            DidLoadVersionInfoGetCalled = true;
        }

        public bool DidLoadVersionInfoGetCalled { get; private set; }

        public void RemoveVersionTable()
        {
            DidRemoveVersionTableGetCalled = true;
        }

        public bool DidRemoveVersionTableGetCalled { get; private set; }

        public IMigrationRunner Runner { get; set; }

        public void UpdateVersionInfo(long version)
        {
            UpdateVersionInfo(version, null);
        }

        public void UpdateVersionInfo(long version, string description)
        {
            Versions.Add(version);

            DidUpdateVersionInfoGetCalled = true;
        }

        public bool DidUpdateVersionInfoGetCalled { get; private set; }

        public IVersionInfo VersionInfo { get; set; }

        public IVersionTableMetaData VersionTableMetaData => _versionTableMetaData;

        public List<long> Versions { get; private set; }
    }
}
