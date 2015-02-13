require 'albacore'
require 'version_bumper'
require './packages/packaging'

task :default => [:build]

desc "Build the project as debug"
task :build => 'build:debug'

directory 'dist'

namespace :build do

  desc "create solutioninfo.cs file"
  asmver :solutioninfo do |asm|
    asm.file_path = "src/SolutionInfo.cs"

    asm.attributes  assembly_version: bumper_version.to_s,
                    assembly_file_version: bumper_version.to_s,
                    assembly_product: "FluentMigrator",
                    assembly_copyright: "Copyright - Sean Chambers 2008-" + Time.now.year.to_s,
                    assembly_configuration_attribute: "Debug",
                    CLSCompliant: true
    asm.using 'System'
  end

  build :debug do |msb|
    msb.prop 'Configuration', 'Debug'
    msb.target = ['Clean', 'Rebuild']
    msb.be_quiet
    msb.sln = "FluentMigrator.sln"
  end

  desc "build the release version of the solution"
  build :release do |msb|
    msb.prop 'Configuration', 'Release'
    msb.target = ['Clean', 'Rebuild']
    msb.be_quiet
    msb.sln = "FluentMigrator.sln"
  end

  @platforms = ['x86', 'AnyCPU']
  @versions = ['v3.5', 'v4.0']
  @platforms.each do |p|
    @versions.each do |v|

      directory "dist/console-#{v}-#{p}"

      desc "build the console app for target .NET Framework version ${v}"
      task "console-#{v}-#{p}" => [:release, "compile-console-#{v}-#{p}", "dist/console-#{v}-#{p}"] do
	    cp_r FileList['lib/Postgres/*', 'lib/MySql.Data.dll', 'lib/Oracle.DataAccess.dll', 'lib/System.Data.SQLite.dll', 'lib/SQLServerCE4/Private/*', 'lib/FirebirdSql.Data.FirebirdClient.dll', 'lib/Oracle.ManagedDataAccess.dll'], "dist/console-#{v}-#{p}"
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

      build "compile-console-#{v}-#{p}" do |msb|
        msb.prop 'Configuration', 'Release'
        msb.prop 'TargetFrameworkVersion', v
        msb.prop 'PlatformTarget', p
        msb.target = ['Clean', 'Rebuild']
        msb.be_quiet
        msb.sln = "FluentMigrator.sln"
      end

    end
  end

  # FYI: `Array.product` will only work in ruby 1.9
  desc "compile the console runner for all x86/64/4.0/3.5 combinations"
  task :console => @platforms.product(@versions).map {|x| "console-#{x[1]}-#{x[0]}"}

end

desc 'run the unit tests'
test_runner :test do |tests|
  tests.files = FileList['**/src/FluentMigrator.Tests/bin/Debug/FluentMigrator.Tests.dll'] # dll files with test
  tests.exe = "tools/NUnit/nunit-console.exe" # executable to run tests with
  tests.copy_local # when running from network share
  tests.native_exe # when you don't want to use 'mono' as the native executable on non-windows systems
end

