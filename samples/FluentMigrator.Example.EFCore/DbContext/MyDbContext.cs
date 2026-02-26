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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FluentMigrator.EFCore.Example.DbContext;

public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    // TPH (Table Per Hierarchy) inheritance
    public DbSet<Payment> Payments { get; set; }
    public DbSet<CreditCardPayment> CreditCardPayments { get; set; }
    public DbSet<BankTransferPayment> BankTransferPayments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Fakedb");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ===== Product Configuration =====
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsFixedLength();

            // Foreign key relationship (Many-to-One)
            entity.HasOne(p => p.User)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Foreign key to Category
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Composite index
            entity.HasIndex(p => new { p.CategoryId, p.Price })
                .HasDatabaseName("IX_Product_Category_Price");

            // Check constraint
            entity.ToTable(t => t.HasCheckConstraint("CK_Product_Price", "[ProductPrice] >= 0"));
        });

        // ===== User Configuration =====
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Name).UseCollation("SQL_Latin1_General_CP1_CI_AS");

            // Shadow property (not in the entity class)
            entity.Property<DateTime>("LastModified");

            // Global query filter (soft delete)
            entity.HasQueryFilter(u => !u.IsDeleted);

            // Default value
            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Computed column
            entity.Property(u => u.FullName)
                .HasComputedColumnSql("[Name] + ' <' + [Email] + '>'", stored: false);

            // Value converter for enum stored as string
            entity.Property(u => u.UserType)
                .HasConversion(new EnumToStringConverter<UserTypeEnum>());

            // Unique constraint
            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_User_Email_Unique");
        });

        // ===== UserProfile Configuration (One-to-One) =====
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(up => up.Id);

            // One-to-One relationship
            entity.HasOne(up => up.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Owned type (Address embedded in UserProfile table)
            entity.OwnsOne(up => up.Address, address =>
            {
                address.Property(a => a.Street).HasColumnName("Address_Street").HasMaxLength(200);
                address.Property(a => a.City).HasColumnName("Address_City").HasMaxLength(100);
                address.Property(a => a.ZipCode).HasColumnName("Address_ZipCode").HasMaxLength(20);
                address.Property(a => a.Country).HasColumnName("Address_Country").HasMaxLength(100);
            });
        });

        // ===== Order Configuration =====
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);

            // Alternate key
            entity.HasAlternateKey(o => o.OrderNumber);

            entity.Property(o => o.OrderNumber)
                .HasMaxLength(50);

            entity.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Owned collection (stored in separate table)
            entity.OwnsMany(o => o.ShippingHistory, sh =>
            {
                sh.WithOwner().HasForeignKey("OrderId");
                sh.Property<int>("Id");
                sh.HasKey("Id");
                sh.ToTable("OrderShippingHistory");
            });

            // Value converter for Status enum
            entity.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        // ===== OrderItem Configuration =====
        modelBuilder.Entity<OrderItem>(entity =>
        {
            // Composite primary key
            entity.HasKey(oi => new { oi.OrderId, oi.ProductId });

            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            entity.HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            entity.Property(oi => oi.UnitPrice)
                .HasPrecision(18, 4);
        });

        // ===== Category Configuration (Self-referencing / Hierarchical) =====
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Self-referencing relationship (hierarchical)
            entity.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== Tag Configuration (Many-to-Many with Product) =====
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(t => t.Name).IsUnique();

            entity.HasData(
                new Tag { Id = 1, Name = "New" },
                new Tag { Id = 2, Name = "Sale" },
                new Tag { Id = 3, Name = "Popular" }
            );
        });

        // ===== ProductTag Configuration (Many-to-Many join entity with payload) =====
        modelBuilder.Entity<ProductTag>(entity =>
        {
            entity.HasKey(pt => new { pt.ProductId, pt.TagId });

            entity.HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTags)
                .HasForeignKey(pt => pt.ProductId);

            entity.HasOne(pt => pt.Tag)
                .WithMany(t => t.ProductTags)
                .HasForeignKey(pt => pt.TagId);

            // Payload property
            entity.Property(pt => pt.AssignedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ===== Payment Configuration (TPH - Table Per Hierarchy) =====
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);

            // Discriminator for TPH inheritance
            entity.HasDiscriminator<string>("PaymentType")
                .HasValue<CreditCardPayment>("CreditCard")
                .HasValue<BankTransferPayment>("BankTransfer");

            entity.HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId);

            entity.Property(p => p.Amount)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<CreditCardPayment>(entity =>
        {
            entity.Property(cc => cc.CardLastFourDigits)
                .HasMaxLength(4)
                .IsFixedLength();
        });

        modelBuilder.Entity<BankTransferPayment>(entity =>
        {
            entity.Property(bt => bt.BankAccountNumber)
                .HasMaxLength(34); // IBAN max length
        });

        // ===== AuditLog Configuration =====
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.Id);

            // JSON column (EF Core 7+)
            entity.OwnsOne(a => a.Changes, changes =>
            {
                changes.ToJson();
            });

            entity.Property(a => a.EntityName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.Action)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Index for querying by date
            entity.HasIndex(a => a.Timestamp)
                .HasDatabaseName("IX_AuditLog_Timestamp");
        });

        // ===== Sequence Configuration =====
        modelBuilder.HasSequence<int>("OrderNumbers", schema: "dbo")
            .StartsAt(1000)
            .IncrementsBy(1);
    }
}
