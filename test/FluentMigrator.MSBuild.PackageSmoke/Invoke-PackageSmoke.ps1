<#
.SYNOPSIS
    End-to-end check that a packed FluentMigrator.MSBuild.nupkg's auto-imported targets load the Migrate task.

.DESCRIPTION
    For each consumer target framework, creates a throwaway project outside the repository that references the
    package, restores it from a local feed, and runs a target that invokes <Migrate>. NuGet imports
    build/<tfm>/FluentMigrator.MSBuild.targets, whose UsingTask must point at tasks/<tfm>/ in the package root.

    The check passes when MSBuild loads the task from tasks/<expected tfm>/FluentMigrator.MSBuild.dll and reports
    none of MSB4062 (task could not be loaded), MSB4036 (task not found) or MSB4026. The Migrate call itself is
    expected to fail (it names a database id that does not exist); a failure inside the task proves it ran.

    Every release up to 8.x failed this check: the targets resolved to build/tasks/<tfm>, which does not exist.
    The static counterpart is MSBuildPackageLayoutTests in FluentMigrator.Tests.

.PARAMETER PackagePath
    Path to FluentMigrator.MSBuild.<version>.nupkg.

.PARAMETER TargetFrameworks
    Consumer target frameworks to check with the .NET SDK's MSBuild (dotnet msbuild).

.PARAMETER FullFramework
    Also check a net48 consumer with Visual Studio's MSBuild.exe (Windows only; skipped with a warning if
    vswhere cannot find it).

.EXAMPLE
    ./Invoke-PackageSmoke.ps1 -PackagePath src/FluentMigrator.MSBuild/bin/Release/FluentMigrator.MSBuild.9.0.0.nupkg -FullFramework
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $PackagePath,

    [string[]] $TargetFrameworks = @('net8.0', 'net9.0', 'net10.0', 'net11.0'),

    [switch] $FullFramework
)

$ErrorActionPreference = 'Stop'

# `pwsh -File script.ps1 -TargetFrameworks net8.0,net9.0` passes a single string; accept that too.
$TargetFrameworks = $TargetFrameworks | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ }

$package = Get-Item -LiteralPath $PackagePath
if ($package.Name -notmatch '^FluentMigrator\.MSBuild\.(?<version>\d+\.\d+\.\d+.*)\.nupkg$') {
    throw "Expected FluentMigrator.MSBuild.<version>.nupkg, got '$($package.Name)'."
}
$version = $Matches.version

$work = Join-Path ([IO.Path]::GetTempPath()) ('fm-msbuild-smoke-' + [guid]::NewGuid().ToString('N'))
$feed = Join-Path $work 'feed'
$packages = Join-Path $work 'packages'
New-Item -ItemType Directory -Force -Path $feed | Out-Null
Copy-Item -LiteralPath $package.FullName -Destination $feed

# Stop MSBuild and NuGet from picking up anything above the temp folder.
'<Project />' | Set-Content -Path (Join-Path $work 'Directory.Build.props')
'<Project />' | Set-Content -Path (Join-Path $work 'Directory.Build.targets')
'<Project />' | Set-Content -Path (Join-Path $work 'Directory.Packages.props')
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <config>
    <add key="globalPackagesFolder" value="$packages" />
  </config>
  <packageSources>
    <clear />
    <add key="local" value="$feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local">
      <package pattern="FluentMigrator.MSBuild" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
"@ | Set-Content -Path (Join-Path $work 'nuget.config')

function New-Consumer([string] $targetFramework) {
    $directory = Join-Path $work $targetFramework
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    $project = Join-Path $directory 'Consumer.csproj'
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$targetFramework</TargetFramework>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FluentMigrator.MSBuild" Version="$version" />
  </ItemGroup>
  <Target Name="SmokeMigrate">
    <Migrate Database="fm-smoke-no-such-database" Connection="Data Source=smoke" Target="`$(MSBuildProjectFullPath)" />
  </Target>
</Project>
"@ | Set-Content -Path $project
    return $project
}

function Test-Output([string] $label, [string] $taskFramework, [string[]] $output) {
    $text = $output -join "`n"
    $expectedSuffix = [IO.Path]::Combine('tasks', $taskFramework, 'FluentMigrator.MSBuild.dll')
    $loadedFrom = [regex]::Matches($text, 'Using "Migrate" task from assembly "(?<path>[^"]+)"') |
        ForEach-Object { $_.Groups['path'].Value } | Select-Object -First 1
    $loadErrors = [regex]::Matches($text, 'MSB4062|MSB4036|MSB4026') | ForEach-Object { $_.Value } | Sort-Object -Unique

    $problems = @()
    if (-not $loadedFrom) {
        $problems += 'MSBuild never resolved the Migrate task.'
    } elseif (-not ([IO.Path]::GetFullPath($loadedFrom)).EndsWith($expectedSuffix, [StringComparison]::OrdinalIgnoreCase)) {
        $problems += "task resolved to '$loadedFrom', expected ...$expectedSuffix"
    }
    if ($loadErrors) {
        $problems += "task load errors: $($loadErrors -join ', ')"
    }

    if ($problems.Count -eq 0) {
        Write-Host "PASS  $label -> $loadedFrom"
        return $true
    }

    Write-Host "FAIL  $label"
    $problems | ForEach-Object { Write-Host "      $_" }
    $output | Select-String -Pattern 'error' | Select-Object -First 5 | ForEach-Object { Write-Host "      $($_.Line.Trim())" }
    return $false
}

$failures = 0

foreach ($targetFramework in $TargetFrameworks) {
    $project = New-Consumer $targetFramework
    $restore = & dotnet restore $project --configfile (Join-Path $work 'nuget.config') 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAIL  dotnet msbuild, $targetFramework consumer: restore failed"
        $restore | Select-Object -Last 10 | ForEach-Object { Write-Host "      $_" }
        $failures++
        continue
    }

    $output = & dotnet msbuild $project -t:SmokeMigrate -v:d -nologo 2>&1 | ForEach-Object { "$_" }
    if (-not (Test-Output "dotnet msbuild, $targetFramework consumer" $targetFramework $output)) {
        $failures++
    }
}

if ($FullFramework) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = if (Test-Path $vswhere) {
        & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    }

    if (-not $msbuild) {
        Write-Warning 'Visual Studio MSBuild.exe not found; skipping the .NET Framework MSBuild check.'
    } else {
        $project = New-Consumer 'net48'
        & $msbuild $project -t:Restore "-p:RestoreConfigFile=$(Join-Path $work 'nuget.config')" -v:q -nologo | Out-Null
        $output = & $msbuild $project -t:SmokeMigrate -v:d -nologo 2>&1 | ForEach-Object { "$_" }
        if (-not (Test-Output 'MSBuild.exe, net48 consumer' 'net48' $output)) {
            $failures++
        }
    }
}

Remove-Item -Recurse -Force -LiteralPath $work -ErrorAction SilentlyContinue

if ($failures -gt 0) {
    Write-Host "$failures FluentMigrator.MSBuild package smoke check(s) failed."
    exit 1
}

Write-Host 'All FluentMigrator.MSBuild package smoke checks passed.'

# Exit explicitly: the last native command was a <Migrate> run that fails by design, and callers such as
# GitHub Actions' pwsh shell otherwise propagate its $LASTEXITCODE.
exit 0
