# Schema Operations.

## Creating and Managing Schemas

```csharp
public class BasicSchemaOperations : Migration
{
    public override void Up()
    {
        // Create schemas
        Create.Schema("Sales");
        Create.Schema("Inventory");
        Create.Schema("HR");
        Create.Schema("Reporting");

        // Create tables in specific schemas
        Create.Table("Customers")
            .InSchema("Sales")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable();

        Create.Table("Products")
            .InSchema("Inventory")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("SKU").AsString(50).NotNullable()
            .WithColumn("Price").AsDecimal(10, 2).NotNullable();

        Create.Table("Employees")
            .InSchema("HR")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("FirstName").AsString(50).NotNullable()
            .WithColumn("LastName").AsString(50).NotNullable()
            .WithColumn("Department").AsString(50).NotNullable()
            .WithColumn("HireDate").AsDateTime().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Employees").InSchema("HR");
        Delete.Table("Products").InSchema("Inventory");
        Delete.Table("Customers").InSchema("Sales");

        Delete.Schema("Reporting");
        Delete.Schema("HR");
        Delete.Schema("Inventory");
        Delete.Schema("Sales");
    }
}
```

## Cross-Schema References

```csharp
public class CrossSchemaReferences : Migration
{
    public override void Up()
    {
        // Create related tables in different schemas
        Create.Table("Orders")
            .InSchema("Sales")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("CustomerId").AsInt32().NotNullable()
            .WithColumn("OrderDate").AsDateTime().NotNullable()
            .WithColumn("TotalAmount").AsDecimal(10, 2).NotNullable();

        Create.Table("OrderItems")
            .InSchema("Sales")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("OrderId").AsInt32().NotNullable()
            .WithColumn("ProductId").AsInt32().NotNullable()
            .WithColumn("Quantity").AsInt32().NotNullable()
            .WithColumn("UnitPrice").AsDecimal(10, 2).NotNullable();

        // Cross-schema foreign keys
        Create.ForeignKey("FK_Orders_Customers")
            .FromTable("Orders").InSchema("Sales").ForeignColumn("CustomerId")
            .ToTable("Customers").InSchema("Sales").PrimaryColumn("Id");

        Create.ForeignKey("FK_OrderItems_Orders")
            .FromTable("OrderItems").InSchema("Sales").ForeignColumn("OrderId")
            .ToTable("Orders").InSchema("Sales").PrimaryColumn("Id");

        Create.ForeignKey("FK_OrderItems_Products")
            .FromTable("OrderItems").InSchema("Sales").ForeignColumn("ProductId")
            .ToTable("Products").InSchema("Inventory").PrimaryColumn("Id");
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_OrderItems_Products").OnTable("OrderItems").InSchema("Sales");
        Delete.ForeignKey("FK_OrderItems_Orders").OnTable("OrderItems").InSchema("Sales");
        Delete.ForeignKey("FK_Orders_Customers").OnTable("Orders").InSchema("Sales");

        Delete.Table("OrderItems").InSchema("Sales");
        Delete.Table("Orders").InSchema("Sales");
    }
}
```
