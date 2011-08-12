require 'fileutils'

#Unfortunately SQLite cannot be ILMerged in because it is not 100% managed.
#exec :ilmerge_runner_with_providers => :release do |cmd|
#  FileUtils.mkdir_p 'il_merged'
#  cmd.path_to_command = "tools/ILMerge.exe"
#  cmd.parameters [
#  	'/target:library',
#  	'/out:il_merged\\FluentMigrator.Runner.dll',
#  	'/zeroPeKind', # necessary for SQLLite, which isn't managed. might not work though...
#  	'src\\FluentMigrator\\bin\\Release\\FluentMigrator.dll',
#  	'src\\FluentMigrator.Runner\\bin\\Release\\FluentMigrator.Runner.dll',
#  	'lib\\MySql.Data.dll',
#  	'lib\\System.Data.SQLite.DLL'
#  ]
#end

FLUENTMIGRATOR_VERSION = "0.9.2.0"

def copy_files(from, to, filename, extensions)
	extensions.each do |ext|
		FileUtils.cp "#{from}#{filename}.#{ext}", "#{to}#{filename}.#{ext}"
	end
end

def copy_rename_files(from, to, orig_filename, renamed_filename, extensions)
	extensions.each do |ext|
		FileUtils.cp "#{from}#{orig_filename}.#{ext}", "#{to}#{renamed_filename}.#{ext}"
	end
end

desc "create the nuspec file"
nuspec :create_spec do |nuspec|
   version = "#{ENV['version']}"

   nuspec.id = "FluentMigrator"
   nuspec.version = version.length == 7 ? version : FLUENTMIGRATOR_VERSION
   nuspec.authors = "Josh Coffman"
   nuspec.owners = "Sean Chambers"
   nuspec.description = "FluentMigrator is a database migration framework for .NET written in C#. The basic idea is that you can create migrations which are simply classes that derive from the Migration base class and have a Migration attribute with a unique version number attached to them. Upon executing FluentMigrator, you tell it which version to migrate to and it will run all necessary migrations in order to bring your database up to that version.
In addition to forward migration support, FluentMigrator also supports different ways to execute the migrations along with selective migrations called profiles and executing arbitrary SQL."
   nuspec.title = "Fluent Migrator"
   nuspec.language = "en-US"
   nuspec.projectUrl = "https://github.com/schambers/fluentmigrator/wiki/"
   nuspec.working_directory = "packages/FluentMigrator"
   nuspec.output_file = "FluentMigrator.nuspec"
end

task :prepare_nuget_package => ['build:release', :create_spec] do
  output_directory_lib = './packages/FluentMigrator/lib/35/'
  output_directory_tools = './packages/FluentMigrator/tools/'
  
  FileUtils.rm_rf output_directory_lib
  FileUtils.rm_rf output_directory_tools
  FileUtils.mkdir_p output_directory_lib
  FileUtils.mkdir_p output_directory_tools

  copy_files './src/FluentMigrator/bin/Release/', output_directory_lib, 'FluentMigrator', ['dll', 'pdb', 'xml']
  copy_files './src/FluentMigrator/bin/Release/', output_directory_tools, 'FluentMigrator', ['dll', 'pdb', 'xml']
  copy_files './src/FluentMigrator.Runner/bin/Release/', output_directory_tools, 'FluentMigrator.Runner', ['dll', 'pdb']
  copy_files './src/FluentMigrator.Console/bin/Release/', output_directory_tools, 'Migrate', ['exe', 'pdb', 'exe.config']
  copy_files './src/FluentMigrator.Nant/bin/Release/', output_directory_tools, 'FluentMigrator.NAnt', ['dll', 'pdb']
  copy_files './src/FluentMigrator.MSBuild/bin/Release/', output_directory_tools, 'FluentMigrator.MSBuild', ['dll', 'pdb']
  
  copy_files './lib/', output_directory_tools, 'MySql.Data', ['dll']
  copy_files './lib/', output_directory_tools, 'System.Data.SQLite', ['dll']
  copy_files './lib/SQLServerCE4/Private/', output_directory_tools, 'System.Data.SqlServerCe', ['dll']
  copy_files './lib/Postgres/', output_directory_tools, 'Npgsql', ['dll', 'pdb', 'xml']
  copy_files './lib/Postgres/', output_directory_tools, 'Mono.Security', ['dll']
  
  output_directory_tools_SQLServerCENative = './packages/FluentMigrator/tools/SQLServerCENative/'
  FileUtils.mkdir_p output_directory_tools_SQLServerCENative
  FileUtils.cp_r './lib/SQLServerCE4/Private/amd64/', output_directory_tools_SQLServerCENative
  FileUtils.cp_r './lib/SQLServerCE4/Private/x86/', output_directory_tools_SQLServerCENative
  
  Rake::Task['build:console-v4.0'].invoke
  copy_rename_files './dist/console-v4.0/', output_directory_tools, 'Migrate', 'Migrate40', ['exe', 'pdb', 'exe.config']
end

exec :nuget_package => :prepare_nuget_package do |cmd|
  base_folder = 'packages/FluentMigrator/'
  output = 'nuget/'
  cmd.command = 'tools/NuGet.exe'
  cmd.parameters = "pack packages/FluentMigrator/FluentMigrator.nuspec -basepath #{base_folder} -outputdirectory #{output}"
end