<#
===================================================================================================================
 IBM LICENSE TERMS

 BY RUNNING THIS SCRIPT, YOU AGREE TO IBM'S LICENSE TERMS FOR THE FOLLOWING IBM PROGRAMS, WHICH IT DOWNLOADS AND RUNS:

 - IBM Db2 Community Edition, in the container image icr.io/db2_community/db2. The script starts it with
   LICENSE=accept, which is how the image records that you accept its license.
 - IBM Data Server Provider for .NET (the Net.IBM.Data.Db2 or Net.IBM.Data.Db2-lnx NuGet package, including the
   IBM Data Server Driver, clidriver), which the tests use to connect. Its terms are in Lic_en.txt in the package
   (L/N L-ZRDX-CGZHUA), with the International Program License Agreement (i125-3301-15).

 If you do not agree to these terms, do not run this script. If you run it on behalf of an employer or another
 organization, make sure you may accept them for it.
===================================================================================================================
#>

<#
.SYNOPSIS
    Runs FluentMigrator's Db2 integration tests on this machine, against Db2 Community Edition in a container.

.DESCRIPTION
    Db2 is not part of the build server's default test run, and some Db2 tests, such as Db2DescriptionTests, skip
    themselves on a build server. This script runs them on a developer machine:

    1. It sets FM_TestConnectionStrings__DB2__IsEnabled and FM_TestConnectionStrings__DB2__ContainerEnabled, so the
       test assembly's setup (IntegrationTestsSetup) starts Db2 with Testcontainers (Db2Container), which accepts the
       image's license.
    2. It builds the test project, and on Linux points LD_LIBRARY_PATH at the clidriver the build copies, as the build
       server does.
    3. It runs dotnet test with -Filter.

    Requirements: Docker running Linux containers, with room for a privileged container of several GB; the first run
    downloads the image. On Linux, IBM's driver needs libxml2.so.2. The .NET SDK in global.json.

.PARAMETER Filter
    The dotnet test filter. By default the Db2 processor integration tests and the creation-script tests, whose other
    databases are skipped unless enabled.

.PARAMETER Framework
    The target framework to test.

.PARAMETER Configuration
    The build configuration.

.PARAMETER TestArguments
    More arguments for dotnet test, for example a logger.

.PARAMETER NewContainer
    Removes the Db2 container of an earlier run first. Testcontainers reuses it, and a restarted Db2 can take
    connections before it is ready, which fails the tests with SQL30081N.

.EXAMPLE
    ./test/FluentMigrator.Tests/Invoke-Db2Tests.ps1

.EXAMPLE
    ./test/FluentMigrator.Tests/Invoke-Db2Tests.ps1 -Filter 'FullyQualifiedName~Db2DescriptionTests'

.EXAMPLE
    ./test/FluentMigrator.Tests/Invoke-Db2Tests.ps1 -TestArguments '--logger', 'trx', '--results-directory', 'TestResults-Db2'
#>
[CmdletBinding()]
param(
    [string] $Filter = 'FullyQualifiedName~.Integration.Processors.Db2.|FullyQualifiedName~.Integration.CreationScript.',

    [string] $Framework = 'net8.0',

    [string] $Configuration = 'Debug',

    [string[]] $TestArguments = @(),

    [switch] $NewContainer
)

$ErrorActionPreference = 'Stop'

# `pwsh -File script.ps1 -TestArguments --logger,trx` passes a single string; accept that too.
$TestArguments = @($TestArguments | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })

Write-Warning (@(
        'By running this script, you have agreed to the IBM license terms for Db2 Community Edition and for the IBM'
        'Data Server Provider for .NET, as set out at the top of this script. Press Ctrl+C now if you do not agree.'
    ) -join ' ')

$project = Join-Path $PSScriptRoot 'FluentMigrator.Tests.csproj'

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker was not found. Testcontainers needs Docker running Linux containers to start Db2.'
}

$dockerOs = & docker info --format '{{.OSType}}' 2>$null
if ($LASTEXITCODE -ne 0) {
    throw 'Docker is not running, or this user cannot reach it.'
}

if ($dockerOs -ne 'linux') {
    throw "Docker runs $dockerOs containers. Db2's image needs Linux containers."
}

if ($IsLinux) {
    $libxml2 = & ldconfig -p 2>$null | Select-String -SimpleMatch 'libxml2.so.2 ' | Select-Object -First 1
    if (-not $libxml2) {
        throw "IBM's Db2 driver needs libxml2.so.2, which this machine doesn't have. Newer distributions ship only libxml2.so.16; the build server stays on Ubuntu 24.04 for this reason (see runs-on in .github/workflows/pull-requests.yaml)."
    }
}

if ($NewContainer) {
    # Only the container Db2Container labels, so another Db2 on this machine, and its data, is left alone.
    $containers = & docker ps --all --quiet --filter 'label=org.fluentmigrator.tests=db2'
    if ($containers) {
        & docker rm --force @containers | Out-Null
    }
}

$env:FM_TestConnectionStrings__DB2__IsEnabled = 'true'
$env:FM_TestConnectionStrings__DB2__ContainerEnabled = 'true'

& dotnet build $project --framework $Framework --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

# Net.IBM.Data.Db2-lnx copies IBM's native client (clidriver) next to the tests. Its provider loads libdb2.so from
# clidriver/lib, which the loader doesn't search unless told to.
$clidriver = Join-Path $PSScriptRoot "bin/$Configuration/$Framework/clidriver"
if ($IsLinux) {
    $lib = Join-Path $clidriver 'lib'
    $env:LD_LIBRARY_PATH = if ($env:LD_LIBRARY_PATH) { "$lib$([IO.Path]::PathSeparator)$env:LD_LIBRARY_PATH" } else { $lib }
}
elseif ($IsMacOS) {
    $lib = Join-Path $clidriver 'lib'
    $env:DYLD_LIBRARY_PATH = if ($env:DYLD_LIBRARY_PATH) { "$lib$([IO.Path]::PathSeparator)$env:DYLD_LIBRARY_PATH" } else { $lib }
}
else {
    $bin = Join-Path $clidriver 'bin'
    if (Test-Path -LiteralPath $bin) {
        $env:PATH = "$bin$([IO.Path]::PathSeparator)$env:PATH"
    }
}

& dotnet test $project --no-build --framework $Framework --configuration $Configuration --filter $Filter @TestArguments
exit $LASTEXITCODE
