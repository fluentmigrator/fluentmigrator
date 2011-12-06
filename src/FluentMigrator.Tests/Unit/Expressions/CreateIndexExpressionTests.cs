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

using System.Collections.ObjectModel;
using System.Diagnostics;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
	[TestFixture]
	public class CreateIndexExpressionTests
	{
		[Test]
		public void ToStringIsDescriptive()
		{
			new CreateIndexExpression{ Index = new IndexDefinition
													{
														Columns = new Collection<IndexColumnDefinition>
																		{
																			new IndexColumnDefinition
																				{
																			Name = "Name"
																				},
																	new IndexColumnDefinition
																		{
																			Name = "Slug"
																		}
																		},
													TableName = "Table",
													Name = "NameIndex"
												}
			}.ToString().ShouldBe("CreateIndex Table (Name, Slug)");
			
		}

		[Test]
		public void ShouldDelegateApplyConventionsToIndexDefinition()
		{
			var definitionMock = new Mock<IndexDefinition>();
			var createIndexExpression = new CreateIndexExpression { Index = definitionMock.Object} ;
			var migrationConventions = new Mock<IMigrationConventions>(MockBehavior.Strict).Object;

			definitionMock.Setup(id => id.ApplyConventions(migrationConventions)).Verifiable();

			createIndexExpression.ApplyConventions(migrationConventions);

			definitionMock.VerifyAll();
		}
	}
}
