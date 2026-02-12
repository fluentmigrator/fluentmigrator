#region License
// Copyright (c) 2024, Fluent Migrator Project
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

using System.Text.Json;

using FluentMigrator.Runner;

namespace FluentMigrator.DotNet.Cli.VersionTableMetadata
{
    public static class Extensions
    {
        public static IMigrationRunnerBuilder AddCustomVersionTableInfo(
            this IMigrationRunnerBuilder builder,
            string versionInfoTableMetadataJson)
        {
            if (!string.IsNullOrWhiteSpace(versionInfoTableMetadataJson))
            {
                var customVersionInfoTableMetadata =
                    JsonSerializer.Deserialize<CustomVersionTableMetaData>(versionInfoTableMetadataJson);
                builder.WithVersionTable(customVersionInfoTableMetadata);
            }
            return builder;
        }
    }
}
