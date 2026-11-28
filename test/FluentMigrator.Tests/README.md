# Integration tests

## How to set connection strings

```
dotnet user-secrets set "TestConnectionStrings:SqlServer2016:ConnectionString" "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=FluentMigrator;Integrated Security=true"
dotnet user-secrets set "TestConnectionStrings:SqlServer2016:IsEnabled" "true"
```

## Databases in containers

With `ContainerEnabled` also set, the tests start the database with Testcontainers instead of using the connection
string's server. This is how the build server runs SQL Server, PostgreSQL, MySQL, Oracle and Firebird:

```
FM_TestConnectionStrings__Postgres__IsEnabled=true
FM_TestConnectionStrings__Postgres__ContainerEnabled=true
```

## Db2

Db2 runs on a developer machine, with [`Invoke-Db2Tests.ps1`](Invoke-Db2Tests.ps1). Some Db2 tests skip themselves on
a build server. **Running the script means agreeing to IBM's license terms** for Db2 Community Edition and the IBM
Data Server Provider for .NET, as set out at the top of the script.

```powershell
./test/FluentMigrator.Tests/Invoke-Db2Tests.ps1
```
