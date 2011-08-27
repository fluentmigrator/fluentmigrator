function Increment-NuspecXmlVersion
{
Param(
$XmlFile,
$XPath = "/package"
)

$XmlFile = Join-Path (Get-Item ./) $XmlFile

# Load central xml file (for backup and reading the current version number)
$XmlDoc = New-Object System.Xml.XmlDocument
$XmlDoc.Load($XmlFile)

# Select the correct node
$Node = $XmlDoc.DocumentElement.FirstChild.GetElementsByTagName('version')  | Select-Object -First 1

# Read the current version
$CurrentVersion = $Node.InnerText
$MajorVersion = [Int]($CurrentVersion.Split(".")[0])
$MinorVersion = [Int]($CurrentVersion.Split(".")[1])
$PatchVersion = [Int]($CurrentVersion.Split(".")[2])
Write-Host "Current Version of Package: $CurrentVersion"

# Increment the version
$PatchVersion++
$newVersion = [string]::Concat($MajorVersion,".",$MinorVersion, ".", $PatchVersion)
Write-Host "New Version of Package: $newVersion"

# Write the new version
$Node.InnerText = $newVersion

# Save the personal xml file as the new central xml file
$XmlDoc.Save($XmlFile)	
}

Remove-Item '.\FluentMigrator.NugetConsole\package\bin\*'
# (Get-Project).DTE.ExecuteCommand("Build.SolutionConfigurations", "Release")
# (Get-Project).DTE.ExecuteCommand("Build.BuildSolution")
Invoke-Expression '.\build_nopause.bat'
Copy-Item '.\build\*' '.\FluentMigrator.NugetConsole\package\bin'

Increment-NuspecXmlVersion '.\FluentMigrator.NugetConsole\package\FluentMigrator.NugetConsole.nuspec'
NuGet pack .\FluentMigrator.NugetConsole\package\FluentMigrator.NugetConsole.nuspec
move ".\*.nupkg" "C:\Users\Guillaume\Dropbox\Nuget"