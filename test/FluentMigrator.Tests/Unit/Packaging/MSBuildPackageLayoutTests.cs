#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Packaging
{
    /// <summary>
    /// Checks, without packing anything, that the <c>build/&lt;tfm&gt;/FluentMigrator.MSBuild.targets</c> files
    /// shipped in the FluentMigrator.MSBuild package point at the task assembly where
    /// <c>PackBuildOutputs</c> actually puts it.
    /// </summary>
    /// <remarks>
    /// Every release up to and including 8.x shipped <c>$(MSBuildThisFileDirectory)..\tasks\...</c>, which
    /// resolves to <c>build/tasks/&lt;tfm&gt;</c> inside the package. The assembly lives in <c>tasks/&lt;tfm&gt;</c> at
    /// the package root, so the auto-imported <c>Migrate</c> task always failed with MSB4062. The
    /// end-to-end check is <c>test/FluentMigrator.MSBuild.PackageSmoke/Invoke-PackageSmoke.ps1</c>, which CI
    /// runs against the packed nupkg.
    /// </remarks>
    [TestFixture]
    [Category("Packaging")]
    public class MSBuildPackageLayoutTests
    {
        private const string TaskName = "FluentMigrator.MSBuild.Migrate";
        private const string TargetsFileName = "FluentMigrator.MSBuild.targets";

        private string _projectDirectory;
        private XDocument _project;

        [OneTimeSetUp]
        public void LoadProject()
        {
            _projectDirectory = Path.Combine(FindRepositoryRoot(), "src", "FluentMigrator.MSBuild");
            _project = XDocument.Load(Path.Combine(_projectDirectory, "FluentMigrator.MSBuild.csproj"));
        }

        [Test]
        public void EveryTargetFrameworkHasBuildTargets()
        {
            Assert.Multiple(
                () =>
                {
                    foreach (var targetFramework in GetTargetFrameworks())
                    {
                        File.Exists(GetTargetsPath(targetFramework)).ShouldBeTrue(
                            $"build/{targetFramework}/{TargetsFileName} is missing, so NuGet will import another framework's targets for {targetFramework} consumers.");
                    }
                });
        }

        [Test]
        public void EveryBuildTargetsFolderIsATargetFramework()
        {
            var targetFrameworks = GetTargetFrameworks();
            var folders = Directory.GetDirectories(Path.Combine(_projectDirectory, "build")).Select(Path.GetFileName);

            folders.ShouldBeSubsetOf(targetFrameworks, "a build/<tfm> folder whose task assembly is never packed");
        }

        [Test]
        public void UsingTaskResolvesToThePackedTaskAssembly()
        {
            var packageRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "FluentMigrator.MSBuild.package"));
            var taskFolderPattern = GetPackedTaskFolderPattern();

            Assert.Multiple(
                () =>
                {
                    foreach (var targetFramework in GetTargetFrameworks())
                    {
                        var targetsPath = GetTargetsPath(targetFramework);
                        if (!File.Exists(targetsPath))
                        {
                            continue; // Reported by EveryTargetFrameworkHasBuildTargets.
                        }

                        var usingTasks = XDocument.Load(targetsPath).Descendants()
                            .Where(e => e.Name.LocalName == "UsingTask" && (string)e.Attribute("TaskName") == TaskName)
                            .ToList();
                        usingTasks.Count.ShouldBe(1, $"{targetFramework}: expected exactly one UsingTask for {TaskName}");
                        if (usingTasks.Count != 1)
                        {
                            continue;
                        }

                        // Inside the package, the targets file sits at build/<tfm>/.
                        var thisFileDirectory = Path.Combine(packageRoot, "build", targetFramework) + Path.DirectorySeparatorChar;
                        var assemblyFile = (string)usingTasks[0].Attribute("AssemblyFile") ?? string.Empty;
                        var resolved = Path.GetFullPath(
                            ToLocalSeparators(assemblyFile.Replace("$(MSBuildThisFileDirectory)", thisFileDirectory)));

                        var expected = Path.GetFullPath(
                            Path.Combine(
                                packageRoot,
                                ToLocalSeparators(taskFolderPattern.Replace("$(TargetFramework)", targetFramework)),
                                "FluentMigrator.MSBuild.dll"));

                        resolved.ShouldBe(expected, $"{targetFramework}: {assemblyFile}");
                    }
                });
        }

        private string[] GetTargetFrameworks()
        {
            var value = _project.Descendants()
                .Where(e => e.Name.LocalName == "TargetFrameworks" && e.Attribute("Condition") == null)
                .Select(e => e.Value)
                .Single();

            return value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();
        }

        /// <summary>
        /// Gets the package path that <c>PackBuildOutputs</c> uses for the task assembly, e.g. <c>tasks\$(TargetFramework)</c>.
        /// </summary>
        private string GetPackedTaskFolderPattern()
        {
            var packBuildOutputs = _project.Descendants()
                .Single(e => e.Name.LocalName == "Target" && (string)e.Attribute("Name") == "PackBuildOutputs");

            return packBuildOutputs.Descendants()
                .Where(e => e.Name.LocalName == "TfmSpecificPackageFile" && (string)e.Attribute("Include") == "$(TargetPath)")
                .Select(e => (string)e.Attribute("PackagePath"))
                .Single();
        }

        private string GetTargetsPath(string targetFramework)
        {
            return Path.Combine(_projectDirectory, "build", targetFramework, TargetsFileName);
        }

        private static string ToLocalSeparators(string path)
        {
            return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "FluentMigrator.sln")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName
                ?? throw new InvalidOperationException($"FluentMigrator.sln not found above {TestContext.CurrentContext.TestDirectory}.");
        }
    }
}
