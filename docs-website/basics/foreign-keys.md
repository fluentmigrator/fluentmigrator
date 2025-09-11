# Foreign Keys

Foreign key constraints are essential for maintaining data integrity and establishing relationships between tables. FluentMigrator provides comprehensive support for creating and managing foreign key relationships across different database providers.

## Basic Foreign Key Operations

### Creating Simple Foreign Keys

```csharp
public class BasicForeignKeys : Migration
{
    public override void Up()
    {
        // Create parent table
        Create.Table("Categories")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Description").AsString(500).Nullable();

        // Create child table
        Create.Table("Products")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Price").AsDecimal(10, 2).NotNullable()
            .WithColumn("CategoryId").AsInt32().NotNullable();

        // Create foreign key constraint
        Create.ForeignKey("FK_Products_Categories")
            .FromTable("Products").ForeignColumn("CategoryId")
            .ToTable("Categories").PrimaryColumn("Id");
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_Products_Categories").OnTable("Products");
        Delete.Table("Products");
        Delete.Table("Categories");
    }
}
```

### Foreign Keys with Cascading Actions

```csharp
public class ForeignKeysWithCascading : Migration
{
    public override void Up()
    {
        Create.Table("Customers")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable();

        Create.Table("Orders")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("OrderDate").AsDateTime().NotNullable()
            .WithColumn("TotalAmount").AsDecimal(10, 2).NotNullable()
            .WithColumn("CustomerId").AsInt32().NotNullable();

        Create.Table("OrderItems")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("OrderId").AsInt32().NotNullable()
            .WithColumn("ProductName").AsString(100).NotNullable()
            .WithColumn("Quantity").AsInt32().NotNullable()
            .WithColumn("UnitPrice").AsDecimal(10, 2).NotNullable();

        // FK with CASCADE DELETE - when customer is deleted, orders are deleted
        Create.ForeignKey("FK_Orders_Customers")
            .FromTable("Orders").ForeignColumn("CustomerId")
            .ToTable("Customers").PrimaryColumn("Id")
            .OnDelete(Rule.Cascade);

        // FK with CASCADE DELETE - when order is deleted, order items are deleted
        Create.ForeignKey("FK_OrderItems_Orders")
            .FromTable("OrderItems").ForeignColumn("OrderId")
            .ToTable("Orders").PrimaryColumn("Id")
            .OnDelete(Rule.Cascade);
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_OrderItems_Orders").OnTable("OrderItems");
        Delete.ForeignKey("FK_Orders_Customers").OnTable("Orders");
        Delete.Table("OrderItems");
        Delete.Table("Orders");
        Delete.Table("Customers");
    }
}
```

### Foreign Keys with Different Actions

```csharp
public class ForeignKeyActions : Migration
{
    public override void Up()
    {
        Create.Table("Departments")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable();

        Create.Table("Employees")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("DepartmentId").AsInt32().Nullable()
            .WithColumn("ManagerId").AsInt32().Nullable();

        Create.Table("Projects")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("LeaderId").AsInt32().Nullable();

        // SET NULL - when department is deleted, employee's department becomes NULL
        Create.ForeignKey("FK_Employees_Departments")
            .FromTable("Employees").ForeignColumn("DepartmentId")
            .ToTable("Departments").PrimaryColumn("Id")
            .OnDelete(Rule.SetNull);

        // CASCADE - when employee is deleted, referencing records are also deleted
        Create.ForeignKey("FK_Employees_Manager")
            .FromTable("Employees").ForeignColumn("ManagerId")
            .ToTable("Employees").PrimaryColumn("Id")
            .OnDelete(Rule.SetNull); // Manager deletion sets subordinate's manager to NULL

        // RESTRICT - prevents deletion of employee if they lead projects
        Create.ForeignKey("FK_Projects_Leader")
            .FromTable("Projects").ForeignColumn("LeaderId")
            .ToTable("Employees").PrimaryColumn("Id")
            .OnDelete(Rule.None); // Default behavior - restrict deletion
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_Projects_Leader").OnTable("Projects");
        Delete.ForeignKey("FK_Employees_Manager").OnTable("Employees");
        Delete.ForeignKey("FK_Employees_Departments").OnTable("Employees");
        Delete.Table("Projects");
        Delete.Table("Employees");
        Delete.Table("Departments");
    }
}
```

