require 'albacore'
require './packages/packaging'

task :default => [:build]

desc "Build the project as debug"
task :build => 'build:debug'

directory 'dist'

namespace :build do
  
  msbuild :debug do |msb|
    # this doesnt work for me, and it builds fine w/o it. sry if it breaks for you. -josh c
    # to josh c, Please upgrade your Albacore. --tkellogg
    #msb.path_to_command =  File.join(ENV['windir'], 'Microsoft.NET', 'Framework',  'v4.0.30319', 'MSBuild.exe')
    msb.properties :configuration => :Debug
    msb.targets :Clean, :Rebuild
    msb.verbosity = 'quiet'
    msb.solution = "FluentMigrator (2010).sln"
  end
  
  desc "build the release version of the solution"
  msbuild :release do |msb|
    # this doesnt work for me, and it builds fine w/o it. sry if it breaks for you. -josh c
    #msb.path_to_command =  File.join(ENV['windir'], 'Microsoft.NET', 'Framework',  'v4.0.30319', 'MSBuild.exe')
	msb.properties :configuration => :Release, :TargetFrameworkVersion => 'v3.5'
    msb.targets :Clean, :Rebuild
    msb.verbosity = 'quiet'
    msb.solution = "FluentMigrator (2010).sln"
  end
  
  @versions = ['v3.5', 'v4.0']
  @versions.each do |v|
    
    directory "dist/console-#{v}"
    
    desc "build the console app for target .NET Framework version ${v}"
    task "console-#{v}" => [:release, "compile-console-#{v}", "dist/console-#{v}"] do
      cp_r FileList['src/FluentMigrator.Console/bin/Release/*'], "dist/console-#{v}"
    end
    
    msbuild "compile-console-#{v}" do |msb|
      msb.properties :configuration => :Release, :TargetFrameworkVersion => v 
      msb.targets :Clean, :Rebuild
      msb.verbosity = 'quiet'
      msb.solution = 'src/FluentMigrator.Console/FluentMigrator.Console.csproj'
    end
    
  end
  
  task :console => @versions.map {|v| "console-#{v}"}
  
end

nunit :test => :build do |nunit|
  nunit.command = "tools/NUnit/nunit-console.exe"
  nunit.assemblies "src/FluentMigrator.Tests/bin/Debug/FluentMigrator.Tests.dll"
end
  
