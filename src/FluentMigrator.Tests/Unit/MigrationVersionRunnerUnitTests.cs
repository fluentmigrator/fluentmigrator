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

using System;
using System.Collections.Generic;
using System.Reflection;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
	[TestFixture]
	public class MigrationVersionRunnerUnitTests
	{
		private Mock<IMigrationConventions> _conventionMock;
		private Mock<IMigrationProcessor> _processorMock;
		private Mock<IMigrationLoader> _loaderMock;
		private MigrationVersionRunner _vrunner;

		[SetUp]
		public void SetUp()
		{
			_conventionMock = new Mock<IMigrationConventions>(MockBehavior.Loose);
			_processorMock = new Mock<IMigrationProcessor>(MockBehavior.Loose);
			_loaderMock = new Mock<IMigrationLoader>(MockBehavior.Loose);

			_vrunner = new MigrationVersionRunner(_conventionMock.Object, _processorMock.Object, _loaderMock.Object);
		}

		[Test]
		public void LoadsCorrectCallingAssembly()
		{
			_vrunner.MigrationAssembly.ShouldBe(Assembly.GetAssembly(typeof(MigrationVersionRunnerUnitTests)));
		}

		[Test]
		public void HandlesNullMigrationList()
		{
			//set migrations to return empty list
			var asm = Assembly.GetAssembly(typeof(MigrationVersionRunnerUnitTests));
			_loaderMock.Setup(x => x.FindMigrationsIn(asm, null)).Returns<IEnumerable<Migration>>(null);


			_vrunner.Migrations.Count.ShouldBe(0);

			_vrunner.MigrateUp();

			_loaderMock.VerifyAll();
		}

		[Test, ExpectedException(typeof(Exception))]
		public void ShouldThrowExceptionIfDuplicateVersionNumbersAreLoaded()
		{
			_loaderMock.Setup(x => x.FindMigrationsIn(It.IsAny<Assembly>(), null)).Returns(new List<MigrationMetadata>
			                                                                         	{
			                                                                         		new MigrationMetadata {Version = 1, Type = typeof(UserToRole)},
			                                                                         		new MigrationMetadata {Version = 2, Type = typeof(FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass2.UserToRole)},
			                                                                         		new MigrationMetadata {Version = 2, Type = typeof(FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass2.UserToRole)}
			                                                                         	});

			_vrunner.MigrateUp();
		}

	    [Test]
        public void HandlesMigrationThatDoesNotInheritFromMigrationBaseClass()
	    {
            _loaderMock.Setup(x => x.FindMigrationsIn(It.IsAny<Assembly>(), null)).Returns(new List<MigrationMetadata>
			                                                                         	{
			                                                                         		new MigrationMetadata {Version = 1, Type = typeof(MigrationThatDoesNotInheritFromMigrationBaseClass)},
			                                                                         	});

            _vrunner.Migrations[1].ShouldNotBeNull();
            _vrunner.Migrations[1].ShouldBeOfType<MigrationThatDoesNotInheritFromMigrationBaseClass>();
	    }

        private class MigrationThatDoesNotInheritFromMigrationBaseClass : IMigration
        {
            public void GetUpExpressions(IMigrationContext context)
            {
                throw new NotImplementedException();
            }

            public void GetDownExpressions(IMigrationContext context)
            {
                throw new NotImplementedException();
            }
        }
	}
}
