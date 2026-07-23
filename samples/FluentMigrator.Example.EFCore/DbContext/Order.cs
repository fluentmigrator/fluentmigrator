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
/// Demonstrates alternate keys, owned collections, and enum conversion.
/// </summary>
public class Order
{
    public int Id { get; set; }

    // Alternate key
    [Required]
    public string OrderNumber { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public OrderStatus Status { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    // Owned collection - stored in separate table
    public ICollection<ShippingUpdate> ShippingHistory { get; set; } = new List<ShippingUpdate>();
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>
/// Owned type collection - stored in separate table.
/// </summary>
public class ShippingUpdate
{
    public DateTime Timestamp { get; set; }

    [MaxLength(100)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Location { get; set; }
}
