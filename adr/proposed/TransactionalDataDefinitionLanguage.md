See also: https://wiki.postgresql.org/wiki/Transactional_DDL_in_PostgreSQL:_A_Competitive_Analysis

# ATOMIC CREATE

| Database   | Non-Recoverable Operations               | TABLE | VIEW | FUNCTION | TYPE |
| ---------- | ---------------------------------------- | ----- | ---- | -------- | ---- |
| Postgres   | Database, Tablespace, Catalog operations | ✔️    | ✔️  | ✔️       | ✔️  |
| SQL Server | Database, Tablespace, Catalog operations | ✔️    | ❌ | ✔️ | ❌ |



# ATOMIC ALTER

[Postgres `ALTER TYPE`](https://www.postgresql.org/docs/9.1/sql-altertype.html)

> `ALTER TYPE ... ADD VALUE` (the form that adds a new value to an enum type) cannot be executed inside a transaction block.