## Composite Foreign Keys

### Multi-Column Foreign Keys

```csharp
public class CompositeForeignKeys : Migration
{
    public override void Up()
    {
        // Parent table with composite primary key
        Create.Table("CompanyDepartments")
            .WithColumn("CompanyId").AsInt32().NotNullable()
            .WithColumn("DepartmentCode").AsString(10).NotNullable()
            .WithColumn("DepartmentName").AsString(100).NotNullable();

        Create.PrimaryKey("PK_CompanyDepartments")
            .OnTable("CompanyDepartments")
            .Columns("CompanyId", "DepartmentCode");

        // Child table referencing composite key
        Create.Table("Employees")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("CompanyId").AsInt32().NotNullable()
            .WithColumn("DepartmentCode").AsString(10).NotNullable()
            .WithColumn("Position").AsString(100).NotNullable();

        // Composite foreign key
        Create.ForeignKey("FK_Employees_CompanyDepartments")
            .FromTable("Employees").ForeignColumns("CompanyId", "DepartmentCode")
            .ToTable("CompanyDepartments").PrimaryColumns("CompanyId", "DepartmentCode");
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_Employees_CompanyDepartments").OnTable("Employees");
        Delete.Table("Employees");
        Delete.Table("CompanyDepartments");
    }
}
```

### Cross-Schema Foreign Keys

```csharp
public class CrossSchemaForeignKeys : Migration
{
    public override void Up()
    {
        // Create schemas first
        Create.Schema("Sales");
        Create.Schema("Inventory");

        // Table in Sales schema
        Create.Table("Customers")
            .InSchema("Sales")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable();

        // Table in Inventory schema
        Create.Table("Products")
            .InSchema("Inventory")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Price").AsDecimal(10, 2).NotNullable();

        // Table in Sales schema referencing both schemas
        Create.Table("Orders")
            .InSchema("Sales")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("CustomerId").AsInt32().NotNullable()
            .WithColumn("ProductId").AsInt32().NotNullable()
            .WithColumn("Quantity").AsInt32().NotNullable()
            .WithColumn("OrderDate").AsDateTime().NotNullable();

        // Foreign key within same schema
        Create.ForeignKey("FK_Orders_Customers")
            .FromTable("Orders").InSchema("Sales").ForeignColumn("CustomerId")
            .ToTable("Customers").InSchema("Sales").PrimaryColumn("Id");

        // Foreign key across schemas
        Create.ForeignKey("FK_Orders_Products")
            .FromTable("Orders").InSchema("Sales").ForeignColumn("ProductId")
            .ToTable("Products").InSchema("Inventory").PrimaryColumn("Id");
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_Orders_Products").OnTable("Orders").InSchema("Sales");
        Delete.ForeignKey("FK_Orders_Customers").OnTable("Orders").InSchema("Sales");
        Delete.Table("Orders").InSchema("Sales");
        Delete.Table("Products").InSchema("Inventory");
        Delete.Table("Customers").InSchema("Sales");
        Delete.Schema("Inventory");
        Delete.Schema("Sales");
    }
}
```

## Self-Referencing Foreign Keys

### Hierarchical Relationships

