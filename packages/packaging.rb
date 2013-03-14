require 'fileutils'
require 'version_bumper'

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

FLUENTMIGRATOR_VERSION = bumper_version.to_s

def to_nuget_version(v)
	v[1] + v[3]
end

def copy_files(from, to, filename, extensions)
  extensions.each do |ext|
    FileUtils.cp "#{from}#{filename}.#{ext}", "#{to}#{filename}.#{ext}"
  end
end

def prepare_lib(version)
  output_directory_lib = "./packages/FluentMigrator/lib/#{to_nuget_version(version)}/"
  FileUtils.mkdir_p output_directory_lib
  copy_files "./dist/console-#{version}-AnyCPU/", output_directory_lib, 'FluentMigrator', ['dll', 'pdb', 'xml'] 
end

def prepare_runners(version)
  output_directory_lib = "./packages/FluentMigrator.Runners/lib/#{to_nuget_version(version)}/"
  FileUtils.mkdir_p output_directory_lib
  copy_files "./dist/console-#{version}-AnyCPU/", output_directory_lib, 'FluentMigrator.Runner', ['dll', 'pdb'] 
end

def prepare_inproc(version)
  output_directory_lib = "./packages/FluentMigrator.InProc/lib/#{to_nuget_version(version)}/"
  FileUtils.mkdir_p output_directory_lib
  copy_files "./dist/console-#{version}-AnyCPU/", output_directory_lib, 'FluentMigrator.InProc', ['dll', 'pdb'] 
end

def prepare_tools
  output_directory_tools = './packages/FluentMigrator/tools/'
  FileUtils.mkdir_p output_directory_tools
  cp_r FileList["dist/console-v4.0-x86/*"], output_directory_tools
end

def prepare_tools_package
  output_directory_tools = './packages/FluentMigrator.Tools/tools/'
  output_directory_content = './packages/FluentMigrator.Tools/content/'

  FileUtils.mkdir_p output_directory_content
  FileUtils.mkdir_p output_directory_tools

  copy_files './packages/', output_directory_tools, 'install', ['ps1']
  copy_files './packages/', output_directory_content, 'InstallationDummyFile', ['txt']
  
  @platforms.each do |p|
    FileUtils.mkdir_p output_directory_tools + p + '/'
    @versions.each do |v|
      output_folder = output_directory_tools + p + "/#{to_nuget_version(v)}/"
      FileUtils.mkdir_p output_folder
      cp_r FileList["dist/console-#{v}-#{p}/*"], output_folder
    end
  end
end

