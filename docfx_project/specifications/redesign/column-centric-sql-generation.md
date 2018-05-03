# Redesign of [IColumn](xref:FluentMigrator.Runner.Generators.IColumn)

# Current state

- Only differentiates between column creation and column
  alteration, but there are two ways to create a column
  and some databases (I'm looking at you, DB2) have a
  different syntax for both.
- Difficult to change the order (some anonymous lambda array)

# Use cases

## Primary use cases

- Column definition in `CREATE TABLE`
- Column definition in `ALTER TABLE ... ADD COLUMN`
- Column definition in `ALTER TABLE ... ALTER COLUMN`

## Secondary use cases

- `PRIMARY KEY` constraint
- `FOREIGN KEY` constraint
- Other constraints (optional)

# How to fix it

Just some random ideas:

- Using SQL fragments
- SQL fragments wrapped in classes
    - Pattern matching

# Open questions

- Maybe different API for all three use cases?
- How to handle the seconday use cases?
