#region License
// Copyright (c) 2007-2026, Fluent Migrator Project
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
using System.Collections.Generic;
using System.IO;

using FluentMigrator.MSBuild;

using Microsoft.Build.Framework;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Runners
{
    /// <summary>
    /// Runs the <c>Migrate</c> MSBuild task in-process, with a mocked build engine, against this assembly's
    /// <c>ConnectionlessPreview</c> migration.
    /// </summary>
    [TestFixture]
    [Category("Runner")]
    [Category("MSBuild")]
    public class MSBuildMigrateTaskTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void PreviewFromNothingWritesTheVersionTableToTheOutputFile(bool previewFromNothing)
        {
            var outputFileName = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                $"msbuild-previewFromNothing-{previewFromNothing}.sql");
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }

            var messages = new List<string>();
            var buildEngine = new Mock<IBuildEngine>();
            buildEngine.Setup(e => e.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
                .Callback<BuildErrorEventArgs>(e => messages.Add("error: " + e.Message));
            buildEngine.Setup(e => e.LogWarningEvent(It.IsAny<BuildWarningEventArgs>()))
                .Callback<BuildWarningEventArgs>(e => messages.Add("warning: " + e.Message));

            var task = new Migrate
            {
                BuildEngine = buildEngine.Object,
                Database = ProcessorIdConstants.SQLite,
                Connection = "Data Source=:memory:",
                Target = typeof(MSBuildMigrateTaskTests).Assembly.Location,
                Namespace = "FluentMigrator.Tests.Unit.ConnectionlessPreview",
                Task = "migrate:up",
                NoConnection = true,
                PreviewOnly = true,
                PreviewFromNothing = previewFromNothing,
                Output = true,
                OutputFilename = outputFileName,
            };

            try
            {
                var succeeded = task.Execute();

                var log = string.Join(Environment.NewLine, messages);
                succeeded.ShouldBeTrue(log);
                var script = File.ReadAllText(outputFileName);
                script.ShouldContain("CREATE TABLE \"Users\"", customMessage: log);
                script.Contains("CREATE TABLE \"VersionInfo\"").ShouldBe(previewFromNothing);
            }
            finally
            {
                File.Delete(outputFileName);
            }
        }

        [Test]
        public void PreviewFromNothingWithoutPreviewOnlyFails()
        {
            var messages = new List<string>();
            var buildEngine = new Mock<IBuildEngine>();
            buildEngine.Setup(e => e.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
                .Callback<BuildErrorEventArgs>(e => messages.Add(e.Message));

            var task = new Migrate
            {
                BuildEngine = buildEngine.Object,
                Database = ProcessorIdConstants.SQLite,
                Connection = "Data Source=:memory:",
                Target = typeof(MSBuildMigrateTaskTests).Assembly.Location,
                Namespace = "FluentMigrator.Tests.Unit.ConnectionlessPreview",
                Task = "migrate:up",
                NoConnection = true,
                PreviewFromNothing = true,
            };

            task.Execute().ShouldBeFalse();
            string.Join(Environment.NewLine, messages).ShouldContain("PreviewOnly");
        }
    }
}
