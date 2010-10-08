require 'albacore'
require 'fileutils'

task :default => [:build]

msbuild :build do |msb|
  msb.path_to_command =  File.join(ENV['windir'], 'Microsoft.NET', 'Framework',  'v4.0.30319', 'MSBuild.exe')
  msb.properties :configuration => :Debug
  msb.targets :Clean, :Rebuild
  msb.solution = "FluentMigrator (2010).sln"
end

nunit :test => :build do |nunit|
  nunit.path_to_command = "tools/NUnit/nunit-console.exe"
  nunit.assemblies "src/FluentMigrator.Tests/bin/Debug/FluentMigrator.Tests.dll"
end

msbuild :release do |msb|
  msb.path_to_command =  File.join(ENV['windir'], 'Microsoft.NET', 'Framework',  'v4.0.30319', 'MSBuild.exe')
  msb.properties :configuration => :Release
  msb.targets :Clean, :Rebuild
  msb.solution = "FluentMigrator (2010).sln"
end

def copy_files(from, to, filename, extensions)
	extensions.each do |ext|
		FileUtils.cp "#{from}#{filename}.#{ext}", "#{to}#{filename}.#{ext}"
	end
end

task :package => :release do
  output_directory = './packages/FluentMigrator/lib/35/'
  FileUtils.mkdir_p output_directory

  copy_files './src/FluentMigrator/bin/Release/', output_directory, 'FluentMigrator', ['dll', 'pdb', 'xml']
  copy_files './src/FluentMigrator.Runner/bin/Release/', output_directory, 'FluentMigrator.Runner', ['dll']
  copy_files './src/FluentMigrator.Console/bin/x86/Release/', output_directory, 'Migrate', ['exe', 'exe.config']
  copy_files './src/FluentMigrator.NAnt/bin/Release/', output_directory, 'FluentMigrator.NAnt', ['dll']
  copy_files './src/FluentMigrator.MSBuild/bin/Release/', output_directory, 'FluentMigrator.MSBuild', ['dll']
end