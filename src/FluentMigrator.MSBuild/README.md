# msbuild runner

You cannot use this runner in a development environment, so something
like the following doesn't work anymore:

```xml
<UsingTask TaskName="FluentMigrator.MSBuild.Migrate" AssemblyFile="$(MSBuildProjectDirectory)\..\..\src\FluentMigrator.MSBuild\bin\$(Configuration)\$(TargetFramework)\FluentMigrator.MSBuild.dll" />
<Target Name="Migration" AfterTargets="AfterBuild">
  <Message Text="Starting FluentMigrator Migration" Importance="high" />
  <Migrate DatabaseType="SQLite" Connection="Data Source=:memory:" Verbose="True" Target="$(MSBuildProjectDirectory)\$(OutDir)FluentMigrator.Example.Migrations.dll" />
</Target>
```

The main reason for this restriction is, that the runner isn't able to
find the necessary assemblies for - for example - the ADO.NET provider
when using the .NET Core tooling.

The problem with the .NET Core tooling and the new-style package references
is, that the package assemblies aren't copied to the output directory
anymore during a build.

The assemblies are only available after a `dotnet publish`.
