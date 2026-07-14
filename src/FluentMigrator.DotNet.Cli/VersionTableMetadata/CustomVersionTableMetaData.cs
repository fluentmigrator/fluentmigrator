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

using FluentMigrator.Runner.VersionTableInfo;

namespace FluentMigrator.DotNet.Cli.VersionTableMetadata;

public class CustomVersionTableMetaData : IVersionTableMetaData
{
    public bool OwnsSchema { get; set; }

    public string SchemaName { get; set; }

    public string TableName { get; set; }

    public string ColumnName { get; set; }

    public string DescriptionColumnName { get; set; }

    public string UniqueIndexName { get; set; }

    public string AppliedOnColumnName { get; set; }

    public bool CreateWithPrimaryKey { get; set; }
}
