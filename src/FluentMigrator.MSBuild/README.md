# msbuild runner

A sample is provided in the sample folder.

When using the MSBuild runner, be careful with whether you are using MSBuild for .NET Framework (which ships as part of Visual Studio still) and `dotnet.exe msbuild` (which ships as part of .NET and is the newer version of MSBuild).

If you get an error like the following, then you are most likely using the wrong version of MSBuild to run your target with the `FluentMigrator.MSBuild.Migrate` custom task: `Could not load file or assembly 'System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies. The system cannot find the file specified. Confirm that the <UsingTask> declaration is correct, that the assembly and all its dependencies are available, and that the task contains a public class that implements Microsoft.Build.Framework.ITask.`