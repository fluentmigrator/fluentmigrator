require 'albacore'
require './packages/packaging'

task :default => [:build]

msbuild :build do |msb|
  # this doesnt work for me, and it builds fine w/o it. sry if it breaks for you. -josh c
  #msb.path_to_command =  File.join(ENV['windir'], 'Microsoft.NET', 'Framework',  'v4.0.30319', 'MSBuild.exe')
  msb.properties :configuration => :Debug
  msb.targets :Clean, :Rebuild
  msb.verbosity = 'quiet'
  msb.solution = "FluentMigrator (2010).sln"
end

nunit :test => :build do |nunit|
  nunit.command = "tools/NUnit/nunit-console.exe"
  nunit.assemblies "src/FluentMigrator.Tests/bin/Debug/FluentMigrator.Tests.dll"
end

msbuild :release do |msb|
  # this doesnt work for me, and it builds fine w/o it. sry if it breaks for you. -josh c
  #msb.path_to_command =  File.join(ENV['windir'], 'Microsoft.NET', 'Framework',  'v4.0.30319', 'MSBuild.exe')
  msb.properties :configuration => :Release
  msb.targets :Clean, :Rebuild
  msb.verbosity = 'quiet'
  msb.solution = "FluentMigrator (2010).sln"
end
