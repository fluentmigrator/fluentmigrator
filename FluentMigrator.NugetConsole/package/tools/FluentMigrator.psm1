param($migrateToolPath)

function Migrate-Project ([parameter(Mandatory = $true)] [string] $DbType, 
						  [parameter(Mandatory = $true)] [string] $Conn,
						  [string] $Task = 'migrate',
						  [string] $ProjectName,
						  [switch] $Preview) {

	$Project = Get-Project

	if($ProjectName) {
		$Project = Get-Project $ProjectName
	}

	# Set to release
	$Project.DTE.ExecuteCommand("Build.SolutionConfigurations", "Release")

	# Register buildDone event
	$null = Register-ObjectEvent (Get-Project).DTE.Events.BuildEvents OnBuildDone -SourceIdentifier 'FluentMigrator.Console.BuildDone'
	$Project.DTE.ExecuteCommand("Build.BuildSolution")
	
	$null = Wait-Event -SourceIdentifier 'FluentMigrator.Console.BuildDone'
	$null = Unregister-Event 'FluentMigrator.Console.BuildDone'

	# HACK!!! I havn't found a better way
	$error = $Project.DTE.StatusBar.Text.Contains('Failed')

	if($error){
		Write-Host 'Build Failed'
		return
	}

	# Get dll path
	$projectPath = ((Get-Item $Project.FullName).Directory).FullName
	$dllFolderPath = Join-Path $projectPath ($Project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value)
	$dllFileName = $Project.Properties.Item("AssemblyName").Value + '.dll'
	$dllFilePath = Join-Path $dllFolderPath $dllFileName

	$migrateCommandLine = "& ""$migrateToolPath"" -a '$dllFilePath' -db $DbType -c '$Conn' -t $Task"
	if($Preview){
		$migrateCommandLine = $migrateCommandLine + ' -p -verbose true'
	}
	Invoke-Expression $migrateCommandLine
	$Project.DTE.StatusBar.Text = 'Migration completed'
}

Register-TabExpansion "Migrate-Project" @{
	'DbType' = {
		param($context)
		return 'jet', 'mysql', 'orable', 'postgres', 'sqlite', 'sqlserver2000', 'sqlserver2005', 'sqlserver2008', 'sqlserverce', 'sqlserver'
	}
	'Task' = {
		return 'migrate', 'migrate:up', 'migrate:down', 'rollback', 'rollback:toversion', 'rollback:all'		
	}
	'ProjectName' = {
		param($context)
		return Get-Project -All | Select-Object -ExpandProperty 'ProjectName'
	}
}

Export-ModuleMember Migrate-Project