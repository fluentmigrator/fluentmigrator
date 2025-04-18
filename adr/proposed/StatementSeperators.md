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
      <td> Postgres </td>
<td> "dollar quoting" e.g.<br/><br/>

```sql
$function$
BEGIN
 RETURN ($1 ~ $q$[\t\r\n\v\\]$q$);
END;
$function$
```

</td> <td> n/a </td>
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
