# Unifying Sequence Creation and Deletion with Table and Column Operations

## Status

Proposed

## Context

FluentMigrator currently supports sequence operations through `Create.Sequence()` and `Delete.Sequence()`, but these operations are not integrated with table and column creation workflows. This creates a disconnected experience when developers want to use sequences as default values for columns, particularly in scenarios like:

```sql
ALTER TABLE [Purchasing].[Suppliers] 
ADD CONSTRAINT [DF_Purchasing_Suppliers_SupplierID] 
DEFAULT (NEXT VALUE FOR [Sequences].[SupplierID]) 
FOR [SupplierID]
```

This ADR addresses issue #1228 and proposes a unified approach to sequence management that aligns with FluentMigrator's fluent interface patterns for tables and columns.

## Database Provider Support Analysis

### Native ANSI SQL SEQUENCE Support

| Database Provider | Native Support | Syntax | Notes |
|-------------------|----------------|--------|-------|
| **SQL Server 2012+** | ✅ Yes | `CREATE SEQUENCE [schema].[name]` | Full ANSI SQL support since SQL Server 2012 |
| **PostgreSQL** | ✅ Yes | `CREATE SEQUENCE schema.name` | Full ANSI SQL support; also supports SERIAL pseudo-type |
| **Oracle** | ✅ Yes | `CREATE SEQUENCE schema.name` | Full ANSI SQL support |
| **Firebird 3.0+** | ✅ Yes | `CREATE SEQUENCE name` | Schema support added in Firebird 3.0 |
| **DB2** | ✅ Yes | `CREATE SEQUENCE schema.name` | Full ANSI SQL support |
| **SAP HANA** | ✅ Yes | `CREATE SEQUENCE schema.name` | Full ANSI SQL support |
| **Snowflake** | ✅ Yes | `CREATE SEQUENCE schema.name` | Full ANSI SQL support |
| **Redshift** | ✅ Yes | `CREATE SEQUENCE schema.name` | Based on PostgreSQL |
| **MariaDB** | ✅ Yes | `CREATE SEQUENCE [schema.]name` | Native sequence support via SEQUENCE Engine |
| **MySQL 5.x/8.x** | ❌ No | N/A | No native sequence support |
| **SQLite** | ❌ No | N/A | No native sequence support |
| **Jet (MS Access)** | ❌ No | N/A | No native sequence support; uses AutoNumber |

### Workarounds for Non-Supporting Databases

#### MySQL Workaround
MySQL doesn't support sequences but provides AUTO_INCREMENT for similar functionality:
- **Option 1**: Use AUTO_INCREMENT columns (most common)
- **Option 2**: Simulate sequences using a dedicated table:
  ```sql
  CREATE TABLE sequences (name VARCHAR(50) PRIMARY KEY, current_value BIGINT);
  INSERT INTO sequences VALUES ('sequence_name', 0);
  UPDATE sequences SET current_value = LAST_INSERT_ID(current_value + 1) WHERE name = 'sequence_name';
  SELECT LAST_INSERT_ID();
  ```
- **Recommendation**: Document that sequences map to AUTO_INCREMENT in MySQL context

