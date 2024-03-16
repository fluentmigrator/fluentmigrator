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

using FluentMigrator.Runner.Processors;

using Microsoft.Extensions.Options;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Initialization.Migrations
{
    [Migration(20180517100700, "This one requires a scoped service")]
    public class MigrationWithScopedService : ForwardOnlyMigration
    {
        private readonly ProcessorOptions _processorOptions;

        public MigrationWithScopedService(IOptionsSnapshot<ProcessorOptions> options)
        {
            _processorOptions = options.Value;
        }

        /// <inheritdoc />
        public override void Up()
        {
            Assert.That(_processorOptions?.ConnectionString, Is.Not.Null);
        }
    }
}
