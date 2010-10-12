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
using System.Collections.ObjectModel;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
	[TestFixture]
	public class DeleteIndexExpressionTests
	{
		[Test]
		public void ToStringIsDescriptive()
		{
			new DeleteIndexExpression
			{
				Index = new IndexDefinition
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
			}.ToString().ShouldBe("DeleteIndex Table (Name, Slug)");
		}
	}
}