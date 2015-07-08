require 'fileutils'
require 'version_bumper'
require 'albacore/nuget_model'
require 'albacore/task_types/nugets_pack'

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

SEM_VER = [bumper_version.major, bumper_version.minor, bumper_version.revision].join('.')

def to_nuget_version(v)
	v[1] + v[3]
end

def prepare_nuspec(id, version, has_dependency)
  nuspec = Albacore::NugetModel::Package.new.with_metadata do |m|
      m.id = id
      m.version = version
      m.authors = "Josh Coffman, Tom Marien"
      m.owners = "Sean Chambers"
      m.summary = "FluentMigrator is a database migration framework for .NET written in C#."
      m.description = "FluentMigrator is a database migration framework for .NET written in C#. The basic idea is that you can create migrations which are simply classes that derive from the Migration base class and have a Migration attribute with a unique version number attached to them. Upon executing FluentMigrator, you tell it which version to migrate to and it will run all necessary migrations in order to bring your database up to that version.
      In addition to forward migration support, FluentMigrator also supports different ways to execute the migrations along with selective migrations called profiles and executing arbitrary SQL."
      m.language = "en-US"
      m.project_url = "https://github.com/schambers/fluentmigrator/wiki/"
      m.license_url = "https://github.com/schambers/fluentmigrator/blob/master/LICENSE.txt"
      m.release_notes = "https://github.com/schambers/fluentmigrator/releases"
      if has_dependency
        m.add_dependency 'FluentMigrator', version
      end
    end

  nuspec
end

def add_files_for_fluentmigrator_nuget(nuspec)
  @versions.each do |version|
    nuspec.add_file  "../dist/console-#{version}-AnyCPU/FluentMigrator.dll", "lib/#{to_nuget_version(version)}"
    nuspec.add_file  "../dist/console-#{version}-AnyCPU/FluentMigrator.pdb", "lib/#{to_nuget_version(version)}"
    nuspec.add_file  "../dist/console-#{version}-AnyCPU/FluentMigrator.xml", "lib/#{to_nuget_version(version)}"
  end

  add_tools_files 'x86', 'v4.0', nuspec, true
end

def add_files_for_fluentmigrator_runner_nuget(nuspec)
  @versions.each do |version|
    nuspec.add_file  "../dist/console-#{version}-AnyCPU/FluentMigrator.Runner.dll", "lib/#{to_nuget_version(version)}"
    nuspec.add_file  "../dist/console-#{version}-AnyCPU/FluentMigrator.Runner.pdb", "lib/#{to_nuget_version(version)}"
  end
end

def add_files_for_fluentmigrator_tools_nuget(nuspec)
  nuspec.add_file 'install.ps1', 'tools'
  nuspec.add_file 'InstallationDummyFile.txt', 'content'

  @platforms.each do |platform|
    @versions.each do |version|
      add_tools_files platform, version, nuspec, false
    end
  end
end

def add_tools_files(platform, version, nuspec, is_main_package)
  tools_files_path = "console-#{version}-#{platform}"
  tools_files = FileList['dist/' + tools_files_path + "/**/*.*"].exclude(/Migrate.vshost/)

  nuget_tools_path = is_main_package ? '' : platform + "/#{to_nuget_version(version)}/"

  tools_files.each do |src|
    next if File.directory? src
    nuspec.add_file '../' + src, 'tools/' + nuget_tools_path + src.pathmap("%{^dist\/" + tools_files_path + "/?,}d").to_s
  end
end

def pack_nuget(nuspec_path, nuspec)
  File.write(nuspec_path,nuspec.to_xml)
  cmd = Albacore::NugetsPack::Cmd.new 'tools/NuGet.exe', out: "nuget"
  pkg, spkg = cmd.execute nuspec_path
end

namespace :nuget do
  @platforms = ['x86', 'AnyCPU']
  @versions = ['v3.5', 'v4.0']

  desc 'package nugets - finds all projects and package them'
  task :create_nugets => ['build:solutioninfo', 'build:console'] do
    FileUtils.mkdir_p 'nuget/'
    
    fm_nuspec = prepare_nuspec 'FluentMigrator', SEM_VER, false
    add_files_for_fluentmigrator_nuget fm_nuspec
    pack_nuget 'packages/FluentMigrator.nuspec', fm_nuspec

    fmr_nuspec = prepare_nuspec 'FluentMigrator.Runner', SEM_VER, true
    add_files_for_fluentmigrator_runner_nuget fmr_nuspec
    pack_nuget 'packages/FluentMigrator.Runner.nuspec', fmr_nuspec

    fmt_nuspec = prepare_nuspec 'FluentMigrator.Tools', SEM_VER, true
    add_files_for_fluentmigrator_tools_nuget fmt_nuspec
    pack_nuget 'packages/FluentMigrator.Tools.nuspec', fmt_nuspec
  end
end