namespace :nuget do
  task :clean do
    FileUtils.rm_rf './packages/FluentMigrator/tools/'
    FileUtils.rm_rf './packages/FluentMigrator/lib/'
	
    FileUtils.rm_rf './packages/FluentMigrator.Tools/tools/'
	
	FileUtils.rm_rf './packages/FluentMigrator.Runners/tools/'
	FileUtils.rm_rf './packages/FluentMigrator.InProc/tools/'
    FileUtils.rm_rf './packages/FluentMigratorRunners/lib/'
  end
  
  desc "create the FluentMigrator nuspec file"
  nuspec :create_spec do |nuspec|
     version = "#{ENV['version']}"

     nuspec.id = "FluentMigrator"
     nuspec.version = version.length == 7 ? version : FLUENTMIGRATOR_VERSION
     nuspec.authors = "Josh Coffman"
     nuspec.owners = "Sean Chambers"
     nuspec.description = "FluentMigrator is a database migration framework for .NET written in C#. The basic idea is that you can create migrations which are simply classes that derive from the Migration base class and have a Migration attribute with a unique version number attached to them. Upon executing FluentMigrator, you tell it which version to migrate to and it will run all necessary migrations in order to bring your database up to that version.
  In addition to forward migration support, FluentMigrator also supports different ways to execute the migrations along with selective migrations called profiles and executing arbitrary SQL."
     nuspec.title = "Fluent Migrator"
     nuspec.language = "en-US"
     nuspec.projectUrl = "https://github.com/schambers/fluentmigrator/wiki/"
     nuspec.working_directory = "packages/FluentMigrator"
     nuspec.output_file = "FluentMigrator.nuspec"
  end
  
  desc "create the nuspec file"
  nuspec :create_tools_spec do |nuspec|
     version = "#{ENV['version']}"
     new_version = version.length == 7 ? version : FLUENTMIGRATOR_VERSION
     nuspec.id = "FluentMigrator.Tools"
     nuspec.version = new_version
     nuspec.authors = "Josh Coffman"
     nuspec.owners = "Sean Chambers"
     nuspec.description = "FluentMigrator is a database migration framework for .NET written in C#. The basic idea is that you can create migrations which are simply classes that derive from the Migration base class and have a Migration attribute with a unique version number attached to them. Upon executing FluentMigrator, you tell it which version to migrate to and it will run all necessary migrations in order to bring your database up to that version.
  In addition to forward migration support, FluentMigrator also supports different ways to execute the migrations along with selective migrations called profiles and executing arbitrary SQL."
     nuspec.title = "Fluent Migrator Tools"
     nuspec.language = "en-US"
     nuspec.projectUrl = "https://github.com/schambers/fluentmigrator/wiki/"
     nuspec.working_directory = "packages/FluentMigrator.Tools"
     nuspec.output_file = "FluentMigrator.Tools.nuspec"
	 nuspec.dependency "FluentMigrator", new_version
  end
  
  desc "create the FluentMigrator Runners nuspec file"
  nuspec :create_runner_spec do |nuspec|
     version = "#{ENV['version']}"
     new_version = version.length == 7 ? version : FLUENTMIGRATOR_VERSION
     nuspec.id = "FluentMigrator.Runners"
     nuspec.version = new_version
     nuspec.authors = "Josh Coffman"
     nuspec.owners = "Sean Chambers"
     nuspec.description = "FluentMigrator is a database migration framework for .NET written in C#. The basic idea is that you can create migrations which are simply classes that derive from the Migration base class and have a Migration attribute with a unique version number attached to them. Upon executing FluentMigrator, you tell it which version to migrate to and it will run all necessary migrations in order to bring your database up to that version.
  In addition to forward migration support, FluentMigrator also supports different ways to execute the migrations along with selective migrations called profiles and executing arbitrary SQL."
     nuspec.title = "Fluent Migrator Runners"
     nuspec.language = "en-US"
     nuspec.projectUrl = "https://github.com/schambers/fluentmigrator/wiki/"
     nuspec.working_directory = "packages/FluentMigrator.Runners"
     nuspec.output_file = "FluentMigrator.Runners.nuspec"
	 nuspec.dependency "FluentMigrator", new_version
  end
  
  desc "create the FluentMigrator InProc nuspec file"
  nuspec :create_inproc_spec do |nuspec|
     version = "#{ENV['version']}"
     new_version = version.length == 7 ? version : FLUENTMIGRATOR_VERSION
     nuspec.id = "FluentMigrator.InProc"
     nuspec.version = new_version
     nuspec.authors = "Josh Coffman"
     nuspec.owners = "Sean Chambers"
     nuspec.description = "FluentMigrator is a database migration framework for .NET written in C#. The basic idea is that you can create migrations which are simply classes that derive from the Migration base class and have a Migration attribute with a unique version number attached to them. Upon executing FluentMigrator, you tell it which version to migrate to and it will run all necessary migrations in order to bring your database up to that version.
  In addition to forward migration support, FluentMigrator also supports different ways to execute the migrations along with selective migrations called profiles and executing arbitrary SQL."
     nuspec.title = "Fluent Migrator InProc"
     nuspec.language = "en-US"
     nuspec.projectUrl = "https://github.com/schambers/fluentmigrator/wiki/"
     nuspec.working_directory = "packages/FluentMigrator.InProc"
     nuspec.output_file = "FluentMigrator.InProc.nuspec"
	 nuspec.dependency "FluentMigrator", new_version
  end

  @platforms = ['x86', 'AnyCPU']
  @versions = ['v3.5', 'v4.0']
    
  task :prepare_package => ['build:solutioninfo', 'build:console', :create_spec, :create_tools_spec, :create_runner_spec, :create_inproc_spec, :clean] do
    
    @versions.each do |v|
      prepare_lib v
	  prepare_runners v
	  prepare_inproc v
    end
    
    prepare_tools
    prepare_tools_package
	
	
  end

  task :package => :prepare_package do
    nuget_pack('packages/FluentMigrator/', 'packages/FluentMigrator/FluentMigrator.nuspec')
    nuget_pack('packages/FluentMigrator.Tools/', 'packages/FluentMigrator.Tools/FluentMigrator.Tools.nuspec')   
	nuget_pack('packages/FluentMigrator.Runners/', 'packages/FluentMigrator.Runners/FluentMigrator.Runners.nuspec')   
	nuget_pack('packages/FluentMigrator.InProc/', 'packages/FluentMigrator.InProc/FluentMigrator.InProc.nuspec')   
  end
  
  def nuget_pack(base_folder, nuspec_path)
    cmd = Exec.new  
    output = 'nuget/'
    cmd.command = 'tools/NuGet.exe'
    cmd.parameters = "pack #{nuspec_path} -basepath #{base_folder} -outputdirectory #{output}"
    cmd.execute
  end

end