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
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class CreateForeignKeyExpressionTests
    {
        [Test]
        public void ToStringIsDescriptive()
        {
            var sql = new CreateForeignKeyExpression
                        {
                            ForeignKey = new ForeignKeyDefinition
                                            {
                                                ForeignColumns = new Collection<string> { "User_id" },
                                                ForeignTable = "UserRoles",
                                                PrimaryColumns = new Collection<string> { "Id" },
                                                PrimaryTable = "User",
                                                Name = "FK"
                                            }
                        }.ToString();
            sql.ShouldBe("CreateForeignKey FK UserRoles(User_id) User(Id)");
        }
    }
}
