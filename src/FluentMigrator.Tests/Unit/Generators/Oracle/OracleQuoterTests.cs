#region License
// 
// Copyright (c) 2011, Grant Archibald
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
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
   [TestFixture]
   public class OracleQuoterTests
   {
      [Test]
      public void CanQuoteBinaryValue()
      {
         // Arrange
         var quoter = new OracleQuoter();
         // Act

         var value = quoter.QuoteValue(System.Text.Encoding.ASCII.GetBytes("Foo"));

         // Assert
         value.ShouldBe("hextoraw('466F6F')");
      }

      [Test]
      public void CanQuoteGuid()
      {
         // Arrange
         var quoter = new OracleQuoter();

         // Act

         var value = quoter.QuoteValue(new Guid("30e16f6e-d8e2-45b9-97aa-9ef500fa1e9b"));

         // Assert
         value.ShouldBe("hextoraw('30E16F6ED8E245B997AA9EF500FA1E9B')");
      }

      [Test]
      public void CanQuoteNull()
      {
         // Arrange
         var quoter = new OracleQuoter();

         // Act

         var value = quoter.QuoteValue(null);

         // Assert
         value.ShouldBe("NULL");
      }

      [Test]
      public void EmptyStringForOpenQuote()
      {
         // Arrange
         var quoter = new OracleQuoter();
         // Act

         // Assert
         quoter.OpenQuote.ShouldBe(string.Empty);
      }

      [Test]
      public void EmptyStringForCloseQuote()
      {
         // Arrange
         var quoter = new OracleQuoter();

         // Act

         // Assert
         quoter.CloseQuote.ShouldBe(string.Empty);
      }
   }
}
