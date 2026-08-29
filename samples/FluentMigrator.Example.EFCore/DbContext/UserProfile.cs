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

using System.ComponentModel.DataAnnotations;

namespace FluentMigrator.EFCore.Example.DbContext;

/// <summary>
/// Demonstrates One-to-One relationship with User and Owned Type (Address).
/// </summary>
public class UserProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(255)]
    public string? AvatarUrl { get; set; }

    public DateTime? DateOfBirth { get; set; }

    // Owned type - will be stored in the same table
    public Address? Address { get; set; }
}

/// <summary>
/// Owned type - embedded in UserProfile table.
/// </summary>
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
