require 'rubygems'

task :default => [:clean, :compile, :test]

desc 'removes build files'
task :clean do
  FileUtils.rm_rf("build")
end

desc 'compile'
task :compile do
  msbuild_path = File.join(ENV['windir'].dup, 'Microsoft.NET', 'Framework', 'v3.5', 'msbuild.exe')
  sh "#{msbuild_path} FluentMigratorVS2008.sln /maxcpucount /v:m /property:BuildInParallel=false /property:Configuration=debug /property:Architecture=x86 /t:Rebuild"
  
  FileUtils.mkdir_p 'build'
  Dir.glob(File.join('src/FluentMigrator.Console/bin/Debug', "*.{dll,pdb,xml}")) do |file|
    puts file
	copy(file, 'build') 
  end
end

desc 'runs tests'
task :test do
  sh 'tools\nunit\nunit-console.exe src\FluentMigrator.Tests\bin\Debug\FluentMigrator.Tests.dll'
end

desc 'opens the sln file'
task :sln do
  Thread.new do
    devenv = 'C:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\IDE\devenv.exe'
    path = File.join(Dir.pwd, 'FluentMigratorVS2008.sln')
    sh "\"#{devenv}\" #{path}"
  end
end