**Note on MariaDB**: MariaDB, a fork of MySQL, provides native `CREATE SEQUENCE` support through its SEQUENCE Engine, offering an experience similar to Oracle or PostgreSQL. If using MariaDB instead of MySQL, native sequence operations are fully supported. See [MariaDB SEQUENCE documentation](https://mariadb.com/docs/server/reference/sql-structure/sequences/create-sequence).

#### SQLite Workaround
SQLite doesn't support sequences but provides AUTOINCREMENT:
- **Option 1**: Use AUTOINCREMENT for INTEGER PRIMARY KEY
- **Option 2**: Use sqlite_sequence system table (automatically managed)
- **Recommendation**: Document that sequences are not portable to SQLite

#### Jet (MS Access) Workaround
- Use AutoNumber data type
- Limited control over sequence behavior

## Current State

### Existing API Structure

```csharp
// Current sequence creation (standalone)
Create.Sequence("SequenceName")
    .InSchema("SchemaName")
    .StartWith(1000)
    .IncrementBy(1)
    .MinValue(1)
    .MaxValue(9999999999)
    .Cache(20)
    .Cycle();

// Current table creation (separate workflow)
Create.Table("TableName")
    .WithColumn("Id").AsInt64().NotNullable().PrimaryKey()
    .WithColumn("Name").AsString(100).NotNullable();

// No built-in way to connect default value to sequence
```

### Gap Analysis

1. **No fluent integration**: Cannot declare sequence as column default in table creation
2. **Multi-statement requirement**: Requires separate expressions for sequence and default constraint
3. **No automatic cleanup**: Deleting a table doesn't cascade to related sequences
4. **Provider inconsistency**: Different behavior across database providers

## Proposed Solutions

### Option 1: Extended Column Syntax (Recommended)

Extend the column builder to support sequence-based defaults directly:

```csharp
// Approach 1A: Explicit sequence reference
Create.Table("Suppliers")
    .WithColumn("SupplierID").AsInt64().NotNullable().PrimaryKey()
        .WithDefaultSequence("SupplierID")
        .InSchema("Sequences")
    .WithColumn("Name").AsString(100).NotNullable();

// Approach 1B: Inline sequence creation
Create.Table("Suppliers")
    .WithColumn("SupplierID").AsInt64().NotNullable().PrimaryKey()
        .WithDefault()
            .Sequence("SupplierID")
            .InSchema("Sequences")
            .StartWith(1000)
            .IncrementBy(1)
    .WithColumn("Name").AsString(100).NotNullable();

// Approach 1C: PostgreSQL SERIAL-like syntax
Create.Table("Suppliers")
    .WithColumn("SupplierID").AsSerial().NotNullable().PrimaryKey()
    .WithColumn("Name").AsString(100).NotNullable();
```

**Advantages:**
- Natural integration with fluent table/column syntax
- Reads left-to-right in migration definition
- Aligns with how developers think about the problem
- Can auto-generate sequence names following conventions

**Disadvantages:**
- Requires new syntax extensions
- May need multiple expressions generated internally
- Complexity in handling database-specific behavior

### Option 2: Sequence-First with Table Reference

Start with sequence creation and reference it in table:

```csharp
// Create sequence first
var supplierSeq = Create.Sequence("SupplierID")
    .InSchema("Sequences")
    .StartWith(1000)
    .IncrementBy(1);

// Reference in table creation
Create.Table("Suppliers")
    .WithColumn("SupplierID").AsInt64().NotNullable().PrimaryKey()
        .WithDefaultSequence(supplierSeq)
    .WithColumn("Name").AsString(100).NotNullable();
```

**Advantages:**
- Explicit sequence ownership
- Sequence can be referenced by multiple tables
- Clear separation of concerns

**Disadvantages:**
- Requires variable/reference passing
- Breaks fluent method chaining
- Less intuitive for simple cases

### Option 3: Declarative Configuration Object

Use a configuration object approach:

```csharp
Create.Table("Suppliers")
    .WithColumn("SupplierID").AsInt64().NotNullable().PrimaryKey()
        .WithDefault(new SequenceDefault
        {
            SequenceName = "SupplierID",
            SchemaName = "Sequences",
            StartWith = 1000,
            IncrementBy = 1
        })
    .WithColumn("Name").AsString(100).NotNullable();
```

**Advantages:**
- Flexible and extensible
- Testable configuration
- Can serialize/deserialize

**Disadvantages:**
- Breaks fluent interface pattern
- More verbose
- Less discoverable

## Decision

**Adopt Option 1 (Extended Column Syntax) with multiple sub-approaches** supported:

1. **Approach 1A** (`WithDefaultSequence()`) for referencing existing sequences
2. **Approach 1B** (`WithDefault().Sequence()`) for inline sequence creation
3. **Approach 1C** (`AsSerial()`) as syntactic sugar for PostgreSQL-style sequences

This provides maximum flexibility while maintaining FluentMigrator's fluent interface design principles.

### Implementation Details

#### New Interfaces and Builders

```csharp
namespace FluentMigrator.Builders.Create.Column
{
    public interface ICreateColumnOptionSyntax
    {
        // Existing methods...
        
        // New methods for sequence defaults
        ICreateColumnOptionSyntax WithDefaultSequence(string sequenceName);
        ICreateColumnOptionSyntax WithDefaultSequence(string sequenceName, string schemaName);
        ICreateColumnDefaultSequenceSyntax WithDefaultSequence();
    }
    
    public interface ICreateColumnDefaultSequenceSyntax
    {
        ICreateColumnDefaultSequenceOptionSyntax Sequence(string sequenceName);
    }
    
    public interface ICreateColumnDefaultSequenceOptionSyntax : ICreateColumnOptionSyntax
    {
        ICreateColumnDefaultSequenceOptionSyntax InSchema(string schemaName);
        ICreateColumnDefaultSequenceOptionSyntax StartWith(long startWith);
        ICreateColumnDefaultSequenceOptionSyntax IncrementBy(long incrementBy);
        ICreateColumnDefaultSequenceOptionSyntax MinValue(long minValue);
        ICreateColumnDefaultSequenceOptionSyntax MaxValue(long maxValue);
        ICreateColumnDefaultSequenceOptionSyntax Cache(long cache);
        ICreateColumnDefaultSequenceOptionSyntax Cycle();
    }
}
```

#### Expression Changes

The implementation should generate multiple expressions:

1. `CreateSequenceExpression` - Create the sequence if it doesn't exist
2. `AlterDefaultConstraintExpression` - Set the default constraint using the sequence

For SQL Server 2012+, this generates:
```sql
CREATE SEQUENCE [Sequences].[SupplierID] START WITH 1000 INCREMENT BY 1;
ALTER TABLE [Purchasing].[Suppliers] 
    ADD CONSTRAINT [DF_Purchasing_Suppliers_SupplierID] 
    DEFAULT (NEXT VALUE FOR [Sequences].[SupplierID]) 
    FOR [SupplierID];
```

#### Database-Specific Behavior

**SQL Server 2012+:**
- Full support for `NEXT VALUE FOR` syntax
- Generate proper sequence and default constraint

**SQL Server 2000/2008:**
- No sequence support
- Throw meaningful error or fall back to IDENTITY columns with warning

**PostgreSQL:**
- Support both explicit sequences and SERIAL pseudo-type
- `AsSerial()` generates `SERIAL` data type
- Explicit sequence generates `NEXTVAL('sequence_name')`

**Oracle:**
- Full sequence support
- Generate `sequence_name.NEXTVAL` syntax

**MySQL:**
- No sequence support
- `WithDefaultSequence()` throws descriptive error
- Document that AUTO_INCREMENT should be used instead
- Potentially map `AsSerial()` to AUTO_INCREMENT with warning

**SQLite:**
- No sequence support
- `WithDefaultSequence()` throws descriptive error  
- Document that AUTOINCREMENT should be used instead

**Firebird:**
- Full sequence support (3.0+)
- Check version and throw error for older versions

**Others (DB2, HANA, Snowflake, Redshift):**
- Full sequence support following ANSI SQL patterns

#### Convention Support

```csharp
public interface ISequenceConvention
{
    string GetSequenceName(string tableName, string columnName);
    string GetSequenceSchema(string tableSchema);
    long GetDefaultStartWith();
    long GetDefaultIncrementBy();
}
```

Default convention:
- Sequence name: `SEQ_{TableName}_{ColumnName}`
- Sequence schema: Same as table schema
- Start with: 1
- Increment by: 1

#### Reverse Migration

For `Down()` migrations:

```csharp
// Automatically generate cleanup
Delete.Table("Suppliers");
// Should also: 
// - Remove default constraint
// - Optionally drop sequence (if not shared)
```

Need tracking mechanism to determine if sequence is shared across tables.

### API Examples

#### Example 1: Simple Sequential ID

```csharp
public override void Up()
{
    Create.Table("Orders")
        .InSchema("Sales")
        .WithColumn("OrderID").AsInt64().NotNullable().PrimaryKey()
            .WithDefaultSequence() // Uses convention: SEQ_Orders_OrderID in Sales schema
        .WithColumn("OrderDate").AsDateTime().NotNullable()
        .WithColumn("CustomerId").AsInt32().NotNullable();
}

public override void Down()
{
    Delete.Table("Orders").InSchema("Sales");
    // Automatically drops SEQ_Orders_OrderID if not shared
}
```

#### Example 2: Custom Sequence Configuration

```csharp
public override void Up()
{
    Create.Table("Invoices")
        .InSchema("Billing")
        .WithColumn("InvoiceNumber").AsInt64().NotNullable().PrimaryKey()
            .WithDefaultSequence()
                .Sequence("InvoiceNumberSeq")
                .InSchema("Sequences")
                .StartWith(100000)
                .IncrementBy(1)
                .MinValue(100000)
                .MaxValue(999999999)
        .WithColumn("InvoiceDate").AsDateTime().NotNullable();
}
```

#### Example 3: Shared Sequence Across Tables

```csharp
public override void Up()
{
    // Create shared sequence first
    Create.Sequence("GlobalEntityID")
        .InSchema("Sequences")
        .StartWith(1)
        .IncrementBy(1);
    
    // Use in multiple tables
    Create.Table("Customers")
        .WithColumn("CustomerID").AsInt64().NotNullable().PrimaryKey()
            .WithDefaultSequence("GlobalEntityID", "Sequences");
    
    Create.Table("Suppliers")
        .WithColumn("SupplierID").AsInt64().NotNullable().PrimaryKey()
            .WithDefaultSequence("GlobalEntityID", "Sequences");
}
```

#### Example 4: PostgreSQL SERIAL Style

```csharp
public override void Up()
{
    // PostgreSQL-friendly syntax
    Create.Table("Products")
        .WithColumn("ProductID").AsSerial().PrimaryKey()  // Maps to SERIAL in Postgres
        .WithColumn("ProductName").AsString(100).NotNullable();
}
```

#### Example 5: Adding Sequence to Existing Column

```csharp
public override void Up()
{
    // Create sequence
    Create.Sequence("OrderSeq")
        .InSchema("Sales")
        .StartWith(1000);
    
    // Add default constraint to existing column
    Alter.Table("Orders")
        .InSchema("Sales")
        .AlterColumn("OrderID").AsInt64().NotNullable()
            .WithDefaultSequence("OrderSeq", "Sales");
}
```

## Consequences

### Positive

1. **Unified Experience**: Sequences integrate naturally with table/column creation workflows
2. **Database Portability**: Clear documentation on provider-specific behavior
3. **Convention Support**: Automatic naming reduces boilerplate code
4. **Flexibility**: Multiple syntax options for different use cases
5. **Discoverability**: IntelliSense guides developers through fluent interface
6. **Migration Safety**: Automatic cleanup in Down() migrations

### Negative

1. **Implementation Complexity**: Requires significant changes to expression builders
2. **Multi-Expression Coordination**: One fluent chain generates multiple SQL statements
3. **Provider Divergence**: MySQL/SQLite require clear error messages or workarounds
4. **Learning Curve**: Developers must understand sequence vs identity column tradeoffs
5. **Backward Compatibility**: Existing `Create.Sequence()` must continue working

### Neutral

1. **Documentation Burden**: Comprehensive docs needed for each database provider
2. **Testing Surface**: Each provider needs thorough integration tests
3. **Convention Configuration**: Users must be able to override default conventions

## Implementation Phases

### Phase 1: Core Infrastructure (Breaking Ground)
- [ ] Add new interfaces to `FluentMigrator.Abstractions`
- [ ] Extend column builders with sequence support
- [ ] Add `SequenceDefaultDefinition` model
- [ ] Update expression builders
- [ ] Add convention interface and default implementation

### Phase 2: Generator Implementation (Provider Support)
- [ ] SQL Server 2012+ generator
- [ ] PostgreSQL generator (including SERIAL support)
- [ ] Oracle generator
- [ ] Firebird generator
- [ ] DB2 generator
- [ ] HANA generator
- [ ] Snowflake generator
- [ ] Redshift generator
- [ ] MySQL generator (error/warning path)
- [ ] SQLite generator (error/warning path)
- [ ] Jet generator (error/warning path)

### Phase 3: Testing
- [ ] Unit tests for expression builders
- [ ] Integration tests for each provider
- [ ] Convention tests
- [ ] Round-trip (Up/Down) tests
- [ ] Error condition tests

### Phase 4: Documentation
- [ ] API documentation with XML comments
- [ ] Usage examples for each pattern
- [ ] Provider-specific documentation
- [ ] Migration guide from current patterns
- [ ] Troubleshooting guide

### Phase 5: Polish
- [ ] Performance optimization
- [ ] Error message refinement
- [ ] Convention customization examples
- [ ] Community feedback integration

## References

- Issue #1228: Original feature request
- ANSI SQL:2003 Standard (Sequences)
- [SQL Server SEQUENCE Documentation](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-sequence-transact-sql)
- [PostgreSQL SEQUENCE Documentation](https://www.postgresql.org/docs/current/sql-createsequence.html)
- [Oracle SEQUENCE Documentation](https://docs.oracle.com/en/database/oracle/oracle-database/19/sqlrf/CREATE-SEQUENCE.html)
- [Firebird SEQUENCE Documentation](https://firebirdsql.org/file/documentation/html/en/refdocs/fblangref30/firebird-30-language-reference.html#fblangref30-ddl-sequence)

## Alternatives Considered

### Alternative: Do Nothing
Keep sequences completely separate from table/column operations. Users manually create sequences and reference them.

**Rejected because:** Doesn't address the core usability issue and doesn't meet acceptance criteria.

### Alternative: Automatic Sequence Generation
Automatically create sequences for all identity/auto-increment columns across all providers.

**Rejected because:** Too opinionated and may conflict with existing table designs. Some providers have better native alternatives (e.g., IDENTITY in SQL Server).

### Alternative: Single Unified Syntax Only
Force one "best" syntax and don't support alternatives.

**Rejected because:** Different databases and use cases benefit from different approaches. Flexibility is key.

## Migration Path for Existing Code

Existing code using `Create.Sequence()` continues to work:

```csharp
// This still works
Create.Sequence("MySequence")
    .InSchema("dbo")
    .StartWith(1);

// Can now also use new syntax
Create.Table("MyTable")
    .WithColumn("Id").AsInt64()
        .WithDefaultSequence("MySequence", "dbo");
```

No breaking changes to existing migrations.

## Future Considerations

1. **Identity Column Integration**: Consider how this interacts with IDENTITY columns in SQL Server
2. **GENERATED ALWAYS AS IDENTITY**: SQL Server 2017+ and other databases support this newer syntax
3. **Sequence Caching Strategies**: Performance optimization for high-throughput scenarios
4. **Sequence Monitoring**: Built-in health checks for sequence exhaustion
5. **Multi-Database Sync**: Handling sequences in multi-database scenarios
6. **GraphQL/OData Integration**: How sequences work with modern API patterns

## Conclusion

This ADR proposes a comprehensive approach to unifying sequence operations with FluentMigrator's table and column creation patterns. The solution provides multiple syntax options to accommodate different use cases while maintaining backward compatibility. Database-specific behavior is clearly documented, and workarounds are provided for providers without native sequence support.

The recommended implementation prioritizes developer experience through fluent interfaces while maintaining FluentMigrator's core principles of database portability and explicit migration management.