```csharp
public class SelfReferencingForeignKeys : Migration
{
    public override void Up()
    {
        // Employee hierarchy table
        Create.Table("Employees")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Position").AsString(100).NotNullable()
            .WithColumn("ManagerId").AsInt32().Nullable() // Self-referencing
            .WithColumn("HireDate").AsDateTime().NotNullable();

        // Self-referencing foreign key
        Create.ForeignKey("FK_Employees_Manager")
            .FromTable("Employees").ForeignColumn("ManagerId")
            .ToTable("Employees").PrimaryColumn("Id")
            .OnDelete(Rule.SetNull); // When manager is deleted, set subordinates' ManagerId to NULL

        // Category hierarchy
        Create.Table("Categories")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("ParentCategoryId").AsInt32().Nullable()
            .WithColumn("Level").AsInt32().NotNullable().WithDefaultValue(1);

        Create.ForeignKey("FK_Categories_Parent")
            .FromTable("Categories").ForeignColumn("ParentCategoryId")
            .ToTable("Categories").PrimaryColumn("Id")
            .OnDelete(Rule.Cascade); // When parent is deleted, delete children too
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_Categories_Parent").OnTable("Categories");
        Delete.ForeignKey("FK_Employees_Manager").OnTable("Employees");
        Delete.Table("Categories");
        Delete.Table("Employees");
    }
}
```

## Advanced Foreign Key Scenarios

### Many-to-Many Relationships

```csharp
public class ManyToManyRelationships : Migration
{
    public override void Up()
    {
        // Students table
        Create.Table("Students")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable()
            .WithColumn("EnrollmentDate").AsDateTime().NotNullable();

        // Courses table
        Create.Table("Courses")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Code").AsString(10).NotNullable()
            .WithColumn("Credits").AsInt32().NotNullable();

        // Junction table for many-to-many relationship
        Create.Table("StudentCourses")
            .WithColumn("StudentId").AsInt32().NotNullable()
            .WithColumn("CourseId").AsInt32().NotNullable()
            .WithColumn("EnrollmentDate").AsDateTime().NotNullable()
            .WithColumn("Grade").AsString(2).Nullable()
            .WithColumn("Status").AsString(20).NotNullable().WithDefaultValue("Enrolled");

        // Composite primary key for junction table
        Create.PrimaryKey("PK_StudentCourses")
            .OnTable("StudentCourses")
            .Columns("StudentId", "CourseId");

        // Foreign keys to both parent tables
        Create.ForeignKey("FK_StudentCourses_Students")
            .FromTable("StudentCourses").ForeignColumn("StudentId")
            .ToTable("Students").PrimaryColumn("Id")
            .OnDelete(Rule.Cascade);

        Create.ForeignKey("FK_StudentCourses_Courses")
            .FromTable("StudentCourses").ForeignColumn("CourseId")
            .ToTable("Courses").PrimaryColumn("Id")
            .OnDelete(Rule.Cascade);
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_StudentCourses_Courses").OnTable("StudentCourses");
        Delete.ForeignKey("FK_StudentCourses_Students").OnTable("StudentCourses");
        Delete.Table("StudentCourses");
        Delete.Table("Courses");
        Delete.Table("Students");
    }
}
```

### Optional vs Required Relationships

```csharp
public class OptionalVsRequiredRelationships : Migration
{
    public override void Up()
    {
        Create.Table("Companies")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Industry").AsString(50).NotNullable();

        Create.Table("Addresses")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Street").AsString(200).NotNullable()
            .WithColumn("City").AsString(100).NotNullable()
            .WithColumn("State").AsString(50).NotNullable()
            .WithColumn("ZipCode").AsString(10).NotNullable();

        Create.Table("Employees")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("FirstName").AsString(50).NotNullable()
            .WithColumn("LastName").AsString(50).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable()

            // Required relationship - every employee must belong to a company
            .WithColumn("CompanyId").AsInt32().NotNullable()

            // Optional relationship - employee may or may not have an address on file
            .WithColumn("AddressId").AsInt32().Nullable()

            // Optional relationship - employee may or may not have a manager
            .WithColumn("ManagerId").AsInt32().Nullable();

        // Required foreign key (NOT NULL column)
        Create.ForeignKey("FK_Employees_Companies")
            .FromTable("Employees").ForeignColumn("CompanyId")
            .ToTable("Companies").PrimaryColumn("Id")
            .OnDelete(Rule.None); // Prevent company deletion if employees exist

        // Optional foreign key (NULL column)
        Create.ForeignKey("FK_Employees_Addresses")
            .FromTable("Employees").ForeignColumn("AddressId")
            .ToTable("Addresses").PrimaryColumn("Id")
            .OnDelete(Rule.SetNull); // If address is deleted, set to NULL

        // Optional self-referencing foreign key
        Create.ForeignKey("FK_Employees_Manager")
            .FromTable("Employees").ForeignColumn("ManagerId")
            .ToTable("Employees").PrimaryColumn("Id")
            .OnDelete(Rule.SetNull);
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_Employees_Manager").OnTable("Employees");
        Delete.ForeignKey("FK_Employees_Addresses").OnTable("Employees");
        Delete.ForeignKey("FK_Employees_Companies").OnTable("Employees");
        Delete.Table("Employees");
        Delete.Table("Addresses");
        Delete.Table("Companies");
    }
}
```

