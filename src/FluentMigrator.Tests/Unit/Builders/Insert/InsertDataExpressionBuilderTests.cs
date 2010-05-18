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

using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Builders.Insert
{
	[TestFixture]
	public class InsertDataExpressionBuilderTests
	{
		[Test]
		public void RowsGetSetWhenRowIsCalled()
		{
			var expression = new InsertDataExpression();

			var builder = new InsertDataExpressionBuilder(expression);
			builder
				.Row(new { Data1 = "Row1Data1", Data2 = "Row1Data2" })
				.Row(new { Data1 = "Row2Data1", Data2 = "Row2Data2" });

			expression.Rows.Count.ShouldBe(2);

			expression.Rows[0][0].Key.ShouldBe("Data1");
			expression.Rows[0][0].Value.ShouldBe("Row1Data1");

			expression.Rows[0][1].Key.ShouldBe("Data2");
			expression.Rows[0][1].Value.ShouldBe("Row1Data2");

			expression.Rows[1][0].Key.ShouldBe("Data1");
			expression.Rows[1][0].Value.ShouldBe("Row2Data1");

			expression.Rows[1][1].Key.ShouldBe("Data2");
			expression.Rows[1][1].Value.ShouldBe("Row2Data2");
		}
	}
}