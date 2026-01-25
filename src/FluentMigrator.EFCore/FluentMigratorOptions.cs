#region License
// Copyright (c) 2026, Fluent Migrator Project
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

namespace FluentMigrator.EFCore;

public class FluentMigratorOptions
{
    /// <summary>
    /// The base migration class to inherit from. Default: "Migration"
    /// </summary>
    public string BaseMigrationClass { get; set; } = "Migration";

    /// <summary>
    /// Additional using statements to add to generated migrations
    /// </summary>
    public List<string> AdditionalUsings { get; set; } = new();

    /// <summary>
    /// Custom column name transformer
    /// </summary>
    public Func<string, string> ColumnNameTransformer { get; set; } = name => name;

    /// <summary>
    /// Custom table name transformer
    /// </summary>
    public Func<string, string> TableNameTransformer { get; set; } = name => name;

    /// <summary>
    /// Whether to add [Tags] attribute to migrations
    /// </summary>
    public List<List<string>> DefaultTags { get; set; } = new();

    /// <summary>
    /// Timestamp formatter for migration identifiers
    /// </summary>
    public string TimestampFormat { get; set; } = "yyyyMMddHHmmss";

    /// <summary>
    /// Timestamp provider for migration identifiers
    /// </summary>
    public Func<string, string> TimestampProvider { get; set; } = format => DateTime.UtcNow.ToString(format);
}