### Conditional Foreign Key Creation

```csharp
public class ConditionalForeignKeys : Migration
{
    public override void Up()
    {
        // Only create foreign key if both tables exist and have data
        if (Schema.Table("Users").Exists() && Schema.Table("Roles").Exists())
        {
            // Check if the foreign key doesn't already exist
            if (!Schema.Table("Users").Constraint("FK_Users_Roles").Exists())
            {
                Create.ForeignKey("FK_Users_Roles")
                    .FromTable("Users").ForeignColumn("RoleId")
                    .ToTable("Roles").PrimaryColumn("Id")
                    .OnDelete(Rule.None);
            }
        }
    }

    public override void Down()
    {
        if (Schema.Table("Users").Constraint("FK_Users_Roles").Exists())
        {
            Delete.ForeignKey("FK_Users_Roles").OnTable("Users");
        }
    }
}
```

## Best Practices for Foreign Keys

### 1. Naming Conventions

```csharp
public class ForeignKeyNaming : Migration
{
    public override void Up()
    {
        // Convention: FK_ChildTable_ParentTable
        Create.ForeignKey("FK_Orders_Customers")
            .FromTable("Orders").ForeignColumn("CustomerId")
            .ToTable("Customers").PrimaryColumn("Id");

        // For self-referencing: FK_Table_RoleName
        Create.ForeignKey("FK_Employees_Manager")
            .FromTable("Employees").ForeignColumn("ManagerId")
            .ToTable("Employees").PrimaryColumn("Id");

        // For multiple FKs to same table: FK_ChildTable_ParentTable_Role
        Create.ForeignKey("FK_Orders_Employees_Salesperson")
            .FromTable("Orders").ForeignColumn("SalespersonId")
            .ToTable("Employees").PrimaryColumn("Id");

        Create.ForeignKey("FK_Orders_Employees_ApprovedBy")
            .FromTable("Orders").ForeignColumn("ApprovedById")
            .ToTable("Employees").PrimaryColumn("Id");
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_Orders_Employees_ApprovedBy").OnTable("Orders");
        Delete.ForeignKey("FK_Orders_Employees_Salesperson").OnTable("Orders");
        Delete.ForeignKey("FK_Employees_Manager").OnTable("Employees");
        Delete.ForeignKey("FK_Orders_Customers").OnTable("Orders");
    }
}
```

### 2. Choosing Appropriate Actions

```csharp
public class ForeignKeyActionGuidelines : Migration
{
    public override void Up()
    {
        // CASCADE: Use for parent-child relationships where child cannot exist without parent
        Create.ForeignKey("FK_OrderDetails_Orders")
            .FromTable("OrderDetails").ForeignColumn("OrderId")
            .ToTable("Orders").PrimaryColumn("Id")
            .OnDelete(Rule.Cascade); // Order details are meaningless without order

        // SET NULL: Use for optional relationships
        Create.ForeignKey("FK_Employees_Manager")
            .FromTable("Employees").ForeignColumn("ManagerId")
            .ToTable("Employees").PrimaryColumn("Id")
            .OnDelete(Rule.SetNull); // Employee can exist without manager

        // RESTRICT (None): Use for lookup tables or when deletion should be prevented
        Create.ForeignKey("FK_Products_Categories")
            .FromTable("Products").ForeignColumn("CategoryId")
            .ToTable("Categories").PrimaryColumn("Id")
            .OnDelete(Rule.None); // Prevent category deletion if products exist
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_Products_Categories").OnTable("Products");
        Delete.ForeignKey("FK_Employees_Manager").OnTable("Employees");
        Delete.ForeignKey("FK_OrderDetails_Orders").OnTable("OrderDetails");
    }
}
```

