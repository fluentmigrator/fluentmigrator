require 'rubygems'

task :default => [:compile, :test]

desc 'compile'
task :compile do
  msbuild_path = File.join(ENV['windir'].dup, 'Microsoft.NET', 'Framework', 'v3.5', 'msbuild.exe')
  sh "#{msbuild_path} FluentMigratorVS2008.sln /maxcpucount /v:m /property:BuildInParallel=false /property:Configuration=debug /property:Architecture=x86 /t:Rebuild"
end

desc 'runs tests'
task :test do
  sh 'tools\nunit\nunit-console.exe src\FluentMigrator.Tests\bin\x86\Debug\FluentMigrator.Tests.dll'
end