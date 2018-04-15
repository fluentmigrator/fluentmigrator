# Integration tests

## How to set connection strings

```
dotnet user-secrets set "TestConnectionStrings:SqlServer2016:ConnectionString" "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=FluentMigrator;Integrated Security=true"
dotnet user-secrets set "TestConnectionStrings:SqlServer2016:IsEnabled" "true"
```
