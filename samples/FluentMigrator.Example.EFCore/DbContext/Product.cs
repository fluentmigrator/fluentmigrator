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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace FluentMigrator.EFCore.Example.DbContext;

[Index(nameof(UniqueIdentifier), IsUnique = true)]
public class Product
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Login { get; set; } = string.Empty;

    [StringLength(10)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(int.MaxValue)]
    public string Description { get; set; } = string.Empty;

    [Column("ProductPrice")]
    [Precision(18, 2)]
    public decimal Price { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid UniqueIdentifier { get; set; }

    [NotMapped]
    public int StockQuantity { get; set; }

    // Many-to-Many with Tag through ProductTag
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}
