param($rootPath, $toolsPath, $package, $project)

$migrateToolPath = (Join-Path $rootPath "bin\Migrate.exe")

Import-Module (Join-Path $toolsPath "FluentMigrator.psm1") -ArgumentList $migrateToolPath -DisableNameChecking -Force