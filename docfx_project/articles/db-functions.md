---
uid: db-functions
title: Database functions
---

# Database functions

It is possible to set the default value when creating or altering a column. To just set a value you can use the following fluent syntax:

```cs
Create.Table("TestTable").WithColumn("Name").AsString().Nullable().WithDefaultValue("test");
```

However, you can take advantage of some database functions to set the default value. The SystemMethods enum contains five database functions:

Function                    | Description
----------------------------|------------------------------------------------
NewGuid                     | Creates a new GUID
NewSequentialId             | Creates a new sequential GUID
CurrentDateTime             | The current (local) timestamp
CurrentDateTimeOffset       | The current (local) timestamp with time zone
CurrentUTCDateTime          | The current (UTC) timestamp
CurrentUser                 | The current user name

These are specific to each database, for example CurrentDateTime calls the GETDATE() function for Sql Server and the now() function for Postgres. By using WithDefault instead of WithDefaultValue, you can pass in one of the enum values.

```cs
Create.Table("TestTable").WithColumn("Created").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentDateTime);
```

# Function/Database support matrix

Server          | NewGuid               | NewSequentialId   | CurrentDateTime   | CurrentDateTimeOffset | CurrentUTCDateTime    | CurrentUser
----------------|-----------------------|-------------------|-------------------|-----------------------|-----------------------|--------------
DB2             | ☐                     | ☐                | ☑                | ☐                     | ☑                    | ☑
Firebird        | ☑                     | ☑ <sup>1</sup>   | ☑                | ☐                     | ☐                    | ☐
Hana            | ☐                     | ☐                | ☑                | ☐                     | ☑                    | ☐
JET             | ☐                     | ☐                | ☐                | ☐                     | ☐                    | ☐
MySQL           | ☑                     | ☑ <sup>1</sup>   | ☑                | ☐                     | ☑                    | ☑
Oracle          | ☑                     | ☑ <sup>1</sup>   | ☑                | ☑                     | ☑                    | ☑
Postgres        | ☑ <sup>2</sup>        | ☑ <sup>2</sup>   | ☑                | ☐                     | ☑                    | ☑
Redshift        | ☐                     | ☐                | ☑                | ☑ <sup>3</sup>        | ☑                    | ☑
SQLite          | ☐                     | ☐                | ☑                | ☐                     | ☑                    | ☐
SQL Anywhere    | ☑                     | ☑ <sup>1</sup>   | ☑                | ☑ <sup>3</sup>        | ☑                    | ☑
SQL Server 2000 | ☑                     | ☑                | ☑                | ☑ <sup>4</sup>        | ☑                    | ☑
SQL Server 2008 | ☑                     | ☑                | ☑                | ☑                     | ☑                    | ☑
SQL Server CE   | ☑                     | ☑                | ☑                | ☑ <sup>4</sup>        | ☑                    | ☑

<sup>1</sup> Is the same as `NewGuid`<br />
<sup>2</sup> uuid-ossp extension is required<br />
<sup>3</sup> Implicitly contains the offset<br />
<sup>4</sup> Same as `CurrentDateTime`<br />
