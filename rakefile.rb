require 'albacore'
require './packages/packaging'

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
