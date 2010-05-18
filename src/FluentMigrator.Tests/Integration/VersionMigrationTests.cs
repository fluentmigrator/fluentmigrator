#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.VersionTableInfo;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration
{
	[TestFixture]
	public class VersionMigrationTests : IntegrationTestBase
	{
		[Test]
		public void CanUseVersionInfo()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var runner = new MigrationRunner(new MigrationConventions(), processor, new TextWriterAnnouncer(System.Console.Out), new StopWatch());

					IVersionTableMetaData tableMetaData = new DefaultVersionTableMetaData();

					//ensure table doesn't exist
					if (processor.TableExists(tableMetaData.TableName))
						runner.Down(new VersionMigration(tableMetaData));

					runner.Up(new VersionMigration(tableMetaData));
					processor.TableExists(tableMetaData.TableName).ShouldBeTrue();

					runner.Down(new VersionMigration(tableMetaData));
					processor.TableExists(tableMetaData.TableName).ShouldBeFalse();
				});

		}
	}
}
