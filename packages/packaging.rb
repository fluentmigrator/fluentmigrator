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

def copy_files(from, to, filename, extensions)
	extensions.each do |ext|
		FileUtils.cp "#{from}#{filename}.#{ext}", "#{to}#{filename}.#{ext}"
	end
end

task :prepare_package => :release do
  output_directory_lib = './packages/FluentMigrator/lib/35/'
  output_directory_tools = './packages/FluentMigrator/tools/'
  FileUtils.mkdir_p output_directory_lib
  FileUtils.mkdir_p output_directory_tools

  copy_files './src/FluentMigrator/bin/Release/', output_directory_lib, 'FluentMigrator', ['dll', 'pdb', 'xml']

  copy_files './lib/', output_directory_tools, 'MySql.Data', ['dll']
  copy_files './lib/', output_directory_tools, 'System.Data.SQLite', ['dll']
  copy_files './src/FluentMigrator.Runner/bin/Release/', output_directory_tools, 'FluentMigrator.Runner', ['dll']
  copy_files './src/FluentMigrator.Console/bin/x86/Release/', output_directory_tools, 'Migrate', ['exe']
  copy_files './src/FluentMigrator.Console/bin/x86/Release/', output_directory_tools, 'Migrate', ['exe.config']
  copy_files './src/FluentMigrator.Nant/bin/Release/', output_directory_tools, 'FluentMigrator.NAnt', ['dll']
  copy_files './src/FluentMigrator.MSBuild/bin/Release/', output_directory_tools, 'FluentMigrator.MSBuild', ['dll']
end

exec :package => :prepare_package do |cmd|
  cmd.path_to_command = 'tools/NuPack.exe'
  cmd.parameters [
    'packages\\FluentMigrator\\FluentMigrator.nuspec',
    'packages\\FluentMigrator'
  ]
end