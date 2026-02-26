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
/// Demonstrates Many-to-Many relationship with Product through ProductTag.
/// </summary>
public class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}

/// <summary>
/// Join entity for Many-to-Many relationship with payload (AssignedAt, AssignedBy).
/// </summary>
public class ProductTag
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;

    // Payload properties
    public DateTime AssignedAt { get; set; }

    public string? AssignedBy { get; set; }
}
