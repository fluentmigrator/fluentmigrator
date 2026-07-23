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

namespace FluentMigrator.EFCore.Example.DbContext;

/// <summary>
/// Demonstrates JSON column type mapping
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The JSON data
    /// </summary>
    public Change Changes { get; set; }

    /// <summary>
    /// The name of the entity that was changed
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// The primary key of the entity that was changed
    /// </summary>
    public ActionTypeEnum Action { get; set; }

    /// <summary>
    /// The date of the change
    /// </summary>
    public DateTime Timestamp { get; set; }
}

public class Change
{
    public string PropertyName { get; set; }

    public string OldValue { get; set; }

    public string NewValue { get; set; }
}

public enum ActionTypeEnum
{
    Created,
    Updated,
    Deleted
}
