# Overview

Every SQL dialect has a way to separate:

1. Statements
2. Batches

In most SQL dialects, the statement separator is a semi-colon (';'). In some SQL dialects, the statement separator can be dynamically configured.

Further, some SQL dialects have the concept of _statement batches_. In these dialects, normally there are also "batch statement rules", such as:

1. Some special statements must be the first statement in the batch, like `CREATE TABLE` statements.
2. Some statements within the batch must be explicitly separated with a statement separator, like T-SQL Common Table Expressions. e.g., `;WITH CTE AS ( /* some sql */ ) SELECT * FROM CTE`

# Database-Specific Statement Separators

Below is a list of statement separators for every database FluentMigrator supports:

<table>
  <thead>
    <tr>
      <th> Database Type </th> <th> Default </th> <th> Override </th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td> MySQL </td> <td> <code>;</code> </td>
<td>

```sql
DELIMITER <char sequence>
```

</td>
    </tr>
    <tr>
      <td> Postgres </td> <td> <code>;</code> </td>
<td>

PostgreSQL itself does not provide a built-in way to override the semicolon (;) as the statement terminator within its SQL syntax. The semicolon is a fundamental part of SQL syntax in PostgreSQL, and there is no direct configuration or command to change this behavior within the database server.<br />

However, if you are using the psql command-line interface to interact with PostgreSQL, there is a way to change the statement terminator temporarily. You can use the \set command to redefine the statement terminator. For example: <br />

```
\set ECHO_HIDDEN 1  
\set myterminator '^'  
```

Then you can use `\;` to execute the commands with the new terminator:<br />

```
SELECT * FROM users\myterminator  
INSERT INTO users (name, email) VALUES ('John Doe', 'john.doe@example.com')\myterminator  
```

Note that this method is specific to the psql interactive terminal and does not affect how statements are terminated in SQL scripts or other PostgreSQL clients and tools.<br />

In other PostgreSQL clients or when running SQL scripts directly, you will need to use the standard semicolon terminator. If you need to handle complex scripts with alternative terminators, you might need to preprocess the scripts with a custom parser or use a tool that supports such functionality.

</td>
    </tr>
    <tr>
      <td> Firebird </td> <td> <code>;</code> </td>
<td>

```sql
SET TERM <character>
```

</td>
    </tr>
    <tr>
      <td> DB2 </td> <td> <code>;</code> </td>
<td>

```sql
--#SET TERMINATOR <phrase>
```

</td>
    </tr>
    <tr>
      <td> Oracle (SQLPlus) </td> <td> n/a </td>
<td>

```sql
set cmdsep <phrase>
```

</td>
    </tr>
    <tr>
      <td> Hana (HDBSQL) </td> <td> <code>;</code> </td>
<td>

```sql
set cmdsep <phrase>
```

</td>
    </tr>
    <tr>
      <td> T-SQL (Microsoft SQL Server) </td> <td> <code>;</code> </td>
<td>

Special _statement batch_ separator supported by `sqlcmd.exe`:

```sql
GO
```

</td>
    </tr>
    <tr>
      <td> Redshift </td> <td> <code>;</code> </td>
<td>

n/a

</td>
    </tr>
  </tbody>
</table>

# Notes on Postgres

In addition to semi-colon statement separator, Postgres also supports "dollar quoting" ($$).  Dollar-quoting is used to define string constants or code blocks.

```sql
$function$
BEGIN
 RETURN ($1 ~ $q$[\t\r\n\v\\]$q$);
END;
$function$
```
