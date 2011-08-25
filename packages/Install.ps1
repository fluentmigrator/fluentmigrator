param($installPath, $toolsPath, $package, $project)

$project.ProjectItems | ForEach { if ($_.Name -eq "InstallationDummyFile.txt") { $_.Remove() } }
$projectPath = Split-Path $project.FullName -Parent
Join-Path $projectPath "InstallationDummyFile.txt" | Remove-Item