### 3. Documentation and Validation

```csharp
public class ForeignKeyDocumentation : Migration
{
    public override void Up()
    {
        // Document the business rules through foreign key design
        Create.Table("BusinessRuleExample")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()

            // Required relationship (business rule: every record must have a category)
            .WithColumn("CategoryId").AsInt32().NotNullable()

            // Optional relationship (business rule: approval is optional)
            .WithColumn("ApprovedById").AsInt32().Nullable()

            // Audit trail (business rule: track who created the record)
            .WithColumn("CreatedById").AsInt32().NotNullable();

        // Document the relationships through consistent foreign key creation
        Create.ForeignKey("FK_BusinessRuleExample_Categories")
            .FromTable("BusinessRuleExample").ForeignColumn("CategoryId")
            .ToTable("Categories").PrimaryColumn("Id")
            .OnDelete(Rule.None); // Business rule: categories must exist

        Create.ForeignKey("FK_BusinessRuleExample_Users_ApprovedBy")
            .FromTable("BusinessRuleExample").ForeignColumn("ApprovedById")
            .ToTable("Users").PrimaryColumn("Id")
            .OnDelete(Rule.SetNull); // Business rule: approval can be removed

        Create.ForeignKey("FK_BusinessRuleExample_Users_CreatedBy")
            .FromTable("BusinessRuleExample").ForeignColumn("CreatedById")
            .ToTable("Users").PrimaryColumn("Id")
            .OnDelete(Rule.None); // Business rule: audit trail must be preserved
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_BusinessRuleExample_Users_CreatedBy").OnTable("BusinessRuleExample");
        Delete.ForeignKey("FK_BusinessRuleExample_Users_ApprovedBy").OnTable("BusinessRuleExample");
        Delete.ForeignKey("FK_BusinessRuleExample_Categories").OnTable("BusinessRuleExample");
        Delete.Table("BusinessRuleExample");
    }
}
```

## Troubleshooting Foreign Key Issues

### Handling Circular References

```csharp
public class CircularReferences : Migration
{
    public override void Up()
    {
        // Create tables first without foreign keys
        Create.Table("TableA")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("TableBId").AsInt32().Nullable();

        Create.Table("TableB")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("TableAId").AsInt32().Nullable();

        // Create foreign keys after both tables exist
        Create.ForeignKey("FK_TableA_TableB")
            .FromTable("TableA").ForeignColumn("TableBId")
            .ToTable("TableB").PrimaryColumn("Id")
            .OnDelete(Rule.SetNull);

        Create.ForeignKey("FK_TableB_TableA")
            .FromTable("TableB").ForeignColumn("TableAId")
            .ToTable("TableA").PrimaryColumn("Id")
            .OnDelete(Rule.SetNull);
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_TableB_TableA").OnTable("TableB");
        Delete.ForeignKey("FK_TableA_TableB").OnTable("TableA");
        Delete.Table("TableB");
        Delete.Table("TableA");
    }
}
```

## Database-Specific Foreign Key Features

Different database providers offer specialized foreign key options and advanced referential integrity features:

### SQL Server Specific
- **Clustered Foreign Keys**: Performance optimization for large tables - [SQL Server Provider](../providers/sql-server.md)

## See Also
- [Constraints](./constraints.md) - Other constraint types and validation
- [Indexes](./indexes.md) - Foreign key index optimization
- [Execute SQL](../operations/execute-sql.md) - Custom foreign key implementation
