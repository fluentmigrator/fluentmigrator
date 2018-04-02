#region License
// Copyright (c) 2018, FluentMigrator Project
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

using FluentMigrator.Runner.Processors.Firebird;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    [TestFixture]
    public class FirebirdOptionsTests
    {
        [TestCase("force quote=true", true)]
        [TestCase("force quote=1", true)]
        [TestCase("force quote=yes", true)]
        [TestCase("force quote=0", false)]
        [TestCase("force quote=false", false)]
        [TestCase("force quote=no", false)]
        public void CanParseFoceQuoteFromProviderOptions(string options, bool expectedValue)
        {
            var fbOpt = new FirebirdOptions().ApplyProviderSwitches(options);
            Assert.AreEqual(expectedValue, fbOpt.ForceQuote);
        }
    }
}
