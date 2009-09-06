require 'rubygems'

task :default => [:clean, :compile, :test, :run_corflags]

desc 'removes build files'
task :clean do
  FileUtils.rm_rf("build")
end

desc 'compile'
task :compile => :clean do
  msbuild_path = File.join(ENV['windir'].dup, 'Microsoft.NET', 'Framework', 'v3.5', 'msbuild.exe')
  sh "#{msbuild_path} FluentMigratorVS2008.sln /maxcpucount /v:m /property:BuildInParallel=false /property:Configuration=debug /property:Architecture=x86 /t:Rebuild"
  
  exampleToolsDir = 'src/FluentMigrator.Example/tools/FluentMigrator'
  FileUtils.mkdir_p 'build'
  FileUtils.mkdir_p exampleToolsDir
  
  Dir.glob(File.join('src/FluentMigrator.Console/bin/Debug', "*.{dll,pdb,xml,exe}")) do |file|
	copy(file, 'build')
	copy(file, exampleToolsDir)
  end
end

desc 'set the migrator console exe to run in x86'
task :run_corflags do
  sh '"C:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin\CorFlags.exe" build\FluentMigrator.Console.exe /32BIT+'
  sh '"C:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin\CorFlags.exe" src\FluentMigrator.Example\tools\FluentMigrator\FluentMigrator.Console.exe /32BIT+'
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