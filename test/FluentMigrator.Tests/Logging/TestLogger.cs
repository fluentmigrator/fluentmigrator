#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Logging;

using NUnit.Framework;

namespace FluentMigrator.Tests.Logging
{
    /// <summary>
    /// Logger writing the runner log messages to the <see cref="TestContext"/>
    /// </summary>
    public class TestLogger : FluentMigratorRunnerLogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestLogger"/> class.
        /// </summary>
        public TestLogger()
            : base(TestContext.Out, TestContext.Error, new FluentMigratorLoggerOptions() { ShowSql = true })
        {
        }
    }
}
