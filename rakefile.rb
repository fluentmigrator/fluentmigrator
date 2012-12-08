require 'albacore'
require 'version_bumper'
require './packages/packaging'

task :default => [:build]

desc "Build the project as debug"
task :build => 'build:debug'

directory 'dist'

namespace :build do
  
  desc "create solutioninfo.cs file"
  assemblyinfo :solutioninfo do |asm|
    asm.version = bumper_version.to_s
    asm.file_version = bumper_version.to_s
    asm.product_name = "FluentMigrator"
    asm.copyright = "Copyright - Sean Chambers 2008-" + Time.now.year.to_s
    asm.custom_attributes :AssemblyConfigurationAttribute => "Debug"
    asm.output_file = "src/SolutionInfo.cs"
  end

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
	msb.properties :configuration => :Release
    msb.targets :Clean, :Rebuild
    msb.verbosity = 'quiet'
    msb.solution = "FluentMigrator (2010).sln"
  end
  
  @platforms = ['x86', 'AnyCPU']
  @versions = ['v3.5', 'v4.0']
  @platforms.each do |p|
    @versions.each do |v|
      
      directory "dist/console-#{v}-#{p}"
      
      desc "build the console app for target .NET Framework version ${v}"
      task "console-#{v}-#{p}" => [:release, "compile-console-#{v}-#{p}", "dist/console-#{v}-#{p}"] do
	    cp_r FileList['lib/Postgres/*', 'lib/MySql.Data.dll', 'lib/Oracle.DataAccess.dll', 'lib/System.Data.SQLite.dll', 'lib/SQLServerCE4/Private/*', 'lib/FirebirdSql.Data.FirebirdClient.dll'], "dist/console-#{v}-#{p}"
        cp_r FileList['src/FluentMigrator.Console/bin/Release/*'].exclude('src/FluentMigrator.Console/bin/Release/SQLServerCENative'), "dist/console-#{v}-#{p}"
        cp_r FileList['src/FluentMigrator.Nant/bin/Release/FluentMigrator.Nant.*'], "dist/console-#{v}-#{p}"
        cp_r FileList['src/FluentMigrator.MSBuild/bin/Release/FluentMigrator.MSBuild.*'], "dist/console-#{v}-#{p}"
		
        if to_nuget_version(v) == '35' then 
          File.delete("dist/console-#{v}-#{p}/Migrate.exe.config")
          File.rename("dist/console-#{v}-#{p}/app.35.config", "dist/console-#{v}-#{p}/Migrate.exe.config")
        else
          File.delete("dist/console-#{v}-#{p}/app.35.config")
        end
      end
      
      msbuild "compile-console-#{v}-#{p}" do |msb|
        msb.properties :configuration => :Release, :TargetFrameworkVersion => v, :PlatformTarget => p 
        msb.targets :Clean, :Rebuild
        msb.verbosity = 'quiet'
        msb.solution = 'FluentMigrator (2010).sln'
      end
      
    end
  end
  
  # FYI: `Array.product` will only work in ruby 1.9
  desc "compile the console runner for all x86/64/4.0/3.5 combinations"
  task :console => @platforms.product(@versions).map {|x| "console-#{x[1]}-#{x[0]}"}
  
end

nunit :test => :build do |nunit|
  nunit.command = "tools/NUnit/nunit-console.exe"
  nunit.assemblies "src/FluentMigrator.Tests/bin/Debug/FluentMigrator.Tests.dll"
end
  
